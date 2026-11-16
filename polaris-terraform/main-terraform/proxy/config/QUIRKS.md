# QUIRKS — cross-cutting, and index

Everything odd, surprising or outright broken found while building the test
harness (`proxy/tests`). Companion to [`../docs/PROXY.md`](../docs/PROXY.md) — that
doc says how the proxy is _meant_ to work; the QUIRKS docs say where it doesn't.

This file (next to the root `nginx.conf`) holds the quirks that belong to **no single
feature** — server-preamble, cross-location and terraform-level concerns — plus the
**index**, **status key**, **decisions log** and **open questions** for the whole set.
Feature-specific quirks live in a `QUIRKS.md` **inside each feature folder**.

**Nothing here has been fixed.** The suite is a _golden master_: its job is to pin
today's behaviour so the [`../docs/PROXY.md` §6](../docs/PROXY.md) refactor can prove
it changed nothing. Where behaviour is wrong, it is recorded under a `QUIRK:` test
with a comment. Fix deliberately, updating the test in the same change.

**Status key**

| Status          | Meaning                                                              |
| --------------- | -------------------------------------------------------------------- |
| 🔴 **Pinned**   | Proven by an executable `QUIRK:` test; the suite fails if it changes |
| 🟠 **Verified** | Confirmed by inspection/grep against the config, not yet pinned      |
| ⚪ **Noted**    | A smell or cleanup opportunity, not a defect                         |

Live line numbers are `../nginx.conf` / the live `main-terraform/{nginx,cmsenv}.js`
(the golden master) unless stated. Each entry also points at the file the code now
lives in under the refactored (`next`) config.

---

## Index — where each quirk now lives

| #   | Quirk (short)                                            | Home                                                                             |
| --- | -------------------------------------------------------- | -------------------------------------------------------------------------------- |
| A1  | Empty/trailing-comma whitelist allows everything         | [`features/auth-handover/QUIRKS.md`](./features/auth-handover/QUIRKS.md)         |
| A2  | Whitelist is a prefix match, not an origin match         | [`features/auth-handover/QUIRKS.md`](./features/auth-handover/QUIRKS.md)         |
| A3  | `error_log … debug` on in production                     | **this file**                                                                    |
| A4  | `/internal-implementation/*` internal-only by convention | [`features/polaris-ddei/QUIRKS.md`](./features/polaris-ddei/QUIRKS.md)           |
| B1  | `replaceCmsDomains` never rewrites a real domain         | [`features/cms-proxy/QUIRKS.md`](./features/cms-proxy/QUIRKS.md)                 |
| B2  | `Content-Disposition:` stray colon (`/sas-url/`)         | **this file** (route pruned from `next`)                                         |
| B3  | Security headers silently dropped on many locations      | **this file**                                                                    |
| B4  | `ENVIRONMENT` / `$environment` declared, never read      | **this file**                                                                    |
| C1  | `_POOL` regex over-captures separator-less cookie names  | [`features/auth-handover/QUIRKS.md`](./features/auth-handover/QUIRKS.md)         |
| C2  | CMS-env detection is a loose substring over `Cookie`     | [`features/common/QUIRKS.md`](./features/common/QUIRKS.md)                       |
| C3  | Fixed check order: cin3 beats cin2 beats cin4/cin5       | [`features/common/QUIRKS.md`](./features/common/QUIRKS.md)                       |
| C4  | `getDomainFromCookie` throws when nothing matches        | [`features/global-components/QUIRKS.md`](./features/global-components/QUIRKS.md) |
| C5  | Missing `cookie` arg → silent `{error}` session hint     | [`features/auth-handover/QUIRKS.md`](./features/auth-handover/QUIRKS.md)         |
| D1  | Rate limiter can never fire (burst is 10¹⁷)              | **this file**                                                                    |
| D2  | `/CMSModern/Files` dials a domain, not an IP             | [`features/cms-proxy/QUIRKS.md`](./features/cms-proxy/QUIRKS.md)                 |
| D3  | `/v2/` hardcodes the App Insights hostname               | [`features/cwa-materials/QUIRKS.md`](./features/cwa-materials/QUIRKS.md)         |
| D4  | `/ajax//viewer/` double slash in `proxy_pass`            | [`features/cms-proxy/QUIRKS.md`](./features/cms-proxy/QUIRKS.md)                 |
| D5  | `FORCE_REFRESH_CONFIG` stray `::`, hand-maintained       | **this file**                                                                    |
| D6  | `WEBSITE_SCHEME` == `ENDPOINT_HTTP_PROTOCOL`             | **this file**                                                                    |
| D7  | Four `*Internal` cmsenv fns are pure delegates           | [`features/polaris-ddei/QUIRKS.md`](./features/polaris-ddei/QUIRKS.md)           |
| D8  | `/materials-ui/{a}/{b}/materials` looks redundant        | [`features/cwa-materials/QUIRKS.md`](./features/cwa-materials/QUIRKS.md)         |
| D9  | "DEFAULT" means CIN3                                     | [`features/common/QUIRKS.md`](./features/common/QUIRKS.md)                       |
| D10 | Root `/` catch-all proxies to CMS Modern                 | [`features/cms-proxy/QUIRKS.md`](./features/cms-proxy/QUIRKS.md)                 |
| D11 | Every app setting is written three times                 | **this file**                                                                    |
| D12 | `/launch/*`: one route env-driven, nine hardcoded        | [`features/app-launch/QUIRKS.md`](./features/app-launch/QUIRKS.md)               |

---

## Cross-cutting / root / terraform

### A3. 🟠 `error_log ... debug` is on in production

**Where (live):** `nginx.conf:5` — `error_log /dev/stderr debug;`
**Where (next):** `nginx.conf:5` (the root config's server preamble — unchanged by the slice)

Debug-level logging for every request. Verbose (cost, noise) and it can emit
request detail — including cookies — into the log stream, which ships to Log
Analytics via the diagnostic settings.

**Suggested fix:** `warn` or `error` in prod; make the level an app setting if
per-environment control is wanted.

### B2. 🔴 `add_header Content-Disposition: inline` — stray colon breaks it

**Where (live):** `nginx.conf:736` (in `/sas-url/`)
**Where (next):** _n/a — `/sas-url/` was pruned from the refactored config_ (§4
marked it for deletion; there is no `sas-url` feature). Recorded here as history:
if the route is ever reinstated, do not reintroduce the bug.

The colon is taken as part of the **header name**, so nginx puts
`Content-Disposition:: inline` on the wire. HTTP splits on the first colon, so
clients see `Content-Disposition: ": inline"` — a bogus value. The intended
"render this blob inline" behaviour **did not work**.

**Suggested fix:** `add_header Content-Disposition inline;` — but the route is gone
in `next`, so this is moot unless it returns.

### B3. 🔴 Security headers are silently dropped on many locations

**Where (live):** `nginx.conf:65-70` (declared) — nginx's `add_header` inheritance rule
**Test:** `nginx.integration.test.js` → "QUIRK: `= /` does not get the inherited
security headers", "QUIRK: `= /robots933456.txt` does not get them either", "QUIRK:
locations declaring their own add_header lose the security set"

nginx inherits `add_header` into a location **only if that location declares no
`add_header` of its own**. So every block that sets any header for its own purposes
loses the whole server-level set: `X-Frame-Options`, HSTS, `X-Content-Type-Options`,
`X-Permitted-Cross-Domain-Policies`, `Cache-Control: no-store`, `Pragma`.

Affected today (non-exhaustive): `= /`, `= /robots933456.txt`,
`= /api/navigate-cms`, `= /navigate-cms-close`, `/cin2`–`/cin5`, `/sas-url/`.
`/api/` and the SPA/Materials blocks _do_ get them (they declare none).

Cross-cutting by nature — it touches locations owned by several features — which is
why it is pinned by the root-level `nginx.integration.test.js` rather than any one
feature suite.

**Impact:** mostly low-value endpoints (a health string, redirects), so the
practical risk is small — but the `/cinN` and navigate-cms responses are real HTML
served to browsers without `X-Frame-Options`/HSTS.
**Suggested fix:** repeat the security headers inside those blocks, or move them to
a snippet `include`d everywhere. Worth deciding during the §6 slicing, since that
is when each block gets touched anyway. **(See Open questions.)**

### B4. 🟠 `ENVIRONMENT` / `$environment` is declared but never read

**Where (live):** `nginx.conf:119` — `js_var $environment "${ENVIRONMENT}";`
**Evidence:** the only occurrence of `$environment` is the declaration itself; no
njs module reads `r.variables.environment`.

Dead config. It is also an app setting, a `lifecycle.ignore_changes` entry, and a
row in the §2 grid — all for nothing.

**Suggested fix:** delete the `js_var` (and consider dropping the app setting).
Already reflected in `../docs/PROXY.md` §2 as "— declared, unused".

### D1. 🟠 The rate limiter can never fire (burst is 10¹⁷)

**Where:** `nginx.conf:29-30`, `app-service-proxy.tf:72-73`

```nginx
limit_req_zone cmsproxy   zone=cmsproxy:1m   rate=${CMS_RATE_LIMIT};     # 128r/s
limit_req_zone cmsgateway zone=cmsgateway:1m rate=${CMS_RATE_LIMIT};
```

Prod sets `CMS_RATE_LIMIT_QUEUE = "100000000000000000"` (10¹⁷) as the `burst=` on
every `limit_req`, so the queue never fills and the limiter never triggers. The
throttle is configured but inert.

**Not a problem — by design:** the **constant key** (`cmsproxy` rather than
`$binary_remote_addr`) is deliberate and correct. A constant key means a single
shared bucket, which is exactly how you implement a **global throttle of all
traffic to CMS** — the stated intent.

**Suggested fix:** keep the zones and the constant key. Only the burst needs a real
value if/when the throttle is to be armed. Worth a comment in the config recording
that the constant key is intentional, since it reads like a mistake.

### D5. 🟠 `FORCE_REFRESH_CONFIG` has a stray `::` and is hand-maintained

**Where:** `app-service-proxy.tf:71`

```
"${md5(file("nginx.conf"))}:${md5(file("nginx.js"))}:${md5(file("cmsenv.js"))}::${md5(file("polaris-script.js"))}:..."
```

The double colon suggests a deleted entry. More importantly the (live) portion of
the string is maintained by hand, so a new config file is easy to forget.

**Partly addressed in `next`:** the refactored tree is hashed with a
`fileset()`-driven `local.proxy_next_config_hash` appended to this same string (see
`app-service-proxy.tf`), so feature files no longer need hand-maintained md5s. The
stray `::` and the hand-listed live files remain — clean those up at cutover.

### D6. 🟠 `WEBSITE_SCHEME` and `ENDPOINT_HTTP_PROTOCOL` are the same value

**Where:** `app-service-proxy.tf:18` and `:69` — both `"https"`

Two settings, one value, different uses (redirect/`sub_filter` scheme vs
`proxy_pass` protocol). Harmless but confusing; a candidate to merge in §6.

### D11. ⚪ Every app setting is written three times

**Where:** `app-service-proxy.tf` (`app_settings` + `lifecycle.ignore_changes`) and
`app-service-proxy-staging.tf`

71 settings in the block, 71 more in `ignore_changes`, and the staging twin repeats
both. (Checked: prod and staging currently agree at 71 each — no drift _today_.)
Adding a setting means four edits. See `../docs/PROXY.md` §6.5.

---

## Decisions log

| #      | Question                                         | Answer (2026-07-17)                                                                                                                                                              |
| ------ | ------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **C1** | Which `_POOL` cookie names does production emit? | **Legacy — none.** Prod and all pre-prod use `[CF]-<TOKEN>-LBsessioncookie`; the `_POOL` fallback is dead. Over-capture is unreachable. Candidate for deletion in §6.            |
| **A1** | Guard the whitelist now?                         | **No — leave as-is.** Keep it recorded and pinned; raise bug tickets once the refactor has landed. Same for A2.                                                                  |
| **D1** | Is rate limiting wanted?                         | **Yes — a global throttle of all CMS traffic.** So the constant key is _correct_, not a bug (doc corrected). Only the 10¹⁷ burst makes it inert.                                 |
| **D2** | Is the CMS Services DNS dependency deliberate?   | **Unknown — leave alone.** Not well understood; out of scope. Pinned by the harness.                                                                                             |
| **B1** | Delete, repair, or replace `replaceCmsDomains`?  | **Leave as a known bug.** No change now; recorded and pinned. Three routes do no domain rewriting meanwhile. Deciding evidence when picked up: a real `uacdCDTabs.aspx` capture. |

## Open questions

1. **B3** — should the security headers be restored on the locations that
   currently drop them? Natural to settle during the §6 slicing, when each block
   is being touched anyway.
