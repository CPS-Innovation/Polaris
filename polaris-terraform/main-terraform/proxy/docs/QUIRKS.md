# QUIRKS.md — things that look wrong in the proxy

Everything odd, surprising or outright broken found while building the test
harness (`proxy/tests`). Companion to [`PROXY.md`](./PROXY.md) — that doc says how
the proxy is *meant* to work; this one says where it doesn't.

**Nothing here has been fixed.** The suite is a *golden master*: its job is to pin
today's behaviour so the [`PROXY.md` §6](./PROXY.md) refactor can prove it changed
nothing. Where behaviour is wrong, it is recorded under a `QUIRK:` test with a
comment. Fix deliberately, updating the test in the same change.

**Status key**

| Status | Meaning |
| --- | --- |
| 🔴 **Pinned** | Proven by an executable `QUIRK:` test; the suite fails if it changes |
| 🟠 **Verified** | Confirmed by inspection/grep against the config, not yet pinned |
| ⚪ **Noted** | A smell or cleanup opportunity, not a defect |

Line numbers are `nginx.conf` unless stated.

---

## A. Security-relevant

### A1. 🔴 An empty or trailing-comma'd `AUTH_HANDOVER_WHITELIST` allows **everything**

**Where:** `nginx.js:119-123` (`appAuthRedirect`)
**Test:** `tests/unit/nginx.unit.test.js` → "QUIRK: an unset whitelist allows EVERYTHING", "QUIRK: a TRAILING COMMA in the whitelist also allows everything"

```js
const whitelistedUrls = process.env.AUTH_HANDOVER_WHITELIST ?? ""
whitelistedUrls.split(",").some((url) => redirectUrl.startsWith(url))
```

`"".split(",")` is `[""]`, and **every** string `.startsWith("")` is `true`. So an
empty whitelist does not allow *nothing* — it allows *everything*. `/init` will
then `302` to any attacker-supplied target **with the CMS session cookie attached
as `?cc=...`**: an open redirect that also leaks the cookie.

A **trailing comma** does the same (`"/auth-refresh-inbound,"` → `["/auth-refresh-inbound", ""]`)
— a plausible tfvars edit that silently disables the allow-list with no error.

**Impact:** latent, not live — terraform sets `AUTH_HANDOVER_WHITELIST` in every
environment ([`PROXY.md` §2](./PROXY.md)), so the value is non-empty today. The
exposure is if it is ever unset, blanked, or trailing-comma'd.
**Suggested fix:** drop empty entries — `.split(",").filter(Boolean)` — and treat an
empty list as deny-all.
**DECIDED 2026-07-17: leave as-is.** Stays recorded here and pinned by its tests;
raise a bug ticket **after** the refactor lands rather than changing behaviour
mid-flight. (Same for A2 — a natural companion fix when the ticket is picked up.)

### A2. 🔴 The whitelist is a **prefix** match, not an origin match

**Where:** `nginx.js:121-123`
**Test:** `tests/unit/nginx.unit.test.js` → "QUIRK: whitelist is a prefix match, not an origin match"

`redirectUrl.startsWith(entry)` means an entry of `https://allowed.example.org`
also permits `https://allowed.example.org.evil.com/x` — a different origin that
merely *starts with* the allowed string.

**Impact:** depends on the real entries. `PROXY.md` §2 shows prod entries that end
in `/` (e.g. `https://cps.outsystemsenterprise.com/`), which blunts this — but
`/auth-refresh-inbound` and the bare `https://polaris.cps.gov.uk/auth-refresh-inbound?...`
entries do not all end in a delimiter.
**Suggested fix:** compare parsed origins, or require entries to end in `/`.

### A3. 🟠 `error_log ... debug` is on in production

**Where:** `nginx.conf:5` — `error_log /dev/stderr debug;`

Debug-level logging for every request. Verbose (cost, noise) and it can emit
request detail — including cookies — into the log stream, which ships to Log
Analytics via the diagnostic settings.

**Suggested fix:** `warn` or `error` in prod; make the level an app setting if
per-environment control is wanted.

### A4. ⚪ `/internal-implementation/*` is "internal-only" by convention only

**Where:** `nginx.conf:803, 838, 872, 907` — 0 `allow`/`deny` directives in any of them
**Test:** `tests/integration/tests/06-polaris-ddei.integration.test.js` (they are freely reachable in the harness)

The comments say "internal-only route to CMS Classic, used by DDEI", but nothing
in nginx enforces that — it rests entirely on Azure network isolation (the app is
`public_network_access_enabled = false` behind a private endpoint). Correct today,
but there is no defence in depth, and the constraint is invisible to a reader of
the config.

**Suggested fix:** none needed if the network boundary is trusted; worth an
explicit comment, or an `allow`/`deny` pair, given §4 marks these for deletion
anyway.

---

## B. Confirmed broken / dead

### B1. 🔴 KNOWN BUG — `replaceCmsDomains` never rewrites a real domain

**Status: accepted, left in place.** Three routes do no CMS-domain rewriting as a
result. Recorded here rather than fixed — see the decision at the end of this entry.

**Where:** `cmsenv.js:138-148` (`__replaceContent`)
**Test:** `tests/unit/cmsenv.unit.test.js` → "QUIRK: does NOT rewrite a real CMS domain (regex strips the dots)", "QUIRK: it DOES rewrite the dot-less form, proving the regex is mangled"

```js
let reg = /[-=./]/gm;
let repold = (rep.old).replace(reg, "");   // "cin2.cps.gov.uk" -> "cin2cpsgovuk"
let regexp = new RegExp(repold, 'g');
```

It **strips** the regex-special characters instead of **escaping** them. So the
search pattern becomes `/cin2cpsgovuk/`, which never occurs in a real page. Proven
both ways: a body containing `cin2.cps.gov.uk` passes through untouched, while the
dot-less `cin2cpsgovuk` *is* rewritten. Almost certainly a one-character slip
(`.` → `\.`).

#### What it was meant to do

Make CMS content **self-referential to the proxy**. When a browser renders CMS
*through* the proxy the user is on `polaris.cps.gov.uk`, so an **absolute** URL in
the returned HTML/JS saying `https://cms.cps.gov.uk/...` would send them straight
off the proxy and break the proxied session. The filter therefore swaps every
identifier CMS might emit for itself — for the **currently selected environment** —
to the proxy's own host:

| `old` (7 replacements) | `new` |
| --- | --- |
| `<env>UpstreamCmsDomainName` · `…ModernDomainName` · `…ServicesDomainName` | `$host` |
| `<env>UpstreamCmsIpCorsham` · `…ModernIpCorsham` · `…IpFarnborough` · `…ModernIpFarnborough` | `$host` |

The IPs are in there because the proxy dials CMS **by IP**, so CMS can emit one in
a generated URL. `replaceCmsDomainsAjaxViewer` targets `$websiteHostname` instead
of `$host` (the proxy's canonical name rather than whatever `Host` arrived), and
the `r.status === 302` guard just skips redirect bodies.

It is the same job the `sub_filter $upstreamCms*DomainName $host` lines do on the
other six routes — attempted in njs, and slightly **more ambitious**: those six
rewrite the three domains only, whereas this also intended the four datacentre IPs
(which otherwise only the `internal-implementation` blocks rewrite).

#### Where it is used (traced 2026-07-17)

Three `js_body_filter` call sites, and the split against `sub_filter` is **clean —
no location uses both**:

| Rewrites domains via `sub_filter` ✅ | Relies **solely** on the njs filter ❌ |
| --- | --- |
| `164` uainGeneratedScript.aspx | `203` uainMenuBar.js — `cmsMenuBarFilters` |
| `309` /CMSModern/Files | `279` uacdCDTabs.aspx — `replaceCmsDomains` |
| `354` ^/CMS.* | `556` /ajax/viewer/ — `replaceCmsDomainsAjaxViewer` |
| `520` / (Modern root) | |
| `803`, `872` internal-implementation (x5 — also rewrites the IPs) | |

**So this is not redundant dead code.** On those three routes it is the *only*
domain-rewrite mechanism, and it does nothing — meaning **those three routes do no
CMS-domain rewriting at all**. Their only other filter is
`sub_filter https:// ${WEBSITE_SCHEME}://`, a no-op in prod (both are `https`).

**Impact:** any absolute CMS URL (`cms.cps.gov.uk`, `cmsmodern…`, `cms-services…`,
or a Corsham/Farnborough IP) in the toolbar script, the case-detail tabs page, or
ajax-viewer content passes through untouched — a browser following one leaves the
proxy and goes direct to CMS. No visible trouble so far suggests those responses
mostly carry *relative* URLs, but that is unverified (see the open question below).

#### Why we survive without it

1. **Relative URLs do the work.** The big one. A CMS page linking to `/CMS/Case/1`
   needs no rewriting — the browser resolves it against the current origin, which
   *is* the proxy. Only **absolute** URLs need rewriting, and server-rendered apps
   mostly emit relative ones. There is simply little for the filter to find.
2. **These three are narrow special cases, not the bulk of traffic.** The volume
   goes through `^/CMS.*` (354) and `/` (520) — both of which rewrite correctly via
   `sub_filter`.
3. **They would have got it free.** Each of the three *overrides* a route that
   already rewrites properly — `203`/`279` would otherwise match `354 ^/CMS.*`, and
   `556` would otherwise match `520 /`. The blocks exist to add something extra
   (button/script injection); in specialising, they opted into the njs filter
   *instead of* `sub_filter` and silently lost the rewrite they would have
   inherited. There is no technical reason for that — `203` and `279` already run
   `sub_filter` and `js_body_filter` side by side. It reads as an accident of
   authorship, not a constraint.
4. **Proxied-CMS browsing may be a minority mode.** The rewriting only matters when
   a browser renders CMS *through* the proxy. `nginx.js` calls that "primarily
   useful when users are using CMS delivered through this proxy", and the `/cinN`
   switching is dev/test-flavoured. If most prod users are on real CMS and only hop
   to the proxy for Polaris itself, the blast radius is small. *(Least certain of
   the four — needs someone who knows the deployment, not the code.)*

Net: the filter has been inert for as long as it has existed, and the damage is
bounded to whatever absolute CMS URLs happen to appear in exactly those three
responses. Plausibly none — which is why it went unnoticed.

#### Decision (2026-07-17): leave as a known bug

No change now. It is recorded here and pinned by its tests, so it cannot regress
further or be "fixed" by accident during the refactor.

When it is picked up, the options are:

1. **Delete** — behaviourally a **no-op today**, so zero risk, and it drops
   `replaceCmsDomains` / `__replaceCmsDomainsGeneric` / `__replaceContent` from the
   future `common.js` ([`PROXY.md` §6.2](./PROXY.md)). But it makes the gap
   permanent.
2. **Repair** (`.` → `\.`) — restores the intent, but *starts* rewriting three
   routes that currently don't. A real behaviour change.
3. **Replace with `sub_filter`** — give those three blocks the same three
   `sub_filter $upstreamCms*DomainName $host` lines the other six use; one
   mechanism everywhere, and `cmsMenuBarFilters` keeps only its (working) button
   injection. Note `/ajax/viewer/` targets `$websiteHostname`, not `$host`, so it
   needs `${WEBSITE_HOSTNAME}`.

**Note the inversion:** deleting is the *safe* option and repairing is the *risky*
one — 2 and 3 are the same behaviour change and need the same evidence. **A real
capture of `uacdCDTabs.aspx` would settle it**: no absolute CMS URLs ⇒ 1 and 3 are
equivalent and 1 is free; any absolute CMS URLs ⇒ there is a live user-facing bug
on those three routes.

### B2. 🔴 `add_header Content-Disposition: inline` — stray colon breaks it

**Where:** `nginx.conf:736` (in `/sas-url/`)
**Test:** `tests/integration/tests/xx-redundant.integration.test.js` → "QUIRK: add_header 'Content-Disposition:' yields the value ': inline'"

The colon is taken as part of the **header name**, so nginx puts
`Content-Disposition:: inline` on the wire. HTTP splits on the first colon, so
clients see `Content-Disposition: ": inline"` — a bogus value. The intended
"render this blob inline" behaviour **does not work today**.

**Suggested fix:** `add_header Content-Disposition inline;` — but note §4 marks
`/sas-url/` for deletion, so this may be moot.

### B3. 🔴 Security headers are silently dropped on many locations

**Where:** `nginx.conf:65-70` (declared) — nginx's `add_header` inheritance rule
**Test:** `tests/integration/tests/zz-cross-cutting.integration.test.js` → "QUIRK: locations declaring their own add_header lose the security set"; `00-health` → "QUIRK: = / does not get the inherited security headers"

nginx inherits `add_header` into a location **only if that location declares no
`add_header` of its own**. So every block that sets any header for its own purposes
loses the whole server-level set: `X-Frame-Options`, HSTS, `X-Content-Type-Options`,
`X-Permitted-Cross-Domain-Policies`, `Cache-Control: no-store`, `Pragma`.

Affected today (non-exhaustive): `= /`, `= /robots933456.txt`,
`= /api/navigate-cms`, `= /navigate-cms-close`, `/cin2`–`/cin5`, `/sas-url/`.
`/api/` and the SPA/Materials blocks *do* get them (they declare none).

The config comment at `nginx.conf:62` acknowledges the rule — but the consequence
is easy to miss, and it is invisible per-location.

**Impact:** mostly low-value endpoints (a health string, redirects), so the
practical risk is small — but the `/cinN` and navigate-cms responses are real HTML
served to browsers without `X-Frame-Options`/HSTS.
**Suggested fix:** repeat the security headers inside those blocks, or move them to
a snippet `include`d everywhere. Worth deciding during the §6 slicing, since that
is when each block gets touched anyway.

### B4. 🟠 `ENVIRONMENT` / `$environment` is declared but never read

**Where:** `nginx.conf:119` — `js_var $environment "${ENVIRONMENT}";`
**Evidence:** the only occurrence of `$environment` in `nginx.conf` is the
declaration itself; no njs module reads `r.variables.environment`.

Dead config. It is also an app setting, a `lifecycle.ignore_changes` entry, and a
row in the §2 grid — all for nothing.

**Suggested fix:** delete the `js_var` (and consider dropping the app setting).
Already reflected in `PROXY.md` §2 as "— declared, unused".

---

## C. Fragile / surprising behaviour

### C1. 🔴 The legacy `_POOL` regex over-captures separator-less cookie names

**Where:** `nginx.js:84` (`setSessionHintCookie` fallback)
**Test:** `tests/unit/nginx.unit.test.js` and `tests/integration/tests/02-auth-handover.integration.test.js` → "QUIRK: separator-less BIGipServer<pool> ... over-captures"

```
/[a-z][a-z0-9]*(?:\.[a-z][a-z0-9]*)*\.cps\.gov\.uk(?=_POOL)/g
```

It anchors on a **lowercase** run to skip uppercase LB prefixes (the comment cites
`CPSACP-LTM-CM-WAN-CIN3-`, which works). But where lowercase letters run straight
into the domain with no separator — the classic F5 `BIGipServer<pool>` name — it
starts mid-word:

```
BIGipServercin2.cps.gov.uk_POOL  ->  "ervercin2.cps.gov.uk"
                                 ->  handoverEndpoint https://ervercin2.cps.gov.uk/polaris
```

**Impact: LEGACY — not reachable today.** Answered 2026-07-17: neither prod nor any
pre-prod environment uses `BIGipServer`-format LB cookies any more; they all use
the `[CF]-<TOKEN>-LBsessioncookie` form, which takes the *preferred* branch and is
unaffected. This whole `_POOL` fallback is dead legacy.
**Suggested fix:** the over-capture needs no fix on its own — the question is
whether the `_POOL` fallback branch should exist at all. A candidate for deletion
during §6 (which would also remove this quirk). The test stays until then.

### C2. 🔴 CMS environment detection is a loose substring match over the whole `Cookie` header

**Where:** `cmsenv.js:163-170` (`__getCmsEnvInternal`)
**Test:** `tests/unit/cmsenv.unit.test.js` → "QUIRK: any cookie containing 'cin2' selects cin2, even unrelated ones"

```js
cookie = cookie.toLowerCase();
if (cookie.includes("cin3")) return "default";
if (cookie.includes("cin2")) return "cin2";
...
```

It does not parse `__CMSENV` — it substring-searches the **entire** Cookie header.
So *any* cookie containing `cin2` anywhere selects cin2, including a BIG-IP cookie
naming a domain, or an unrelated `somethingcin2something=1`.

**Suggested fix:** read the `__CMSENV` cookie value properly.

### C3. 🔴 Fixed check order means `cin3` beats `cin2` beats `cin4`/`cin5`

**Where:** `cmsenv.js:165-168`
**Test:** `tests/unit/cmsenv.unit.test.js` → "QUIRK: cin3 wins over cin2 when both appear"

The checks return on the first hit in the order cin3 → cin2 → cin4 → cin5. With
C2, a header mentioning several environments resolves by that fixed order rather
than by which cookie is authoritative:

```
Cookie: __CMSENV=cin2; other=cin3   ->  DEFAULT   (not cin2!)
```

Realistic when stale BIG-IP cookies from another environment linger — which is
exactly what the `/cinN` switch blocks exist to clear.

**Suggested fix:** same as C2 — parse the value instead of scanning the header.

### C4. 🔴 `getDomainFromCookie` throws when nothing matches

**Where:** `cmsenv.js:1-6`
**Test:** `tests/unit/cmsenv.unit.test.js` → "QUIRK: throws when the cookie has no *.cps.gov.uk domain"

```js
let domainMatch = cookie.match(/([a-z0-9]+)\.cps\.gov\.uk/);
return domainMatch[0];     // TypeError when match() returns null
```

No null guard, so a request to `/api/navigate-cms` without a CMS domain cookie
raises an njs exception on the `js_set` rather than producing a value.

**Suggested fix:** `return domainMatch ? domainMatch[0] : ""`.

### C5. 🔴 A missing `cookie` arg silently produces an `{error}` session hint

**Where:** `nginx.js:68-112` (`setSessionHintCookie`'s `try/catch/finally`)
**Test:** `tests/unit/nginx.unit.test.js` → "QUIRK: a missing cookie arg yields an {error} hint payload, not a failure"

With no `cookie` query arg, `cookie.match(...)` throws; the `catch` writes
`{ error }` into the `Cms-Session-Hint` cookie and the `finally` still sets it —
and the redirect proceeds as normal. The user gets a working-looking redirect and
a poisoned hint cookie, with nothing logged.

**Suggested fix:** decide whether that is a 4xx, and at minimum log it.

---

## D. Config smells and cleanup opportunities

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
traffic to CMS** — the stated intent. _(An earlier draft of this doc wrongly
flagged the constant key as a bug; that was a misread of the intent.)_

**Suggested fix:** keep the zones and the constant key — the ability to throttle
globally is wanted. Only the burst needs a real value if/when the throttle is to
be armed. Worth a comment in the config recording that the constant key is
intentional, since it reads like a mistake.

### D2. 🟠 `/CMSModern/Files` dials a **domain**, unlike every other CMS block

**Where:** `nginx.conf:349` — `proxy_pass https://$upstreamCmsServicesDomainName;`

Every other CMS route proxies to a Corsham/Farnborough **IP** from
`var.cms_details`; this one alone resolves the CMS *Services* domain by DNS. That
asymmetry is why the test harness needs a docker network alias for
`cms-services.cps.gov.uk` — and it means this route's availability depends on DNS
rather than the pinned IPs.

**Suggested fix:** confirm whether that is deliberate (CMS Services may genuinely
have no DC-pinned IP). If not, align it with the rest.
**DECIDED 2026-07-17: leave alone** — behaviour here is not well understood, so it
is out of scope for the refactor. Do not "tidy" it. The harness pins it via a
docker network alias, so any accidental change will show up as a red test.

### D3. 🟠 `/v2/` hardcodes the App Insights hostname

**Where:** `nginx.conf:940` — `proxy_pass https://uksouth-1.in.applicationinsights.azure.com/v2/;`

The only upstream with no app setting; the region (`uksouth-1`) is baked in. Also
appears as a `sub_filter` target in the SPA/Materials blocks.

**Suggested fix:** make it an app setting like every other endpoint.

### D4. 🟠 `/ajax//viewer/` — double slash in `proxy_pass`

**Where:** `nginx.conf:586` — `proxy_pass $proxyDestinationModern/ajax//viewer/$1$is_args$args;`

With `merge_slashes off` set globally (`nginx.conf:50`), that literal `//` is sent
upstream as-is. Either deliberate (matching a CMS quirk) or a typo — worth a
comment either way.

### D5. 🟠 `FORCE_REFRESH_CONFIG` has a stray `::` and is hand-maintained

**Where:** `app-service-proxy.tf:71`

```
"${md5(file("nginx.conf"))}:${md5(file("nginx.js"))}:${md5(file("cmsenv.js"))}::${md5(file("polaris-script.js"))}:..."
```

The double colon suggests a deleted entry. More importantly the whole string is
maintained by hand, so a new config file is easy to forget — see `PROXY.md` §6.5,
which proposes replacing it with a `fileset()`-driven hash.

### D6. 🟠 `WEBSITE_SCHEME` and `ENDPOINT_HTTP_PROTOCOL` are the same value

**Where:** `app-service-proxy.tf:18` and `:69` — both `"https"`

Two settings, one value, different uses (redirect/`sub_filter` scheme vs
`proxy_pass` protocol). Harmless but confusing; a candidate to merge in §6.

### D7. ⚪ The four `*Internal` cmsenv functions are pure delegates

**Where:** `cmsenv.js:13-15, 22-24, 31-33, 40-42`
**Test:** `tests/unit/cmsenv.unit.test.js` → "the four *Internal fns are pure delegates to their public twins" (proven identical)

`proxyDestinationCorshamInternal(r)` is literally `return proxyDestinationCorsham(r)`.
They exist only to give the internal routes a distinct `js_set` name. Free to
collapse in §6.

### D8. ⚪ `/materials-ui/{a}/{b}/materials` looks redundant with `/materials-ui/`

**Where:** `nginx.conf:701` vs `:709`
**Test:** `tests/integration/tests/04-cwa-materials.integration.test.js` (both produce the same upstream path)

Added by FCT2-15354 for the M-button deep link. Both send the identical path
upstream; the only differences are `proxy_pass` with vs without a URI, and the
regex block having *fewer* `sub_filter_types`. Strong drop candidate — but verify
against a real deep link first (the URI-vs-bare `proxy_pass` distinction affects
URI normalisation).

### D9. ⚪ "DEFAULT" means CIN3

**Where:** `var.cms_details.default_*`, `cmsenv.js:165`

`if (cookie.includes("cin3")) return "default"`. The naming misleads every reader —
worth renaming, or at least a prominent comment, during §6.

### D10. ⚪ The root `/` catch-all proxies to CMS Modern

**Where:** `nginx.conf:520`

Any unmatched path falls through to CMS Modern. Flagged in §4 as "has always been
a bit dodgy just being on the root". It means the proxy has no real 404 — unknown
paths become CMS requests.

### D11. ⚪ Every app setting is written three times

**Where:** `app-service-proxy.tf` (`app_settings` + `lifecycle.ignore_changes`) and
`app-service-proxy-staging.tf`

71 settings in the block, 71 more in `ignore_changes`, and the staging twin repeats
both. (Checked: prod and staging currently agree at 71 each — no drift *today*.)
Adding a setting means four edits. See `PROXY.md` §6.5.

### D12. ⚪ `/launch/*`: one route is env-driven, nine hardcode their domains

**Where:** `nginx.conf:616-645`
**Test:** `tests/integration/tests/01-app-launch.integration.test.js`

Only `/launch/cms` uses `${DEFAULT_UPSTREAM_CMS_DOMAIN_NAME}`; `/launch/cin2`–`cin5`
and all five `-proxy` variants hardcode `cinN.cps.gov.uk` /
`polaris-qa-notprod.cps.gov.uk`, including prod-only and test-only OutSystems URLs
in the same file. §4 notes they "can be collapsed into one (maybe needs njs)".

---

## Decisions log

| # | Question | Answer (2026-07-17) |
| --- | --- | --- |
| **C1** | Which `_POOL` cookie names does production emit? | **Legacy — none.** Prod and all pre-prod use `[CF]-<TOKEN>-LBsessioncookie`; the `_POOL` fallback is dead. Over-capture is unreachable. Candidate for deletion in §6. |
| **A1** | Guard the whitelist now? | **No — leave as-is.** Keep it recorded and pinned; raise bug tickets once the refactor has landed. Same for A2. |
| **D1** | Is rate limiting wanted? | **Yes — a global throttle of all CMS traffic.** So the constant key is *correct*, not a bug (doc corrected). Only the 10¹⁷ burst makes it inert. |
| **D2** | Is the CMS Services DNS dependency deliberate? | **Unknown — leave alone.** Not well understood; out of scope. Pinned by the harness. |
| **B1** | Delete, repair, or replace `replaceCmsDomains`? | **Leave as a known bug.** No change now; recorded and pinned. Three routes do no domain rewriting meanwhile. Deciding evidence when picked up: a real `uacdCDTabs.aspx` capture. |

## Open questions

1. **B3** — should the security headers be restored on the locations that
   currently drop them? Natural to settle during the §6 slicing, when each block
   is being touched anyway.
