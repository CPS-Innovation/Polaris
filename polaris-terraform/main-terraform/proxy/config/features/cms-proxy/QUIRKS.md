# QUIRKS — cms-proxy

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

Live line numbers are the live `main-terraform/{nginx.conf,cmsenv.js}` (the golden
master); the code now lives in `cms-proxy.{conf,js}` in this folder.

---

## B. Confirmed broken / dead

### B1. 🔴 KNOWN BUG — `replaceCmsDomains` never rewrites a real domain

**Status: accepted, left in place.** Three routes do no CMS-domain rewriting as a
result. Recorded here rather than fixed — see the decision at the end of this entry.

**Where (live):** `cmsenv.js:138-148` (`__replaceContent`)
**Where (next):** `cms-proxy.js` (`replaceCmsDomains` / `__replaceCmsDomainsGeneric` / `__replaceContent`)
**Test:** `cms-proxy.unit.test.js` → "QUIRK: does NOT rewrite a real CMS domain (regex strips the dots)", "QUIRK: it DOES rewrite the dot-less form, proving the regex is mangled"

```js
let reg = /[-=./]/gm;
let repold = rep.old.replace(reg, ""); // "cin2.cps.gov.uk" -> "cin2cpsgovuk"
let regexp = new RegExp(repold, "g");
```

It **strips** the regex-special characters instead of **escaping** them. So the
search pattern becomes `/cin2cpsgovuk/`, which never occurs in a real page. Proven
both ways: a body containing `cin2.cps.gov.uk` passes through untouched, while the
dot-less `cin2cpsgovuk` _is_ rewritten. Almost certainly a one-character slip
(`.` → `\.`).

#### What it was meant to do

Make CMS content **self-referential to the proxy**. When a browser renders CMS
_through_ the proxy the user is on `polaris.cps.gov.uk`, so an **absolute** URL in
the returned HTML/JS saying `https://cms.cps.gov.uk/...` would send them straight
off the proxy and break the proxied session. The filter therefore swaps every
identifier CMS might emit for itself — for the **currently selected environment** —
to the proxy's own host:

| `old` (7 replacements)                                                                       | `new`   |
| -------------------------------------------------------------------------------------------- | ------- |
| `<env>UpstreamCmsDomainName` · `…ModernDomainName` · `…ServicesDomainName`                   | `$host` |
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

| Rewrites domains via `sub_filter` ✅                              | Relies **solely** on the njs filter ❌              |
| ----------------------------------------------------------------- | --------------------------------------------------- |
| `164` uainGeneratedScript.aspx                                    | `203` uainMenuBar.js — `cmsMenuBarFilters`          |
| `309` /CMSModern/Files                                            | `279` uacdCDTabs.aspx — `replaceCmsDomains`         |
| `354` ^/CMS.\*                                                    | `556` /ajax/viewer/ — `replaceCmsDomainsAjaxViewer` |
| `520` / (Modern root)                                             |                                                     |
| `803`, `872` internal-implementation (x5 — also rewrites the IPs) |                                                     |

**So this is not redundant dead code.** On those three routes it is the _only_
domain-rewrite mechanism, and it does nothing — meaning **those three routes do no
CMS-domain rewriting at all**. Their only other filter is
`sub_filter https:// ${WEBSITE_SCHEME}://`, a no-op in prod (both are `https`).

**Impact:** any absolute CMS URL (`cms.cps.gov.uk`, `cmsmodern…`, `cms-services…`,
or a Corsham/Farnborough IP) in the toolbar script, the case-detail tabs page, or
ajax-viewer content passes through untouched — a browser following one leaves the
proxy and goes direct to CMS. No visible trouble so far suggests those responses
mostly carry _relative_ URLs, but that is unverified (see the open question below).

#### Why we survive without it

1. **Relative URLs do the work.** The big one. A CMS page linking to `/CMS/Case/1`
   needs no rewriting — the browser resolves it against the current origin, which
   _is_ the proxy. Only **absolute** URLs need rewriting, and server-rendered apps
   mostly emit relative ones. There is simply little for the filter to find.
2. **These three are narrow special cases, not the bulk of traffic.** The volume
   goes through `^/CMS.*` (354) and `/` (520) — both of which rewrite correctly via
   `sub_filter`.
3. **They would have got it free.** Each of the three _overrides_ a route that
   already rewrites properly — `203`/`279` would otherwise match `354 ^/CMS.*`, and
   `556` would otherwise match `520 /`. The blocks exist to add something extra
   (button/script injection); in specialising, they opted into the njs filter
   _instead of_ `sub_filter` and silently lost the rewrite they would have
   inherited. There is no technical reason for that — `203` and `279` already run
   `sub_filter` and `js_body_filter` side by side. It reads as an accident of
   authorship, not a constraint.
4. **Proxied-CMS browsing may be a minority mode.** The rewriting only matters when
   a browser renders CMS _through_ the proxy. `nginx.js` calls that "primarily
   useful when users are using CMS delivered through this proxy", and the `/cinN`
   switching is dev/test-flavoured. If most prod users are on real CMS and only hop
   to the proxy for Polaris itself, the blast radius is small. _(Least certain of
   the four — needs someone who knows the deployment, not the code.)_

Net: the filter has been inert for as long as it has existed, and the damage is
bounded to whatever absolute CMS URLs happen to appear in exactly those three
responses. Plausibly none — which is why it went unnoticed.

#### Decision (2026-07-17): leave as a known bug

No change now. It is recorded here and pinned by its tests, so it cannot regress
further or be "fixed" by accident during the refactor.

When it is picked up, the options are:

1. **Delete** — behaviourally a **no-op today**, so zero risk, and it drops
   `replaceCmsDomains` / `__replaceCmsDomainsGeneric` / `__replaceContent` from the
   future `common.js` ([`../../../docs/PROXY.md` §6.2](../../../docs/PROXY.md)). But
   it makes the gap permanent.
2. **Repair** (`.` → `\.`) — restores the intent, but _starts_ rewriting three
   routes that currently don't. A real behaviour change.
3. **Replace with `sub_filter`** — give those three blocks the same three
   `sub_filter $upstreamCms*DomainName $host` lines the other six use; one
   mechanism everywhere, and `cmsMenuBarFilters` keeps only its (working) button
   injection. Note `/ajax/viewer/` targets `$websiteHostname`, not `$host`, so it
   needs `${WEBSITE_HOSTNAME}`.

**Note the inversion:** deleting is the _safe_ option and repairing is the _risky_
one — 2 and 3 are the same behaviour change and need the same evidence. **A real
capture of `uacdCDTabs.aspx` would settle it**: no absolute CMS URLs ⇒ 1 and 3 are
equivalent and 1 is free; any absolute CMS URLs ⇒ there is a live user-facing bug
on those three routes.

---

## D. Config smells and cleanup opportunities

### D2. 🟠 `/CMSModern/Files` dials a **domain**, unlike every other CMS block

**Where (live):** `nginx.conf:349` — `proxy_pass https://$upstreamCmsServicesDomainName;`
**Where (next):** `cms-proxy.conf` (`location ~ ^/CMSModern/Files.*`)

Every other CMS route proxies to a Corsham/Farnborough **IP** from
`var.cms_details`; this one alone resolves the CMS _Services_ domain by DNS. That
asymmetry is why the test harness needs a docker network alias for
`cms-services.cps.gov.uk` — and it means this route's availability depends on DNS
rather than the pinned IPs.

**Suggested fix:** confirm whether that is deliberate (CMS Services may genuinely
have no DC-pinned IP). If not, align it with the rest.
**DECIDED 2026-07-17: leave alone** — behaviour here is not well understood, so it
is out of scope for the refactor. Do not "tidy" it. The harness pins it via a
docker network alias, so any accidental change will show up as a red test.

### D4. 🟠 `/ajax//viewer/` — double slash in `proxy_pass`

**Where (live):** `nginx.conf:586` — `proxy_pass $proxyDestinationModern/ajax//viewer/$1$is_args$args;`
**Where (next):** `cms-proxy.conf` (`location /ajax/viewer/`)

With `merge_slashes off` set globally (`nginx.conf:50`), that literal `//` is sent
upstream as-is. Either deliberate (matching a CMS quirk) or a typo — worth a
comment either way.

### D10. ⚪ The root `/` catch-all proxies to CMS Modern

**Where (live):** `nginx.conf:520`
**Where (next):** `cms-proxy.conf` (`location /`)

Any unmatched path falls through to CMS Modern. Flagged in §4 as "has always been
a bit dodgy just being on the root". It means the proxy has no real 404 — unknown
paths become CMS requests.

### D11. 🟢 `/cpt` env-switch: next-gen fixes a live cookie-clearing bug

**Where (live):** `nginx.conf` (`location /cpt`, added by FCT2-18732)
**Where (next):** `cms-proxy.js` (`cinSwitch` / `_clearOtherEnvs`), `cms-proxy.conf` (`location /cpt`)
**Test:** `cms-proxy.unit.test.js` + `cms-proxy.integration.test.js` — "/cpt clears CIN3 + MOD, NOT its own CPT"

The live `/cpt` block (copy-pasted from the CIN blocks) regressed: it **missed CIN3**
(neither pool nor LB cleared) and wrongly **cleared its own CPT** LB cookies. Our
handler clears "every env EXCEPT the target", so it's correct by construction — it
emits CIN2–CIN5 (pool + LB) + MOD and leaves CPT alone. The live block has since been
fixed to match (same commit as this port). Also note two deliberate parity carries
from the live config, NOT bugs we introduced: `cmo` is detected from the `mod` token
and has **no upstream config** yet (see [`../common/cms-detection.js`]), and the
`cpt`/`cmo` `CASEWORK_TOOLS_URL` sub_filters hard-code the UAT host (qa→uat) — both to
be addressed wholesale later.
