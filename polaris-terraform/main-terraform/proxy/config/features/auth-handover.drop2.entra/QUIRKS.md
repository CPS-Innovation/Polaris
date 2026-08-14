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

### E3. 🟠 id-token cookie: `HttpOnly` + host-only (IE cookie-jar hygiene)

The reference set the id-token cookie **without** `HttpOnly` and on `Domain=.cps.gov.uk`,
because it wasn't sure which subdomain would read it (a client script via `document.cookie`, or
a relay). Our consumer is the case-locking **`presence-jsonp`** endpoint, which reads it
**server-side** — so:
- **`HttpOnly`** (safer; no script can read it), and
- **host-only** (no `Domain` attribute): setter (this callback) and reader (presence-jsonp) are
  the same polaris host, and nothing cross-subdomain reads it. Host scope keeps the ~1.5–2 KB JWT
  **out of the shared `cps.gov.uk` jar**, which in IE mode is already crowded with CMS / BIG-IP /
  LB / auth cookies — a full jar makes IE silently evict cookies (LRU), potentially a CMS session
  one. `Path=/global-components/presence-jsonp` further keeps it out of every other request's
  `Cookie` header; `Max-Age=43200` (~12h).

Requires the AD callback host (`ENTRA_REDIRECT_URI`) and the JSONP/browse host to be the **same**
polaris host (they are in this model). If a cross-subdomain consumer or multi-hostname flow
appears, revisit. Worth guarding token size too: a groups-heavy id_token can approach the 4 KB
per-cookie limit.

### E4. 🟠 Callback path is dictated by the registered redirect URI

`/init-v2/callback` is not our naming preference — it is the path baked into the reused
reference app registration's redirect URI (`ENTRA_REDIRECT_URI`). AD only redirects to a
**registered** URI, so the location path and `ENTRA_REDIRECT_URI` must agree. To re-path
(e.g. `/init-entra/callback`) you must register the new URI on the app reg (portal, no
terraform) and change **both** the conf location and the env var together.

---

## Pre-production hardening (must fix before prod)

### E5. 🔴 `_rand` falls back to `Math.random`

State/nonce generation uses Web Crypto `getRandomValues` when present, falling back to
`Math.random` (NOT cryptographically secure) — carried over from the reference POC.
Confirm the deployed njs exposes `crypto.getRandomValues` and **remove the fallback**
(or fail closed) before prod. Pinned by the unit test's length/shape check only, not its
randomness.

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

The `entra_auth_state` and id-token cookies use `SameSite=Lax`. That is fine while the
iframe is embedded **same-site** (CMS + proxy both under `cps.gov.uk`). A cross-site
embed would need `SameSite=None; Secure`. Revisit if the iframe host changes.

### E10. Store backend migration (the seam)

`store.js` deposits via a single `deposit(oid, payload, tokens)` binding
(`tableStorageDeposit` today). The planned MDS-API endpoint is a drop-in `apiEndpointDeposit`
(POST + `Authorization: Bearer <accessToken>`) — swap the one binding, no auth-flow change.
The callback already passes the AD tokens through (Table Storage ignores them).

### E11. 🔴 id-token cookie only works when set in IE mode (IE/Edge jar split) + carries the DEV token today

Two coupled realities for the presence handover:

**IE vs Edge cookie jars are separate** (WinINet vs Chromium), with no sharing absent IT policy.
The presence-jsonp reads happen in the **IE-mode** proxied CMS shell → the IE jar. So the
`cms-auth-id-token` cookie is only usable if it was **set by an IE-mode page**. The **top-level**
`/polaris` flow coerces to **Edge** at `/init`, so its cookie lands in the Chromium jar —
**invisible** to presence. Only the **framed** flow (hidden iframe spawned by the IE-mode CMS
shell) sets an IE-jar cookie. The iframe cannot leave IE mode (document mode is tab-level), so it
stays IE — **but confirm `/init`'s `coerce(edge, reject=true)` doesn't 402/302-loop the IE-locked
iframe before it reaches `/init-entra`** (if it does, the framed entry must skip that Edge gate,
à la the reference's framed-stays-IE design). The store deposit is unaffected (server-side).

**The cookie carries the DEV token today** (`PRESENCE_COOKIE_TOKEN`), not the real id_token — a
proving stopgap so the full round-trip (set in IE jar → presence-jsonp reads → backend accepts)
can be verified before the backend validates real Entra id-tokens. The real idToken still goes to
the store deposit. Swap `PRESENCE_COOKIE_TOKEN` → the real `idToken` in `_succeed` when ready
(and drop the dev-bearer fallback in case-locking — CL7). Kept in sync with case-locking's
`_PRESENCE_DEV_BEARER`.
