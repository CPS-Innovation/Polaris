# QUIRKS — cms-augmentation

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status key
and the whole-set index.

**NEW-GEN, outside the golden master.** Brought across from global-components
`global-components.cms-auth-v2`: the `uaglCMS.aspx` injection route, the client script
(`cms-augmentation.js`, was `cms-auth-v2-client.js`) and the presence relay
(`presence-relay.html`, was `cms-presence-relay.html`), the two assets now served directly by
nginx from this folder instead of the `/global-components/<env>/` blob route. The `AU`-series
numbering is local to this feature.

---

### AU1. 🟠 Reuse cms-proxy's getters via distinct `$aug*` vars (no own module)

The uaglCMS route needs the per-CMS-env upstream getters. Because this feature already fully
depends on cms-proxy (AU3 — it overrides cms-proxy's `^/CMS.*` catch-all and mirrors its
proxying), it does **not** keep a feature-local getter copy; it reuses cms-proxy's already-global
`cmsProxy` binding directly. The one real constraint is that `js_set` **variable** names are
server-global and `[emerg] redeclare` if the same name is bound to a different function — so we use
**distinct** `$aug*` names but point them at `cmsProxy.*` (a unique-name, set-once js_set, which is
fine; cms-proxy itself repeats `js_set $proxyDestination cmsProxy.proxyDestinationCorsham` across
many blocks). Do **not** reuse cms-proxy's `$proxyDestination` / `$upstreamCms*` **variable** names —
*that* would redeclare. (Earlier this feature carried its own `cms-upstream.js` copy bound as
`cmsAug`; removed as needless duplication once the total cms-proxy coupling was clear.)

### AU2. 🟠 uaglCMS path is version-pinned (`/CMS.24.0.01/uaglCMS.aspx`)

An **exact** `location =` for the current CMS version. Exact-match beats cms-proxy's `^/CMS.*`
catch-all, so the injection fires only for this URL; on a CMS upgrade (e.g. `24.0.02`) it stops
matching and the shell falls back to the catch-all (no injection) rather than breaking. A regex
`~ ^/CMS\.[^/]+/uaglCMS\.aspx$` would survive upgrades; the reference chose exact as safest.
Revisit at graduation.

### AU3. 🟠 This feature OVERRIDES an existing CMS path

Unlike case-locking (disjoint new paths only), the exact uaglCMS location intercepts a URL that
otherwise hits cms-proxy's `^/CMS.*` catch-all, to add the `<script>` injection. It proxies
identically otherwise. Not switch-gated (the reference isn't); the whole `--next` config is
parked until cutover, so it only takes effect then. Gate it if a per-user/gradual rollout is
wanted (cf. the auth-handover enrolment cookie).

### AU4. 🟠 `.html` assets now ride the deploy fileset

`presence-relay.html` is a static asset. The blob fileset (`app-service-proxy.tf`), the docker
staging (`stage-config.sh`) and the hand-deploy script (`deploy-non-ddei.local.sh`) were
`.conf`/`.js` only; all three now also include `features/**/*.html` (deployed as-is, no
`.template` suffix → not envsubst'd). If a future `.html` must NOT ship, revisit those globs.

### AU5. 🟠 Client auth entry now `/polaris`; iframe-terminal refinement optional (drop2 E8)

`cms-augmentation.js` is brought across essentially verbatim, with two Polaris edits:
`PRESENCE_RELAY_URL` → the directly-served `/presence-relay.html`, and `POLARIS_PATH` →
`/polaris` (was the reference's `/polaris-v2`, which doesn't exist here). `/polaris` runs the
full Polaris chain (→ `/init` → `/auth-refresh-inbound` → drop2 `/init-entra`), which captures the
CMS cookies, populates the Entra store and sets the id-token cookie — enough for the hidden auth
iframe to do its job. The optional refinement of threading `terminal=iframe` so the flow ends on
drop2's static terminal (instead of a redirect the framed nav just absorbs) remains **drop2
QUIRK E8**; not required for the store to populate.

### AU6. ⚪ Presence relay reads its token from localStorage; drop2 sets a cookie

`presence-relay.html` (the inactive transport — the client defaults to `PRESENCE_METHOD =
"jsonp"`) reads the id-token from same-origin `localStorage`, whereas drop2 sets it as an
HttpOnly cookie. The **active** JSONP path reads the cookie server-side (case-locking
`handlePresenceJsonp`), so this only matters if the relay transport is re-enabled. Left verbatim.
