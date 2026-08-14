# QUIRKS — case-locking

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status key
and the whole-set index.

**NEW-GEN, outside the golden master.** These routes do not exist in the live monolith;
this feature only adds new paths and touches no existing route, so it is not pinned against
the before/after parity suite. The `CL`-series numbering is local to this feature.

Two source origins in the reference repo (both `.ts` → plain `.js`):
- the SignalR routes (`/global-components/case-locking/…`) + negotiate filters — from
  `global-components.case-locking`;
- the presence adapters (`/global-components/presence/*`, `/global-components/presence-jsonp`)
  — from the `global-components.cms-auth-v2` lump, brought here because they are just thin
  adapters over the SAME presence API the SignalR routes front.

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

---

## Presence adapters (transports over the CMS presence API)

### CL6. 🟠 The `/presence/*` proxy must NOT have a runtime `resolver`

`location ~ ^/global-components/presence/(.*)$` uses a variable-less `proxy_pass`, so nginx
resolves `app-cms-presence-api.azurewebsites.net` at LOAD time via the system resolver
(`/etc/resolv.conf` → 127.0.0.11 → the private-endpoint IP). Adding a runtime
`resolver ${WEBSITE_DNS_SERVER}` makes it NXDOMAIN on the privatelink name and 502 — so it is
deliberately omitted (carried over verbatim from the reference). **Divergence to reconcile:**
the case-locking `/api/(.*)` block *does* use a runtime `resolver` for the *same* upstream
(with a URI in its `proxy_pass`). Both work in the reference env; worth confirming which DNS
path each takes in QA/prod and aligning them before graduation.

### CL7. 🔴 Presence JSONP token: cookie value (currently the DEV token) via `_PRESENCE_USE_ID_TOKEN`

`handlePresenceJsonp` sends `Authorization: Bearer <token>` to the presence API.
`_PRESENCE_USE_ID_TOKEN` (**true**) makes it prefer the `cms-auth-id-token` **cookie** value
(decoded), falling back to the static **alg:none** dev bearer when there's no cookie. This
handler is that cookie's consumer (closes drop2's QUIRK E3 loop). To let the whole round-trip be
proven **before** the backend validates real id-tokens, `auth-handover.drop2.entra` currently
writes the **dev token** into the cookie (its `PRESENCE_COOKIE_TOKEN`) — so cookie-path and
fallback both send a token the backend accepts, but the cookie-read path is genuinely exercised.
When the backend accepts real tokens, drop2 swaps the cookie to the real id_token; this stays
true. **Before prod:** drop the dev-bearer fallback / fail closed once the real-token path is live.
NOTE (IE jars): the cookie is only visible to this handler when it was **set in IE mode** — see
drop2 QUIRK E11.

### CL8. 🟠 JSONP callback is the one non-negotiable XSS guard

The `?callback=` value is reflected verbatim into an executable `text/javascript` response, so
`handlePresenceJsonp` rejects anything but a bare JS identifier (`/^[A-Za-z_$][A-Za-z0-9_$]*$/`)
with a 400. Do not relax it. (`?op=` is looked up with a `hasOwnProperty` guard so it can't
reach a prototype member.) Pinned by the unit test.
