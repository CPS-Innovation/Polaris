# QUIRKS — auth-handover.drop2.entra

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key and the whole-set index.

**NEW-GEN, outside the golden master.** Like drop1, this feature is dormant unless its
switch (`ENTRA_STORE_ENABLED`) is on, so it changes nothing in the before/after parity
suite. The entries below are therefore not `QUIRK:`-pinned against the live monolith;
they are **design decisions and pre-production hardening items** for the new Entra flow.
The `E`-series numbering is local to this feature.

---

## Decisions (deliberate, in place)

### E1. 🟠 State secrets live in a first-party cookie, not the OAuth `state` param

The plan floated carrying state in the OAuth `state` param (to dodge the reference's
IE-jar "Missing State" issue for framed flows). We **did not** do that for the payload:
the packed state holds the **CMS cookies + modern token**, which are session secrets.
The `state` param is echoed to `login.microsoftonline.com` in the query string and comes
back the same way — so it lands in Microsoft's logs, the browser history and any
`Referer`. Putting session secrets there would leak them to a third party.

So only a **random anti-CSRF handle** travels in `state`; the secret payload stays in a
first-party, `HttpOnly`, `Secure`, path-scoped `entra_auth_state` cookie
(`auth-handover.drop2.entra.js`), validated against the handle on callback. This is the
reference's model and the OIDC-correct one.

**Integrity (not just confidentiality):** `HttpOnly` stops script *reads* but not cross-subdomain
*writes* — a sibling `*.cps.gov.uk` origin can cookie-toss a `Domain=cps.gov.uk` `entra_auth_state`.
Since the `state`-param check compares the query param against `st.s` read from that same cookie, a
forged cookie satisfies it (attacker controls both sides) and then injects `cc`/`tok`/`ui` →
session fixation + open redirect. So the cookie is **HMAC-SHA256-signed** with a server-side secret
(`ENTRA_STATE_HMAC_SECRET`); `_unpackState` verifies (constant-time) before use and **fails closed
when the secret is unset** — a forged/tampered cookie is rejected and the callback degrades to drop1.

### E2. 🟠 Callback location sheds `X-Frame-Options: DENY` on purpose (QUIRK B3)

The server sets `add_header X-Frame-Options "DENY" always`. The iframe **expansion**
variant needs its terminal page to render inside the CMS shell, so the
`/init-entra/callback` location sets its own `add_header` (a scoped
`Content-Security-Policy: frame-ancestors https://*.cps.gov.uk`). By QUIRK
[B3](../../QUIRKS.md), a location with any `add_header` drops the inherited server set —
which is exactly what removes the `DENY` here. Harmless on the top-level 302 (a redirect
renders nothing). The CSP keeps framing limited to `*.cps.gov.uk`.

### E3. ⚪ (removed) presence id-token cookie

drop2 used to set a browser-side `cms-auth-id-token` cookie (host-only, `HttpOnly`,
`Path=/global-components/presence-jsonp`) carrying a DEV token, read by the case-locking
`presence-jsonp` endpoint. That whole presence path was experimental and has been removed
(both the cookie here and the case-locking consumer). The real id_token still goes to the
store deposit (E10). Kept as a numbered stub so E-numbers stay stable. **Superseded 2026-10:**
drop2 sets the presence cookie again — now `cms-auth-presence-token` (global-components' existing
contract), carrying the NEUTRAL **access** token, on the iframe terminal only, Max-Age = token
lifetime. See PLAN-entra-unified-flow.

### E4. 🟠 Callback path must be a registered redirect URI

The callback is `/init-entra/callback` (was `/init-v2/callback`, borrowed from
global-components' cms-auth-v2 (since renamed cms-auth-presence) — renamed so the two can load in the same server; that
flow is now `/init-presence/*`). AD only redirects to a **registered** URI, so
`https://<host>/init-entra/callback` must be registered (web redirect URIs, portal, no
terraform) on the reused app reg **for every host** drop2 runs on. The redirect_uri is
derived per-request from the Host (`_redirectUri`), so the njs `CALLBACK_PATH` and the
conf location are the only two places to keep in step; `ENTRA_REDIRECT_URI` in
`cmsproxy.mock.env` is vestigial (nothing reads it).

---

## Pre-production hardening (must fix before prod)

### E5. ✅ (resolved) `_rand` no longer uses `Math.random`

State/nonce generation now uses **only** Web Crypto `crypto.getRandomValues` (present in
njs 0.8.5) — the insecure `Math.random` fallback was removed (it is not a CSPRNG; also
flagged by SonarQube). drop1's correlation-id `_uuid` was moved to the same source, so the
config uses no `Math.random` anywhere.

### E6. 🔴 `js_fetch_verify off` on the AD + MDS fetches

The callback disables TLS verification for the `ngx.fetch` calls to
`login.microsoftonline.com` (code exchange + on-behalf-of) and MDS (mirrors drop1's loopback
setup). For **external** endpoints this should be `on` with a trusted CA bundle
(`js_fetch_trusted_certificate`). Fix before prod.

### E7. ⚪ njs `crypto` + `Buffer` dependency (new in this repo's config)

`store.js` is the first config module to use njs's built-in `crypto` (`createHmac`) and
`Buffer` (base64). No other feature does. Confirm the deployed njs build provides both
(the reference relies on the same, so this is expected) — a smoke test of the SharedKeyLite
signature against a known key/date is the cheapest check.

---

## Open threads (not this drop)

### E8. `terminal=iframe` harness wiring

The core supports both modes; `handleInitEntra` reads `terminal=iframe` off the request.
**Threading (done 2026-10):** with no `r`, `/init`'s shim copies `terminal` into the synthesised
`r=/auth-refresh-inbound?…` (an explicit `r` must include it itself), and the rewrite to
`/init-entra` keeps the query. `/init` **skips its Edge gate** when `terminal=iframe`: the iframe
lives in the CMS Classic IE-mode tab, cannot change mode (coercion would loop or 402), and must
stay in IE mode so the presence cookie lands in the IE jar (`auth-handover.js` appAuthRedirect;
unit-pinned). **Still future:** the small JS harness in CMS Classic that opens/destroys the
hidden iframe at login (global-components' Classic client, step 2 of the plan).

### E9. Framed-cookie `SameSite`

The `entra_auth_state` cookie uses `SameSite=Lax`. That is fine while the iframe is
embedded **same-site** (CMS + proxy both under `cps.gov.uk`). A cross-site embed would
need `SameSite=None; Secure`. Revisit if the iframe host changes.

### E10. Store backend migration (the seam)

**Done 2026-10: the backend is MDS.** `store.js` exports `scope` (`ENTRA_MDS_SCOPE`) and
`deposit(payload, bearer)` = `mdsDeposit`: `PUT <WM_MDS_BASE_URL>cms-auth-store` with
`Authorization: Bearer <OBO token>` + `x-functions-key` (the same two settings the
global-components `/global-components/api` route uses — not that route itself, which strips
Authorization) and body `{cookies, token, expiryTime: "2000-01-01T00:00:00Z"}` (fixed; MDS ignores
it today — revisit if it ever honours it). MDS takes the user from the token's `oid`; today it
checks nothing else, full validation arrives later as MDS middleware. The Table Storage backend
(SharedKeyLite, `ENTRA_STORAGE_*`, rows keyed by an id_token OID) is removed — git history has it.
The swap needs the MDS permission on our app reg **plus admin consent** (CPS blocks user consent);
until then the on-behalf-of call fails (AADSTS65001) and drop2 degrades — safe.

**⚠ Interim switch `MDS_SEND_NEUTRAL_TOKEN` (2026-10-06, a config constant in
`auth-handover.drop2.entra.js`, NOT an app setting) — currently ON:** skips the swap and sends the
NEUTRAL token to MDS, so deposits work on QA before admin consent (MDS only reads `oid` today). Costs: MDS
receives a token that is, today, a valid presence-API credential (aud `8d6133af`, scp
`api.presence.user.readwrite`), and it stops working when MDS adds validation. **Turn OFF once admin
consent is granted; never ship beyond QA with it on.** Unit-pinned (both sides + the shipped value).

### E11. ⚪ (removed) presence id-token cookie IE/Edge jar handover

Was the analysis of getting the `cms-auth-id-token` cookie into the IE-mode jar for the
presence-jsonp reader, plus the DEV-token proving stopgap. The presence path (this cookie +
the case-locking consumer) has been removed as experimental — see E3. Kept as a numbered stub
so E-numbers stay stable. If presence is revived, this IE/Edge-jar reality (WinINet vs Chromium
jars are separate; a top-level `/polaris` flow coerced to Edge lands the cookie invisibly to an
IE-mode reader) is the first thing to re-solve.
