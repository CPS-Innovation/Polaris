# QUIRKS — auth-handover.drop2.entra

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key and the whole-set index.

**NEW-GEN, outside the golden master.** Like drop1, this feature is dormant unless its
switch (`ENTRA_STORE_ENABLED`) is on — on in dev, QA and UAT (`proxy_entra.store_enabled`),
off in prod and in the parity suite — so it changes nothing in the before/after parity
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
*writes* — a sibling `*.cps.gov.uk` origin can cookie-toss a `Domain=cps.gov.uk` state cookie.
Since the `state`-param check compares the query param against `st.s` read from that same cookie, a
forged cookie satisfies it (attacker controls both sides) and then injects `cc`/`tok`/`ui` →
session fixation + open redirect. So the cookie is **HMAC-SHA256-signed** with a server-side secret
(`ENTRA_STATE_HMAC_SECRET`); `_unpackState` verifies (constant-time) before use and **fails closed
when the secret is unset** — a forged/tampered cookie is rejected and the flow carries on as plain
`/init` (or ends the iframe on the terminal page).

**Replay and parallel flows (2026-10-19):** the HMAC stops forging, not replaying a real cookie. The
signed payload therefore also carries its issue time (`iat`) and host (`h`); `_loadState` rejects a
cookie older than 300 s (60 s forward skew allowed) or minted for another host
(`expired-state` / `wrong-host`). And the cookie is **one per flow**, named by its random handle
(`entra_auth_state_<handle>`): the OAuth `state` is `<t|i>.<handle>`, the callback reads only the
cookie that handle names and clears only that one. Before this, one shared cookie meant two
handovers in flight in one browser (an OS link and the P button in two tabs) overwrote each other:
the first callback then finished on the second's destination and cleared it, sending the second to
the Polaris UI fallback.

**Code injection: PKCE (2026-10-22).** `state` binds the callback to *a* flow, not to the code: a
code stolen from another user's flow could be replayed to the callback alongside the attacker's own
state cookie and param, and we'd redeem it (we hold the client secret), depositing the attacker's
`cc`/token under the victim's oid. So each flow also carries a PKCE verifier (`v`, 64 hex chars,
fresh per flow) in its signed state; `/authorize` gets only its S256 challenge (every leg of the
flow: silent, hinted, picker), and the exchange sends the verifier, which Entra checks against the
challenge the code was issued with. A replayed code meets the wrong verifier and the exchange fails
(`token-exchange-failed`, carries on). No nonce: the id_token is only read for the hint, never
trusted for identity, and PKCE already binds the code to this browser's flow. A state from before
PKCE (no `v`) exchanges without a verifier. Adds ~90 bytes to the state cookie (E13's size guard).

### E2. 🟠 Callback location sheds `X-Frame-Options: DENY` on purpose (QUIRK B3)

The server sets `add_header X-Frame-Options "DENY" always`. The iframe **expansion**
variant needs its terminal page to render inside the CMS shell, so the
`/init/entra-callback` location sets its own `add_header` (a scoped
`Content-Security-Policy: frame-ancestors https://*.cps.gov.uk`). By QUIRK
[B3](../../QUIRKS.md), a location with any `add_header` drops the inherited server set —
which is exactly what removes the `DENY` here. Harmless on the top-level 302 (a redirect
renders nothing). The CSP keeps framing limited to `*.cps.gov.uk`.

### E3. ⚪ (removed) presence id-token cookie

drop2 used to set a browser-side `cms-auth-id-token` cookie (host-only, `HttpOnly`,
`Path=/global-components/presence-jsonp`) carrying a DEV token, read by the case-locking
`presence-jsonp` endpoint. That whole presence path was experimental and has been removed
(both the cookie here and the case-locking consumer). No id_token is used anywhere now: each
backend identifies the user from its own token (E10). Kept as a numbered stub so E-numbers stay
stable. **Superseded 2026-10:**
drop2 sets the presence cookie again — now `cms-auth-presence-token` (global-components' existing
contract), carrying a **watchdog-scope** access token (a refresh-token swap for
`ENTRA_WATCHDOG_SCOPE`; none if that is empty), on the iframe terminal only, Max-Age = that token's
lifetime. Decided 2026-10-15 (the token model in `auth-handover.drop2.entra.js`).

### E4. 🟠 Callback path must be a registered redirect URI

The callback is `/init/entra-callback` (R0, 2026-10-16; was `/init-entra/callback`, and before
that `/init-v2/callback`, borrowed from global-components' cms-auth-v2 — since renamed
cms-auth-presence, whose flow is `/init-presence/*`). It is an **exact-match** location, so the
prefix `location /init` never catches it (integration-pinned). AD only redirects to a
**registered** URI, so `https://<host>/init/entra-callback` must be registered (web redirect URIs,
portal, no terraform) on our app reg **for every host** drop2 runs on; an unregistered host is the
one misconfiguration that strands users on a Microsoft error page. The redirect_uri is derived
per-request from the Host (`_redirectUri`), so the njs `CALLBACK_PATH` and the conf location are
the only two places to keep in step. The state cookie is `Path=/init/entra-callback`, so it never
rides on other `/init` requests.

### E12. 🟠 The Entra step lives in `/init`, for every destination (R0, 2026-10-16)

`/init` is the one door every app's CMS handover passes through (Polaris, OutSystems, LCC,
housekeeping, …), so drop2 runs there: after the whitelist check and the cookie filter,
`appAuthRedirect` calls `beginFromInit`, which looks the CMS session up (drop1's non-redirecting
`lookupCmsSession`) and starts the silent Entra round trip. The callback deposits to MDS and then
**carries on exactly as `/init` would have**: 302 to `r` with `cc`. No destination's contract
changes; each app can later stop reading `cc` and use bearer + MDS lookup on its own schedule.
`/auth-refresh-inbound` is Polaris's landing only (drop1 or DDEI); `/init-entra` is gone.

- **Failure carries on.** `beginFromInit` resolves false without responding on any failure (not
  enabled, not fully configured, no service scope to swap for, no CMS cookies, lookup failed, state
  too large, an exception) and `/init` does its plain 302. Every callback failure with a
  recoverable state carries on to `r` too, including one that is ours but too old (slow on the
  picker or MFA): it is never redeemed (no exchange, no deposit, no retry), but the user carries on
  to its `r` with `cc`, exactly as `/init` did before drop2 (`degraded expired-state`, real `dest`).
  The one residual case: a top-level callback whose state cookie is lost (typically the back button
  after the flow finished), tampered, future-dated or from another host has no `r`, so the user
  lands on the Polaris UI (`dest=unknown`). (The iframe, known from the `i.` state prefix, still
  gets the terminal page.)
- **The hidden iframe never falls through.** With `terminal=iframe`, "carry on" means the static
  `/init/terminal` page, not `r` (E8).
- **Configured-or-nothing.** With the switch on but any of tenant / client id / client secret /
  HMAC key missing, `/init` never starts the round trip (`isConfigured`, read per request).
- **Token pass-through.** The CMS token looked up at `/init` is appended to the continuation for
  destinations that can use it: our own `/auth-refresh-inbound` on this host (`cms-modern-token` +
  `cms-version-id`; drop1 then skips discover/mint/verify) and OutSystems handover pages
  (`…/auth-handover.html` with `stage=os-cookie-return`: `cms-modern-token`; global-components'
  os-handover then skips its `/auth-refresh-cms-modern-token` bounce — shipped there first). All
  other destinations get exactly today's `r` + `cc`. It is passed on degrades too: it was verified
  at `/init`, independently of Entra.
- **Cost.** Every app's handover gains the Entra round trip plus the CMS lookup and the MDS PUT
  when the switch is on (not yet measured). A Polaris handover no longer looks the token up twice.

### E13. 🟢 State-cookie size guard, and one `drop2-outcome` line per handover (2026-10-16)

**Size guard.** Browsers silently drop a cookie over ~4 KB, and losing the state cookie loses
the destination (E12's one residual case). The top-level state holds `r` + `cc` + token (the
iframe's carries no `r`), and OutSystems `r` URLs are long, so `/init` measures the packed cookie and, above 3800 bytes, skips Entra and
carries on as today (`skipped` / `state-too-large`, with `bytes=` but never the content). Tune the
threshold from real sizes: the `bytes=` field gives them.

**Outcome line.** Exactly one line per handover drop2 handled, to the error log, so it lands in
`AppServiceConsoleLogs` (Polaris `global_la`, via the proxy's existing diagnostic setting):

```
drop2-outcome result=ok|degraded|skipped reason=<code> term=top-level|iframe dest=polaris|os|other|unknown
```

`skipped` is written by `/init` when it never starts Entra (`not-configured`, `no-mds-scope`,
`no-service-scope`, `no-cookies`, `cms-lookup-failed`, `state-too-large`, `entra-begin-error`);
`ok` / `degraded` by the callback (`ok`, or `presence-only` for an iframe flow in an env with a
watchdog scope but no MDS scope; degraded: `bad-state-param`, `ad-error`, `missing-state`, `bad-state`,
`expired-state`, `wrong-host`, `no-code`, `token-exchange-failed`,
`no-refresh-token`, `swap-failed`, `store-deposit-failed`, `callback-error`). Codes only: no token,
cookie, URL or identity; the free-text detail stays in the separate `drop2 entra degrade — …`
line. Silent when drop2 is off. Rate by outcome, per destination kind:

```kusto
AppServiceConsoleLogs
| where ResultDescription has "drop2-outcome"
| parse kind=relaxed ResultDescription with * "drop2-outcome result=" result " reason=" reason " term=" term " dest=" dest " " *
| summarize n = count() by bin(TimeGenerated, 1h), result, reason, dest
```

**Why AD said no.** `reason=ad-error` lines add `ad=<error> aadsts=<number|none>`, from the
callback's `error` and the `AADSTSnnnnn` prefix of `error_description`. Both are attacker-supplied
query params, so they're reduced to `[a-z_]{1,40}` (else `other`) and digits (else `none`). The
number is what separates the cases `error` alone lumps together:

| `ad` | `aadsts` | Meaning |
| --- | --- | --- |
| `login_required` | 50058 | No Entra session in this browser (the "no Entra" case) |
| `interaction_required` | 16000 | Several signed-in accounts, AD wants the user to pick one (the "multiple accounts" case) |
| `interaction_required` | 50076 / 50079 | MFA (or MFA registration) needed |
| `interaction_required` | 53000 / 53003 | Conditional Access: unmanaged device / blocked |
| `consent_required` | 65001 | The app lacks consent for a requested scope |

```kusto
AppServiceConsoleLogs
| where ResultDescription has "drop2-outcome" and ResultDescription has "reason=ad-error"
| extend ad = extract(@" ad=(\S+)", 1, ResultDescription),
         aadsts = extract(@" aadsts=(\S+)", 1, ResultDescription),
         term = extract(@" term=(\S+)", 1, ResultDescription)
| summarize n = count() by bin(TimeGenerated, 1d), ad, aadsts, term
```

Neither field says *which* user. For anecdata, match a time and a CMS user from support reports;
the line deliberately carries no identity. Also invisible: the wrong account silently winning
(`result=ok`; seen in the dev proof with an admin account).

The alert on it (an Azure Monitor scheduled query) is a separate item: it needs an action group
and a threshold agreed.

---

## Pre-production hardening (must fix before prod)

### E5. ✅ (resolved) `_rand` no longer uses `Math.random`

State-handle / PKCE-verifier generation now uses **only** Web Crypto `crypto.getRandomValues` (present in
njs 0.8.5) — the insecure `Math.random` fallback was removed (it is not a CSPRNG; also
flagged by SonarQube). drop1's correlation-id `_uuid` was moved to the same source, so the
config uses no `Math.random` anywhere.

### E6. ✅ (resolved 2026-10-20) TLS verification on the AD + MDS fetches

The callback's `ngx.fetch` calls to `login.microsoftonline.com` (code exchange + refresh-token
swaps) and MDS used to run with `js_fetch_verify off` (copied from drop1's `$host` loopback). Now
verified: `js_fetch_trusted_certificate /etc/ssl/certs/ca-certificates.crt` (the image's own CA
bundle; present in prod's `nginx:latest` and the alpine test image). Proven in both images: AD and
the MDS host verify, a self-signed host is refused. `js_fetch_timeout 10s` on the callback and on
`/init` (its CMS lookups) bounds each call; njs's default is 60s. `/init`'s CMS loopbacks keep
`js_fetch_verify off` (the `$host` loopback, as drop1).

### E7. ✅ (resolved) njs `crypto` + `Buffer` dependency (new in this repo's config)

`auth-handover.drop2.entra.js` is the only config module to use njs's built-in `crypto`
(`createHmac`, for the state-cookie HMAC) and `Buffer` (base64url). No other feature does
(`store.js` used both for the Table Storage SharedKeyLite signature; that backend is gone, E10).
Proven on QA 2026-10-06: callbacks verified the signed state cookie and reached the MDS deposit.

---

## Open threads (not this drop)

### E8. `terminal=iframe` harness wiring

The core supports both modes; `/init` (appAuthRedirect) passes `terminal=iframe` to
`beginFromInit`, which carries it in the state; the callback then renders the terminal page
instead of carrying on to `r`. **If drop2 does not start** (off, unconfigured, no CMS session,
lookup failed, state too large), `/init` 302s the iframe to the static, frameable
`/init/terminal` page instead of carrying on: the hidden frame must never load the Polaris UI.
(A page rendered by `/init` itself would carry the server's `X-Frame-Options: DENY`; the
`/init/terminal` location sheds it like the callback, E2.) An iframe callback whose state cookie
is lost still knows it is the iframe from the OAuth `state` prefix (`i.<handle>`, echoed by Entra
even on errors) and renders the terminal page (E1). `/init` **skips its Edge gate** when `terminal=iframe`: the iframe
lives in the CMS Classic IE-mode tab, cannot change mode (coercion would loop or 402), and must
stay in IE mode so the presence cookie lands in the IE jar (`auth-handover.js` appAuthRedirect;
unit-pinned). **Still future (step 2), and needed before global-components' Classic client switches from
`/polaris-presence` to `/polaris?terminal=iframe`:**
- the harness that opens/destroys the hidden iframe at login **and re-runs it every ~55 min**:
  drop2's presence cookie lives as long as its token (~60–90 min), whereas global-components'
  `/init-presence` cookie lasts 8 h (the watchdog doesn't check expiry yet);
- framing in IE mode: IE ignores the CSP `frame-ancestors`, and the location sheds
  `X-Frame-Options: DENY`, so in IE mode the terminal/callback pages are frameable by anyone
  (static pages, low risk; global-components uses `X-Frame-Options: SAMEORIGIN`, which only suits
  its same-host proxied-CMS model).

### E9. Framed-cookie `SameSite`

The `entra_auth_state` cookie uses `SameSite=Lax`. That is fine while the iframe is
embedded **same-site** (CMS + proxy both under `cps.gov.uk`). A cross-site embed would
need `SameSite=None; Secure`. Revisit if the iframe host changes.

### E10. Store backend migration (the seam)

**Done 2026-10: the backend is MDS.** `store.js` exports `scope` (`ENTRA_MDS_SCOPE`) and
`deposit(payload, bearer)` = `mdsDeposit`: `PUT <WM_MDS_BASE_URL>cms-auth-store` with
`Authorization: Bearer <MDS token>` + `x-functions-key` (the same two settings the
global-components `/global-components/api` route uses — not that route itself, which strips
Authorization) and body `{cookies, token, expiryTime: "2100-01-01T00:00:00Z"}` (fixed, far future —
MDS 400s a non-future one; MDS ignores it today — revisit if it ever honours it). MDS takes the user
from the token's `oid`, after full JwtBearer validation (signature, issuer, lifetime, aud = the MDS
app reg) — so only a token for the MDS scope will do. The Table Storage backend (SharedKeyLite,
`ENTRA_STORAGE_*`, rows keyed by an id_token OID) is removed — git history has it.

**How the MDS token is obtained (decided 2026-10-15): a refresh-token swap, not on-behalf-of.**
The sign-in is neutral (`openid profile offline_access`); the code exchange yields a refresh token,
which the callback swaps (`grant_type=refresh_token` + client secret) for one token per service:
MDS, and on the iframe the watchdog. The refresh token never leaves the callback. On-behalf-of
(used 2026-10-06 → 10-15) needed a token addressed to our own app, i.e. an exposed scope
(`ENTRA_APP_SCOPE`); that coupling is gone: drop2 needs our app reg to expose no API (pre-prod
`8d6133af` happens to expose `api.presence.user.readwrite`, used only as the interim watchdog
scope). Consent for each swap is
the API pre-authorising our client — CPS blocks user consent (MDS terraform
`global_components_client_id`, PR 2026-10-06). Without it the swap fails (AADSTS65001) and drop2
degrades — safe. A missing refresh token (no `offline_access` consent) degrades likewise. With no
MDS scope configured at all (UAT today) a top-level handover doesn't start the round trip
(`skipped` / `no-mds-scope`); the iframe still runs while there is a watchdog scope.

### E11. ⚪ (removed) presence id-token cookie IE/Edge jar handover

Was the analysis of getting the `cms-auth-id-token` cookie into the IE-mode jar for the
presence-jsonp reader, plus the DEV-token proving stopgap. The presence path (this cookie +
the case-locking consumer) has been removed as experimental — see E3. Kept as a numbered stub
so E-numbers stay stable. If presence is revived, this IE/Edge-jar reality (WinINet vs Chromium
jars are separate; a top-level `/polaris` flow coerced to Edge lands the cookie invisibly to an
IE-mode reader) is the first thing to re-solve.

### E14. 🟠 Login hint + one interactive retry (D7): no / several Entra accounts

**Why.** `prompt=none` can't sign in a browser with several Entra accounts (AADSTS16000) or none
(50058): it falls back silently, every time, with no deposit. In pre-prod that is the normal case
(developers on Workspaces / Linux, admin + user accounts), and any app whose own MSAL sign-in picks
a different account from ours can't find the deposit (it is filed under the account `/init` used).

**What.**
- **The hint:** every successful sign-in (silent or interactive, even if the deposit then fails)
  records the account it used in `cps-entra-login-hint`: `Path=/`, `HttpOnly`, `Secure`,
  `SameSite=None`, 30 days, HMAC-signed with the state secret (kind `hint`, so it can't be forged
  by a sibling subdomain or swapped with a state cookie). Not bound to a host or an age, so a
  genuine hint can be replayed or planted from a sibling: harmless, since a hint grants nothing
  (Entra still needs that account's session), and one naming an account not signed in here gets
  50058, which forgets it (below). Value from the id_token: the
  `login_hint` optional claim (opaque; add it to the app reg), else `preferred_username`; plus
  `oid`, `tid`, `at`, and `via`. Each sign-in overwrites it, so it always names the account that last won.
- **`via`, how the account was chosen:** `silent` (no picker), or the reason the silent leg needed
  the picker, from its AADSTS number: `several-accounts` (16000), `no-session` (50058), `mfa`
  (50076/50079), `other`. A hinted sign-in can't tell whether other accounts exist (the hint picks
  one), so **a `several-accounts` hint is re-checked on every handover**: `/init` first signs in
  silently WITHOUT it (the probe, state `p=1`, outcome `hint=0`).
  - 16000, or another account wins: several are still signed in → `retried reason=several-accounts`
    / `other-account`, and the same flow signs in again silently WITH the hint (`m=1`), keeping the
    user's choice → `several-accounts`. The probe's code is discarded (no swap, no deposit).
  - The hint's own account wins: the others have signed out → `silent`.
  - Anything else (e.g. no session) takes the normal path. The hinted leg is never a probe, so it
    can't loop. Cost: one extra silent round trip, only for several-accounts browsers.

  A browser whose ambient account wins silently with no hint (Edge profile / PRT) is `silent` even
  with others signed in: Entra never offered a choice, so nobody is stuck.
- **`/init` sends it** as `login_hint` (still `prompt=none`), so a browser with several accounts
  signs in silently as the hinted one. Iframe too (it never prompts).
- **One interactive retry** (`ENTRA_INTERACTIVE_ENABLED=true`, pre-prod): a TOP-LEVEL callback with
  `login_required` / `interaction_required` / `account_selection_required` — except AADSTS53xxx
  (Conditional Access device blocks: a prompt would strand the user on Microsoft's block page) —
  goes back to `/authorize` with `prompt=select_account` and NO hint (the user chooses; that is also
  how they escape a stale hint). Same flow: same handle and destination, state re-signed with `n=1`
  (never retried twice) and a fresh `iat` with a 900 s lifetime (people are slow). A second failure
  degrades as before. Prod (`false`) stays a pure `prompt=none` probe.
- **A stale hint is forgotten:** a hinted leg (`hint=1`) that gets 50058 (none of the signed-in
  accounts is the hinted one: it signed out, or the browser changed hands) and isn't retried clears
  the hint as it carries on (outcome adds `hint-cleared=1`). Without that, every handover would send
  the dead hint and fail the same way for up to 30 days; the next one signs in unhinted and records
  whoever is there. Other errors (MFA, Conditional Access) keep the hint.
- **The endpoint** `/global-components/entra-login-hint` (exact match; CORS = global-components'
  `$cors_origin`, **https origins only**, with its own preflight): `GET` → `{loginHint, username, oid, tid, setAt, via}` or `null` (`via: "unknown"` for a
  hint set before `via` existed)
  (`no-store`); `DELETE` forgets it (the next handover then lets the user choose).

**For app teams (the contract).** If a hint is present, pass it as MSAL `loginHint`
(`ssoSilent({ scopes, loginHint })`, or `getAccount({ loginHint })` then `acquireTokenSilent`) or
as the OIDC `login_hint` parameter; check the token's `oid`/`tid` against the hint's. Show
"you are pinned to this account" UI (with a way to `DELETE`) only when `via === "several-accounts"`. Fetch with
`credentials: "include"`. From a `*.cps.gov.uk` origin the cookie is same-site; from another site
(prod `cps.outsystemsenterprise.com`) it is a third-party cookie, like the existing
`state` / `cms-session-hint` calls. Later: the hint on the handover redirect (needs the claim), so
CMS-launched apps get it without a fetch.

**Outcome line.** Callback lines with a known flow add `hint=0|1 int=0|1`; a silent leg handing over
to the interactive leg logs `result=retried` (with its `ad=`/`aadsts=`), and that flow then ends with
its own `ok`/`degraded` line with `int=1`. A stale hint's degrade adds `hint-cleared=1`. The probe's hand-over to the hinted leg is also
`retried`, with `reason=several-accounts` or `other-account`. Count handovers by excluding `retried`.

**Not covered.** An env with `mds_scope = ""` skips Entra for top-level handovers, so no hint or
retry there. The wrong account silently winning with no hint is still invisible.
