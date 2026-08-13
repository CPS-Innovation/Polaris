# QUIRKS — case-locking

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status key
and the whole-set index.

**NEW-GEN, outside the golden master.** These routes do not exist in the live monolith;
this feature only adds new paths (all under `/global-components/case-locking/`) and touches
no existing route, so it is not pinned against the before/after parity suite. The `CL`-series
numbering is local to this feature. Ported from global-components
`infra/proxy/config/global-components.case-locking` (`.ts` → plain `.js`).

---

### CL1. 🟠 Cross-feature dependency on `$cors_origin`

`case-locking.conf` reads `$cors_origin` (in the CORS `add_header`s and the preflight) but
does not declare it — an nginx variable may be `js_set` only once per server, and
`features/global-components` already does (`js_set $cors_origin gloco.readCorsOrigin`). So
**global-components must be present** for case-locking to boot. Feature confs are included
alphabetically, so `case-locking.conf` is parsed *before* `global-components.conf`; nginx
registers `js_set` variables across the whole config before resolving references, so the
forward reference is fine (verified by the `--next` boot). Declared here as a
`FEATURE DEPENDENCY:` comment in the conf.

### CL2. ⚪ Hard-coded upstream hostnames (PoC)

The SignalR Service host (`sr-cms-presence.service.signalr.net`) and the presence API host
(`app-cms-presence-api.azurewebsites.net`) are baked into `proxy_pass` / `proxy_set_header
Host`, carried over verbatim from the reference PoC. Candidates for app-setting/env-var- isation
(like the other upstreams) before this graduates beyond QA.

### CL3. ⚪ `proxy_ssl_verify off` on external upstreams

The SignalR Service, presence API and blob-storage proxies disable upstream TLS verification
(reference default). For external endpoints this should be `on` with a trusted CA bundle
before prod.

### CL4. ⚪ Feature-local preflight duplicates the shared one (+2 SignalR headers)

`@gloco_preflight_case_locking` mirrors `features/global-components`'s `@gloco_preflight` but
adds `X-Requested-With` and `X-SignalR-User-Agent` to the allow-headers (both sent by the
SignalR client on negotiate POSTs). The reference kept it separate because it couldn't modify
the shared handler on the deployed box; here we *could* widen the shared one, but it is kept
feature-local so case-locking stays self-contained and removable. Revisit if the duplication
grows.

### CL5. 🟠 Within-file location ordering is load-bearing

`^…/case-locking/api/sr/(.*)$` MUST precede `^…/case-locking/api/(.*)$` — nginx matches
regex locations top-to-bottom and the `/api/` regex would otherwise swallow `/api/sr/`. Kept
in that order; do not reorder.
