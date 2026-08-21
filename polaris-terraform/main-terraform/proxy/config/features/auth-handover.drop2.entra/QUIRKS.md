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

### E2. 🟠 Callback location sheds `X-Frame-Options: DENY` on purpose (QUIRK B3)

The server sets `add_header X-Frame-Options "DENY" always`. The iframe **expansion**
variant needs its terminal page to render inside the CMS shell, so the
`/init-v2/callback` location sets its own `add_header` (a scoped
`Content-Security-Policy: frame-ancestors https://*.cps.gov.uk`). By QUIRK
[B3](../../QUIRKS.md), a location with any `add_header` drops the inherited server set —
which is exactly what removes the `DENY` here. Harmless on the top-level 302 (a redirect
renders nothing). The CSP keeps framing limited to `*.cps.gov.uk`.

### E3. ⚪ (removed) presence id-token cookie

drop2 used to set a browser-side `cms-auth-id-token` cookie (host-only, `HttpOnly`,
`Path=/global-components/presence-jsonp`) carrying a DEV token, read by the case-locking
`presence-jsonp` endpoint. That whole presence path was experimental and has been removed
(both the cookie here and the case-locking consumer). The real id_token still goes to the
store deposit (E10). Kept as a numbered stub so E-numbers stay stable.

### E4. 🟠 Callback path is dictated by the registered redirect URI

`/init-v2/callback` is not our naming preference — it is the path baked into the reused
reference app registration's redirect URI (`ENTRA_REDIRECT_URI`). AD only redirects to a
**registered** URI, so the location path and `ENTRA_REDIRECT_URI` must agree. To re-path
(e.g. `/init-entra/callback`) you must register the new URI on the app reg (portal, no
terraform) and change **both** the conf location and the env var together.

---

## Pre-production hardening (must fix before prod)

### E5. ✅ (resolved) `_rand` no longer uses `Math.random`

State/nonce generation now uses **only** Web Crypto `crypto.getRandomValues` (present in
njs 0.8.5) — the insecure `Math.random` fallback was removed (it is not a CSPRNG; also
flagged by SonarQube). drop1's correlation-id `_uuid` was moved to the same source, so the
config uses no `Math.random` anywhere.

### E6. 🔴 `js_fetch_verify off` on the AD + storage fetches

The callback disables TLS verification for the `ngx.fetch` calls to
`login.microsoftonline.com` and `*.table.core.windows.net` (mirrors drop1's loopback
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
Threading that marker through `/polaris → /init → /auth-refresh-inbound`, and the small
JS harness that opens/destroys the hidden iframe in CMS Classic, are **future** work —
deliberately not built here (the top-level flow is the main event).

### E9. Framed-cookie `SameSite`

The `entra_auth_state` cookie uses `SameSite=Lax`. That is fine while the iframe is
embedded **same-site** (CMS + proxy both under `cps.gov.uk`). A cross-site embed would
need `SameSite=None; Secure`. Revisit if the iframe host changes.

### E10. Store backend migration (the seam)

`store.js` deposits via a single `deposit(oid, payload, tokens)` binding
(`tableStorageDeposit` today). The planned MDS-API endpoint is a drop-in `apiEndpointDeposit`
(POST + `Authorization: Bearer <accessToken>`) — swap the one binding, no auth-flow change.
The callback already passes the AD tokens through (Table Storage ignores them).

### E11. ⚪ (removed) presence id-token cookie IE/Edge jar handover

Was the analysis of getting the `cms-auth-id-token` cookie into the IE-mode jar for the
presence-jsonp reader, plus the DEV-token proving stopgap. The presence path (this cookie +
the case-locking consumer) has been removed as experimental — see E3. Kept as a numbered stub
so E-numbers stay stable. If presence is revived, this IE/Edge-jar reality (WinINet vs Chromium
jars are separate; a top-level `/polaris` flow coerced to Edge lands the cookie invisibly to an
IE-mode reader) is the first thing to re-solve.
