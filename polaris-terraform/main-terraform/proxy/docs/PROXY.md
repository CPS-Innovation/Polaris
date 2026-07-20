# PROXY.md — The Polaris CMS Proxy

> Working notes for a refactor of the nginx CMS proxy. Start here (with
> [`SYSTEM.md`](./SYSTEM.md)) when reorienting. Focused on **which Terraformed
> AppSettings drive the proxy** and how the proxy is put together.
>
> Companion docs: [`QUIRKS.md`](./QUIRKS.md) — every bug/oddity found while
> building the test harness; [`PLAN.md`](./PLAN.md) — the phased roadmap.
> The harness itself lives in [`../tests`](../tests) (see [`../README.md`](../README.md)).

## 1. What the proxy is

A single **nginx** instance running as an Azure Linux Web App (`…-cmsproxy`,
nginx docker image), acting as a multi-purpose reverse proxy that unifies several
backends behind one hostname:

- the **CMS** (Classic + Modern, across environments and two data centres),
- the **Polaris SPA** (`polaris-ui`),
- the **Materials UI**,
- the **gateway API** (`fa-polaris…`),
- **DDEI** auth-handover, and
- **global-components** (shared cross-app UI + Managed Data Store).

Config is assembled at container start by `envsubst` substituting `${VAR}`
placeholders in the `.conf` templates, plus three server-side **njs** (nginx
JavaScript) modules for request-time logic. It is deployed and configured entirely
from `polaris-terraform/main-terraform/`.

### File tour (`polaris-terraform/main-terraform/`)

| File                     | Role                                                                                                                                                |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app-service-proxy.tf`   | Terraform for the proxy web app + all its `app_settings`; uploads the config files below as storage blobs. Has a `-staging` twin.                   |
| `nginx.conf`             | The nginx server: all `location` blocks, rate limits, `sub_filter` rewrites, `js_var`/`js_import` wiring. `envsubst`-templated.                     |
| `nginx.js`               | njs: auth-handover endpoints (`/polaris`, `/init`, `/auth-refresh-outbound`) + the `Cms-Session-Hint` cookie. Only module that reads `process.env`. |
| `cmsenv.js`              | njs: picks the CMS upstream (DEFAULT/CIN2/CIN4/CIN5 × Corsham/Farnborough) from the `__CMSENV`/load-balancing cookie.                               |
| `global-components.conf` | nginx: `/global-components/*` routes (MDS API, blob assets, analytics, state, case-review-redirect). `include`d by `nginx.conf`.                    |
| `global-components.js`   | njs: session-hint, state, CORS, navigate-cms, case-review-redirect handlers. Hardcoded CORS allow-lists (no env vars).                              |
| `polaris-script.js`      | **Client-side** browser JS injected into CMS pages (P / Materials buttons). Consumes no server-side env vars.                                       |

## 2. AppSettings classification grid

Every `app_setting` on the proxy (`app-service-proxy.tf`), on a single axis: **is it
used only inside the proxy, or not?**

**Legend**

- 🟢 **Proxy-only** — set on the proxy and consumed only by `nginx.conf`/njs. This
  includes settings whose _value_ merely names another app as an **upstream target**
  (DDEI, gateway API, SPA, Materials UI, WM-MDS, blob SAS) — nginx dials that host, but
  no other component reads the setting. Owning/moving these is a purely proxy-local change.
- 🟡 **Shared with app** — the setting is proxy-only, **but its terraform variable also
  configures another component**, so a change ripples beyond the proxy. Just two:
  `APP_SUBFOLDER_PATH` (`var.polaris_ui_sub_folder` → SPA `redirect_uris` + UI `PUBLIC_URL`)
  and `CPS_GLOBAL_COMPONENTS_BLOB_STORAGE_DOMAIN` (`var.cps_global_components` → SPA `script_url`).
- 🔴 **Fleet-platform** — standard Azure/plumbing set on _every_ component (env tag, DNS,
  App Insights, content-share, slot-swap); not proxy business logic.

| AppSetting                                        | Terraform source            | Scope                  | Consumed by (feature id)                     |
| ------------------------------------------------- | --------------------------- | ---------------------- | -------------------------------------------- |
| `HostType`                                        | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `ENVIRONMENT`                                     | var.env                     | 🔴 **Fleet-platform**  | — declared, unused                           |
| `WEBSITE_CONTENTOVERVNET`                         | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_DNS_SERVER`                              | var.dns_server              | 🔴 **Fleet-platform**  | resolver: 5 🔀, 6 🔌, GC                     |
| `WEBSITE_DNS_ALT_SERVER`                          | var.dns_alt_server          | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_SCHEME`                                  | literal "https"             | 🟢 **Proxy-only**      | 2 🤝, 3 🧭, 4 📁, 5 🔀, 6 🔌 (cross-cutting) |
| `APPINSIGHTS_INSTRUMENTATIONKEY`                  | app-insights                | 🔴 **Fleet-platform**  | —                                            |
| `APPINSIGHTS_PROFILERFEATURE_VERSION`             | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `APPINSIGHTS_SNAPSHOTFEATURE_VERSION`             | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `APPLICATIONINSIGHTS_CONFIGURATION_CONTENT`       | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `APPLICATIONINSIGHTS_CONNECTION_STRING`           | app-insights                | 🔴 **Fleet-platform**  | —                                            |
| `ApplicationInsightsAgent_EXTENSION_VERSION`      | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `DiagnosticServices_EXTENSION_VERSION`            | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `InstrumentationEngine_EXTENSION_VERSION`         | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `SnapshotDebugger_EXTENSION_VERSION`              | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `XDT_MicrosoftApplicationInsights_BaseExtensions` | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `XDT_MicrosoftApplicationInsights_Mode`           | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `XDT_MicrosoftApplicationInsights_PreemptSdk`     | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_CONTENTAZUREFILECONNECTIONSTRING`        | storage                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_CONTENTSHARE`                            | file share                  | 🔴 **Fleet-platform**  | —                                            |
| `DEFAULT_UPSTREAM_CMS_IP_CORSHAM`                 | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `DEFAULT_UPSTREAM_CMS_MODERN_IP_CORSHAM`          | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `DEFAULT_UPSTREAM_CMS_IP_FARNBOROUGH`             | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `DEFAULT_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH`      | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME`                | var.cms_details             | 🟢 **Proxy-only**      | 1 🚀, 2 🤝, 5 🔀, 6 🔌                       |
| `DEFAULT_UPSTREAM_CMS_SERVICES_DOMAIN_NAME`       | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `DEFAULT_UPSTREAM_CMS_MODERN_DOMAIN_NAME`         | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN2_UPSTREAM_CMS_IP_CORSHAM`                    | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN2_UPSTREAM_CMS_MODERN_IP_CORSHAM`             | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN2_UPSTREAM_CMS_IP_FARNBOROUGH`                | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN2_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH`         | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN2_UPSTREAM_CMS_DOMAIN_NAME`                   | var.cms_details             | 🟢 **Proxy-only**      | 1 🚀, 2 🤝, 5 🔀, 6 🔌                       |
| `CIN2_UPSTREAM_CMS_SERVICES_DOMAIN_NAME`          | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN2_UPSTREAM_CMS_MODERN_DOMAIN_NAME`            | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN4_UPSTREAM_CMS_IP_CORSHAM`                    | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN4_UPSTREAM_CMS_MODERN_IP_CORSHAM`             | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN4_UPSTREAM_CMS_IP_FARNBOROUGH`                | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN4_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH`         | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN4_UPSTREAM_CMS_DOMAIN_NAME`                   | var.cms_details             | 🟢 **Proxy-only**      | 1 🚀, 2 🤝, 5 🔀, 6 🔌                       |
| `CIN4_UPSTREAM_CMS_SERVICES_DOMAIN_NAME`          | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN4_UPSTREAM_CMS_MODERN_DOMAIN_NAME`            | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN5_UPSTREAM_CMS_IP_CORSHAM`                    | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN5_UPSTREAM_CMS_MODERN_IP_CORSHAM`             | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN5_UPSTREAM_CMS_IP_FARNBOROUGH`                | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN5_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH`         | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN5_UPSTREAM_CMS_DOMAIN_NAME`                   | var.cms_details             | 🟢 **Proxy-only**      | 1 🚀, 2 🤝, 5 🔀, 6 🔌                       |
| `CIN5_UPSTREAM_CMS_SERVICES_DOMAIN_NAME`          | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CIN5_UPSTREAM_CMS_MODERN_DOMAIN_NAME`            | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `APP_ENDPOINT_DOMAIN_NAME`                        | as_web_polaris              | 🟢 **Proxy-only**      | 4 📁                                         |
| `MATERIALS_APP_ENDPOINT_DOMAIN_NAME`              | materials app               | 🟢 **Proxy-only**      | 4 📁                                         |
| `APP_SUBFOLDER_PATH`                              | var.polaris_ui_sub_folder   | 🟡 **Shared with app** | 4 📁                                         |
| `API_ENDPOINT_DOMAIN_NAME`                        | fa_polaris                  | 🟢 **Proxy-only**      | 4 📁                                         |
| `AUTH_HANDOVER_ENDPOINT_DOMAIN_NAME`              | ddei                        | 🟢 **Proxy-only**      | 2 🤝                                         |
| `DDEI_ENDPOINT_DOMAIN_NAME`                       | ddei                        | 🟢 **Proxy-only**      | 2 🤝                                         |
| `DDEI_ENDPOINT_FUNCTION_APP_KEY`                  | ddei host key               | 🟢 **Proxy-only**      | 2 🤝                                         |
| `SAS_URL_DOMAIN_NAME`                             | storage (sa)                | 🟢 **Proxy-only**      | 4 📁, X 🗑️                                   |
| `ENDPOINT_HTTP_PROTOCOL`                          | literal "https"             | 🟢 **Proxy-only**      | 2 🤝, 3 🧭, 4 📁, 5 🔀, 6 🔌 (cross-cutting) |
| `NGINX_ENVSUBST_OUTPUT_DIR`                       | literal                     | 🟢 **Proxy-only**      | — build                                      |
| `FORCE_REFRESH_CONFIG`                            | md5(configs)                | 🟢 **Proxy-only**      | — deploy                                     |
| `CMS_RATE_LIMIT_QUEUE`                            | literal                     | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CMS_RATE_LIMIT`                                  | literal                     | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `AUTH_HANDOVER_WHITELIST`                         | var.auth_handover_whitelist | 🟢 **Proxy-only**      | 2 🤝                                         |
| `WM_MDS_BASE_URL`                                 | wm-mds                      | 🟢 **Proxy-only**      | global-components                            |
| `WM_MDS_ACCESS_KEY`                               | kv secret                   | 🟢 **Proxy-only**      | global-components                            |
| `CPS_GLOBAL_COMPONENTS_BLOB_STORAGE_DOMAIN`       | var.cps_global_components   | 🟡 **Shared with app** | global-components                            |
| `WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS`    | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS`      | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_SLOT_MAX_NUMBER_OF_TIMEOUTS`             | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_SWAP_WARMUP_PING_PATH`                   | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_SWAP_WARMUP_PING_STATUSES`               | literal                     | 🔴 **Fleet-platform**  | —                                            |
| `WEBSITE_WARMUP_PATH`                             | literal                     | 🔴 **Fleet-platform**  | —                                            |

> **`WEBSITE_HOSTNAME`** is consumed by nginx/njs (`server_name`, `$websiteHostname`,
> App-Insights host rewrite) but is **Azure-injected**, so it is _not_ a key in the
> `app_settings` block — listed here for completeness only.

**Tally (71 keys total):** Proxy-only = 44 (24 the CMS-upstream family),
Shared with app = 2, Fleet-platform = 25.

### How each value actually reaches nginx (3 injection paths — easy to trip on)

1. **`envsubst` `${VAR}`** — the default; most keys are substituted into `.conf`
   templates at container start.
2. **`js_var` bridge** — the 28 CMS-upstream vars are declared `js_var $...` in
   `nginx.conf` so njs (`cmsenv.js`) can read them as `r.variables[...]`.
3. **`process.env`** — only two keys are read this way, and only in `nginx.js`:
   `AUTH_HANDOVER_WHITELIST` and `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME`.

## 3. Endpoint / responsibility map

- **Health / util:** `= /` ("Polaris Proxy is online"), `/robots933456.txt`,
  `/polaris-script.js`.
- **Polaris app proxying:** `/${APP_SUBFOLDER_PATH}` → SPA; `/materials`, `/materials-ui`,
  `/materials-ui/…` → Materials UI (and the `/materials` handover chain); `/api/` →
  gateway API (with gateway/SAS domains rewritten back to `$host`); `/sas-url/` → blob SAS.
- **Auth handover (njs `nginx.js`):**
  - `/polaris` → `polarisAuthRedirect` — simulates `cms.cps.gov.uk/polaris` for
    proxied-CMS sessions; packs cookie/referer and redirects to `/init`.
  - `/init` → `appAuthRedirect` — validates `r` against `AUTH_HANDOVER_WHITELIST`, sets
    the `Cms-Session-Hint` cookie, appends the CMS cookie as `cc`, then redirects (or 403).
  - `/auth-refresh-outbound` → `handleAuthRefreshOutbound` — resolves the CMS `/polaris`
    handover domain from `Cms-Session-Hint` (or `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME`), forces
    IE mode, 302s.
  - `/auth-refresh-inbound`, `/auth-refresh-termination`, `/auth-refresh-cms-modern-token`
    → DDEI (`AUTH_HANDOVER_ENDPOINT_DOMAIN_NAME`).
  - `/launch/{cms,cin2..cin5}` (+ `-proxy` variants) → static 302s into OutSystems/CMS chains.
- **CMS proxying (njs `cmsenv.js` chooses upstream by `__CMSENV`/LB cookie):**
  Classic (`~ ^/CMS.*`), Modern (`~ ^/CMSModern/Files.*`, `/`), script/button injection
  (`uainGeneratedScript.aspx`, `uainMenuBar.js`, `uacdCDTabs.aspx`), env switch
  (`/cin2`,`/cin3`,`/cin4`,`/cin5` set `__CMSENV` + clear LB cookies), dev-login
  (`/dev-login/`, `/api/dev-login-full-cookie/`), DC-internal
  (`/internal-implementation/{corsham,farnborough}[/modern]/`), IE navigation
  (`/api/navigate-cms`, `/navigate-cms-close`).
- **Global components (`global-components.conf` + njs `global-components.js`):**
  `/global-components/cms-session-hint`, `…/api/*` → MDS (`WM_MDS_*`), `…/{dev|test|prod}/*`
  → blob (`CPS_GLOBAL_COMPONENTS_BLOB_STORAGE_DOMAIN`), `…/state/*`, `…/analytics/*`,
  `…/navigate-cms`, `/case-review-redirect/{osSubdomain}/{envFolder}`.

Cross-cutting: security headers (HSTS, `X-Frame-Options`, `Cache-Control: no-store`, …),
IE-vs-Edge negotiation (`$ieaction`, `X-InternetExplorerMode`), and pervasive `sub_filter`
rewriting of upstream domains / `https://` back to the proxy host.

## 4. Location blocks (`nginx.conf`) → feature

Every `location` in `nginx.conf`, in file order. **Feature** = my best read of what
it's _for_; blank / `(?)` where I'm unsure — edit freely, then we refine the names and
decide which to drop. Scoped to `nginx.conf` only; the `include global-components*.conf`
at line 947 adds more locations (`global-components.conf`) — a later pass.

| Line | `location`                                      | Feature (draft — edit me)                        | Feature id                 | Notes                                                                             |
| ---: | ----------------------------------------------- | ------------------------------------------------ | -------------------------- | --------------------------------------------------------------------------------- |
|  122 | `= /`                                           | Health check (readiness "online")                | 0 💓 - health check        |                                                                                   |
|  127 | `= /api/navigate-cms`                           | CMS navigation (IE-mode iframe)                  | 3 🧭 - rCMS feature        | Candidate to move to global components.                                           |
|  143 | `= /navigate-cms-close`                         | CMS navigation (close helper)                    | 3 🧭 - rCMS feature        | Candidate to move to global components. Why not /api/navigate-cms-close?          |
|  152 | `= /robots933456.txt`                           | Azure platform probe                             | 0 💓 - health check        | Could be dropped, might be to do with SSL provisioning?                           |
|  157 | `/polaris-script.js`                            | Serve injected client script                     | 5 🔀 - CMS proxy           | Inject buttons                                                                    |
|  164 | `~ ^/CMS\..*/Includes/uainGeneratedScript.aspx` | CMS button injection (casework-tools URL)        | 5 🔀 - CMS proxy           |                                                                                   |
|  203 | `~ ^/CMS\..*/Noexpiry/Toolbar/uainMenuBar.js`   | CMS button injection (Polaris/Materials buttons) | 5 🔀 - CMS proxy           | Inline js could be put in njs file                                                |
|  279 | `~ ^/CMS\..*/Case/uacdCDTabs.aspx`              | CMS script injection (polaris-script.js)         | 5 🔀 - CMS proxy           |                                                                                   |
|  309 | `~ ^/CMSModern/Files.*`                         | CMS Modern proxy (Files / Services)              | 5 🔀 - CMS proxy           |                                                                                   |
|  354 | `~ ^/CMS.*`                                     | CMS Classic proxy (general fallback)             | 5 🔀 - CMS proxy           |                                                                                   |
|  400 | `/cin3`                                         | CMS env switch (cin3)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  430 | `/cin2`                                         | CMS env switch (cin2)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  460 | `/cin4`                                         | CMS env switch (cin4)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  490 | `/cin5`                                         | CMS env switch (cin5)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  520 | `/`                                             | CMS Modern proxy (root)                          | 5 🔀 - CMS proxy           | Has always been a bit dodgy just being on the root                                |
|  556 | `/ajax/viewer/`                                 | CMS Modern proxy (ajax viewer)                   | 5 🔀 - CMS proxy           |                                                                                   |
|  596 | `/auth-refresh-outbound`                        | Auth handover (outbound → CMS `/polaris`)        | 2 🤝 - auth handover       |                                                                                   |
|  604 | `/polaris`                                      | Auth handover (simulated CMS `/polaris`)         | 5 🔀 - CMS proxy           | ... and auth 2 🤝 - auth handover                                                 |
|  616 | `/launch/cms`                                   | Launch redirect (CMS)                            | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  619 | `/launch/cin2`                                  | Launch redirect (cin2)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  622 | `/launch/cin3`                                  | Launch redirect (cin3)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  625 | `/launch/cin4`                                  | Launch redirect (cin4)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  628 | `/launch/cin5`                                  | Launch redirect (cin5)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  631 | `/launch/cms-proxy`                             | Launch redirect (CMS, proxied session)           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  634 | `/launch/cin2-proxy`                            | Launch redirect (cin2, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  637 | `/launch/cin3-proxy`                            | Launch redirect (cin3, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  640 | `/launch/cin4-proxy`                            | Launch redirect (cin4, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  643 | `/launch/cin5-proxy`                            | Launch redirect (cin5, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  648 | `/init`                                         | Auth handover (whitelist + `Cms-Session-Hint`)   | 2 🤝 - auth handover       |                                                                                   |
|  662 | `/auth-refresh-inbound`                         | Auth handover (inbound → DDEI)                   | 2 🤝 - auth handover       |                                                                                   |
|  666 | `/auth-refresh-termination`                     | Auth handover (in-situ termination → DDEI)       | 2 🤝 - auth handover       |                                                                                   |
|  674 | `/auth-refresh-cms-modern-token`                | Auth handover (CMS Modern token → DDEI)          | 2 🤝 - auth handover       | Could be refactored away                                                          |
|  679 | `/${APP_SUBFOLDER_PATH}`                        | Polaris SPA proxy                                | 4 📁 - CWA/materials       | reads polaris-ui                                                                  |
|  687 | `/materials`                                    | Materials UI (handover redirect)                 | 4 📁 - CWA/materials       |                                                                                   |
|  697 | `= /materials-ui`                               | Materials UI (redirect → `/materials-ui/`)       | 4 📁 - CWA/materials       |                                                                                   |
|  701 | `~ ^/materials-ui/[^/]+/[^/]+/materials$`       | Materials UI proxy (deep-link) `(?)`             | 4 📁 - CWA/materials       |                                                                                   |
|  709 | `/materials-ui/`                                | Materials UI proxy                               | 4 📁 - CWA/materials       |                                                                                   |
|  718 | `/api/`                                         | Gateway API proxy                                | 4 📁 - CWA/materials       |                                                                                   |
|  735 | `/sas-url/`                                     | Blob storage SAS proxy                           | X 🗑️ - redundant           | Can be deleted                                                                    |
|  741 | `/dev-login/`                                   | Dev login via DDEI (non-prod)                    | 2 🤝 - auth handover       | Allow devs to log into CMS without hitting proxy UI, is also 5 🔀 - CMS proxy     |
|  772 | `/api/dev-login-full-cookie/`                   | Dev login, full cookie (non-prod)                | 2 🤝 - auth handover       | Allow devs to log into CMS without hitting proxy UI, is also 4 📁 - CWA/materials |
|  803 | `/internal-implementation/corsham/`             | Internal CMS route (Corsham, Classic)            | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  838 | `/internal-implementation/corsham/modern/`      | Internal CMS route (Corsham, Modern)             | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  872 | `/internal-implementation/farnborough/`         | Internal CMS route (Farnborough, Classic)        | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  907 | `/internal-implementation/farnborough/modern/`  | Internal CMS route (Farnborough, Modern)         | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  940 | `/v2/`                                          | Telemetry (App Insights ingestion)               | 4 📁 - CWA/materials       |                                                                                   |

## 5. Learnings / gotchas

- **Three injection paths** (§2) — always confirm _how_ a var is consumed before moving it.
- **"DEFAULT" means CIN3.** The `DEFAULT_*` CMS upstream set is the CIN3/production CMS.
- **`FORCE_REFRESH_CONFIG`** is an md5 of `nginx.conf`, `nginx.js`, `cmsenv.js`,
  `polaris-script.js`, `global-components.conf`, `global-components.js`. Editing any of
  them changes this setting and forces the app to pick up new config on deploy.
- **Every proxy `app_setting` is duplicated in the `lifecycle.ignore_changes` block.**
  Add/rename a setting → update it in **two** places in `app-service-proxy.tf` (and again
  in the `-staging` twin).
- **`-staging` twin.** `app-service-proxy-staging.tf` mirrors this file for the slot.
- **Reauth is split-brain (reference).** The UI holds the redirect _strings_
  (`REACT_APP_REAUTH_*`, case-review, bulk-um) while the proxy holds
  `AUTH_HANDOVER_WHITELIST` and _implements_ the `/auth-refresh-*`, `/polaris`, `/init`
  endpoints. Note `AUTH_HANDOVER_WHITELIST` itself is **Proxy-only** (nothing else reads
  it); the "split" is about the auth-handover _behaviour_ living in two components at
  once, not a shared setting — see [`SYSTEM.md`](./SYSTEM.md) §4.

## 6. Target architecture — feature-sliced config (refactor direction)

**Goal.** One nginx sub-config `include` per feature id (§4), and one njs module per
feature **where a feature needs request-time JS** — driven off `include`s the way
`global-components.conf` already is. Avoid a catch-all script, with **one deliberate
exception** (`common.js`, below) that the code genuinely forces.

### 6.1 Where today's njs files land

| Today | Concern | Target |
|---|---|---|
| `nginx.js` | auth handover (feat **2**) — already single-concern | → `auth-handover.js` |
| `cmsenv.js` → `getDomainFromCookie` | navigate-cms (feat **3**), self-contained (own regex, no shared core) | → `navigate-cms.js` |
| `cmsenv.js` → everything else | CMS-env resolution + upstream rewrite, shared by feat **5**, **6**, and the dev-login part of **2** | → **`common.js`** (the one shared module) |
| `polaris-script.js` | client-side button script, served/injected under feat **5** (buttons drive **1** & **4**) | keep as a static asset alongside the feat-5 include (it is browser JS, not njs) |

### 6.2 The one shared module — `common.js` (unavoidable)

Every CMS-proxy / internal / dev-login path funnels through `__getCmsEnvInternal`
(cookie → `default/cin2/cin4/cin5`) and the `r.variables[cmsEnv + 'UpstreamCms…']`
lookup convention. Features **5** and **6** even share the same getters, and 6's
`*Internal` fns just call 5's. Splitting per-feature would force **three copies** of the
cin logic (drift risk — recall cin5 was added later). So this stays one module.

Keep it a **single-responsibility primitive** ("resolve/rewrite the CMS upstream for the
current environment"), _not_ a utils junk-drawer. It holds: `getCmsEnv`, the
`proxyDestination*` / `upstreamCms*` getters, `replaceCmsDomains*`, `__replaceContent`.

> Later optimisation to shrink even this: move the cookie→env detection into an nginx
> `map $http_cookie $cms_env { ~*cin2 cin2; ~*cin4 cin4; ~*cin5 cin5; default default; }`
> in the parent, so per-feature njs just reads `r.variables.cms_env` and `common.js`
> loses its detection half. Optional; not required to start.

### 6.3 What always stays in the parent `nginx.conf` (never per-feature)

The server-scope preamble: `js_import` of every module, the 31 `js_var` upstream vars
(L88–119), `limit_req_zone` ×2, `resolver` (or the future `map`), security-header
defaults, and the `include` list. Plus the **ordering contract**: all CMS regexes
(feature-5 injectors + the `^/CMS.*` catch-all) live together in the feature-5 include,
so include order stays free (see §5 / precedence notes).

### 6.4 Proposed layout

```
nginx.conf                 # parent: preamble + ordered includes only
common.js                  # SHARED primitive: CMS-env resolution + upstream rewrite
features/
  00-health.conf
  01-app-launch.conf
  02-auth-handover.conf    + auth-handover.js     (was nginx.js)
  03-rcms.conf             + navigate-cms.js      (getDomainFromCookie)
  04-cwa-materials.conf
  05-cms-proxy.conf        (+ polaris-script.js asset; uses common.js — holds ALL CMS regexes)
  06-polaris-ddei.conf     (uses common.js)
  # X — redundant: delete (e.g. /sas-url, and the drop-candidates in §4)
global-components.conf     + global-components.js  (existing, separate track)
```

Cross-feature references are fine and expected: the `/polaris` location (feat-5 conf)
calls `auth-handover.js` (feat 2); feat-5 and feat-6 confs both `import` `common.js`.

### 6.5 Terraform mechanics — and the `md5` to make generic (do this FIRST)

Every config file today is wired **three times** by hand, per file:

1. an explicit `azurerm_storage_blob` (uploaded to the container mounted at
   `/etc/nginx/templates`; `.conf` → `*.conf.template` for `envsubst`, `.js` → plain and
   `js_import`ed as `templates/<x>.js`);
2. an entry in the hand-built `FORCE_REFRESH_CONFIG` md5 string
   (`app-service-proxy.tf:71` — note the stray `::` between `cmsenv.js` and
   `polaris-script.js`, and that it is trivially easy to forget a file);
3. (for `.js`) a `js_import` line, plus the setting in `lifecycle.ignore_changes`.

Slicing into ~10 feature files this way = ~10 manual blob resources + 10 hash entries.
**Genericise before slicing** so "add a feature file" costs zero terraform edits:

```hcl
locals {
  proxy_files = fileset(path.module, "{nginx.conf,common.js,features/**/*.conf,features/**/*.js}")
}
# one blob per file (map .conf -> *.conf.template, .js stays as-is)
resource "azurerm_storage_blob" "proxy_config" {
  for_each = local.proxy_files
  name     = endswith(each.value, ".conf") ? "${each.value}.template" : each.value
  source   = "${path.module}/${each.value}"
  content_md5 = md5(file("${path.module}/${each.value}"))
  # …storage_account_name / container / type = "Block" / depends_on…
}
# deterministic single hash of everything
FORCE_REFRESH_CONFIG = md5(join(":", [for f in sort(local.proxy_files) : filemd5("${path.module}/${f}")]))
```

This turns the migration into pure file adds/moves; terraform picks them up by glob.

### 6.6 Step 1 — empty-file scaffold (prove the loop before moving logic)

nginx `include` semantics make a zero-risk first slice possible:

- a **wildcard** include tolerates **zero** matches (what `global-components*.conf` already
  relies on) — an _explicit_ `include features/00-health.conf;` of a missing file, by
  contrast, **fails to boot**;
- nginx is happy to include an **empty** file (valid config, zero directives).

So you can wire the whole feature-include machinery and deploy it as a **behavioural
no-op**, exercising the entire blob → `envsubst` → `include` → `md5` pipeline before a
single `location` moves:

1. **Step 0 first** — genericise the blob/`md5` wiring (§6.5) so a `features/**` glob is
   picked up automatically.
2. Create `features/00-health.conf … 06-polaris-ddei.conf` as **empty** files (a comment
   header is fine).
3. Add `include features/*.conf;` to the parent `server {}` block (keep the existing
   `global-components*.conf` include).
4. Deploy; confirm behaviour is byte-for-byte unchanged and `nginx -t` passes. This green
   run also proves the subdir path — that `envsubst` emits
   `templates/features/*.conf.template` → `/etc/nginx/features/*.conf` on the real image.

Only after that do you start moving `location`s — **cut from parent → paste into feature
file**, one at a time, never duplicating a location (nginx won't boot on a dup). Because
the scaffold is a no-op, any behaviour change after a move is unambiguously that move.

Caveats: use a **wildcard** (not explicit) include; feature files are parsed in **server
context**, so `location {}` blocks only.

### 6.7 Constraints carried into the refactor

- **Regex ordering:** all CMS regexes stay in `05-cms-proxy.conf`, current relative order.
- **No duplicate literal locations** across includes (nginx won't boot).
- **Server-scope directives** (`js_import` / `js_var` / `map` / `limit_req_zone`) stay in
  the parent — feature files contain `location {}` blocks only.
- **`include` order** only matters for the CMS-regex file; wildcard `include features/*.conf`
  is safe because the colliding regexes are co-located (numeric filename prefixes give a
  stable order for free).

_(Step 0 = §6.5 md5/blob genericisation; Step 1 = §6.6 empty-file scaffold. Next: the
transition plan proper — moving logic feature-by-feature while keeping the build green
and terraform happy.)_

## 7. Testing the proxy — integration + unit (the refactor safety net)

**Why.** The whole §6 refactor (condense `cmsenv.js` → `common.js`, slice config into
feature includes) is only safe if we can prove behaviour is unchanged after each move.
The parallel repo `CPS/global-components/infra/proxy/` already runs exactly the kind of
harness we need — njs unit tests **and** Docker-based integration tests against the
rendered nginx config — so we mirror it here. The goal, in the user's words: **feed in
every relevant app setting ourselves, boot the existing config, then manipulate the env /
cookies to integration-test every path** — first to characterise today's behaviour, then
to guarantee parity through the transplant and the later condensing.

### 7.1 How the reference harness works (what we copy)

- **njs unit tests** (`config/**/tests/*.unit.test.ts`, run via `ts-node`): esbuild bundles
  the njs module to `.dist/*.bundle.js`, then `import()`s it; a `createMockRequest()` fakes
  the njs `r` object (`args`, `headersIn`, `headersOut`, `variables`, `return(code, body)`),
  calls the exported fns, and asserts on `r.returnCode` / `r.returnBody` /
  `headersOut['Set-Cookie']`. Types from `njs-types`. No nginx → fast pure-logic tests.
- **Integration tests** (`run-tests.sh` + `docker compose`):
  - `docker/Dockerfile.base` = `nginx:1.27-alpine` + `nginx-mod-http-js`; the config +
    njs are mounted in (`.conf` → `/etc/nginx/templates/*.conf.template` so `envsubst`
    renders them at start, `.js` mounted alongside for `js_import`).
  - a **`mock-upstream`** service (`docker/mock-server.js`, node on `:3000`/`:3443` with a
    self-signed cert) stands in for **every** backend via path routing + header checks +
    CORS simulation — so no real backend is needed.
  - env is fed via **`env_file: *.mock.env`** (the canonical variable manifest with test
    values) plus inline `environment:` — notably `WEBSITE_DNS_SERVER=127.0.0.11`, Docker's
    embedded DNS, so nginx's `resolver` can resolve service names like `mock-upstream`.
  - tests are plain node `fetch(url, { redirect: "manual" })` scripts asserting on
    `status` / `Location` / `Set-Cookie` / body, via a tiny shared `test-utils.js`
    (`assert` / `assertEqual` / `test`). `run-tests.sh` brings the stack up, waits on a
    health endpoint (`curl`), runs the node test file, tears down.

### 7.2 Proposed Polaris harness (what we add)

Location: `polaris-terraform/main-terraform/test/`, next to the config files.

```
test/
  docker/
    Dockerfile.base          # nginx + njs; mounts nginx.conf, nginx.js, cmsenv.js, polaris-script.js
    Dockerfile.mock
    docker-compose.yml       # nginx + mock-upstream
    cmsproxy.mock.env        # EVERY relevant app setting, test values → mock-upstream
    mock-server.js           # stands in for CMS / DDEI / gateway / SPA / Materials / WM-MDS / blob / SAS
  tests/
    auth-handover.integration.test.js   # feat 2: /init /polaris /auth-refresh-*
    cms-proxy.integration.test.js       # feat 5: /CMS.* inject + rewrites + /cinN switch
    …one file per feature id…
    cmsenv.unit.test.ts                 # __getCmsEnv, upstream getters, replaceCmsDomains
    nginx.unit.test.ts                  # auth-handover redirects
  run-tests.sh
  test-utils.js
```

Simpler than the reference in one way: **Polaris njs is already plain `.js`** (`nginx.js`,
`cmsenv.js`), so there's no TS→njs build step — mount them straight in. (esbuild is still
handy for the unit layer, to `import()` an ESM bundle.)

### 7.3 Feeding every app setting — the §2 grid *is* the manifest

`cmsproxy.mock.env` enumerates all ~50 vars with test values, taken straight from §2:

- Every upstream (all 28 CMS-upstream keys, DDEI, gateway, SPA, Materials, WM-MDS, blob,
  SAS) → `mock-upstream`. Set the `*_IP_*` vars to the service **name** `mock-upstream`
  too (nginx `resolver` resolves it — no literal IP needed for the mock).
- `WEBSITE_DNS_SERVER=127.0.0.11`, scheme/protocol, a test `AUTH_HANDOVER_WHITELIST`, the
  rate limits, `APP_SUBFOLDER_PATH=polaris-ui`; secrets (DDEI key, WM-MDS key) → dummies.

That realises the goal literally: we supply the env, boot the real `nginx.conf`, then vary
env / the `__CMSENV` cookie / cin domains to drive each path.

### 7.4 Golden-master ordering

Write the suite against the **current** config first — it encodes today's behaviour per
feature. Then every §6 step (0 md5, 1 scaffold, moving locations, `cmsenv.js` → `common.js`,
condensing) is validated by re-running the same green suite. Behaviour drift shows up as a
red test, pinned to the step that caused it.

### 7.5 Decisions & constraints

- **Full coverage is the acceptance bar.** A golden master only holds if it exercises
  _every_ path before we refactor — the target is a test for **every §4 location / every
  feature** (see §7.7). The build order below is **ordering, not scope**: we need it all in
  the end.
- **Fixtures are best-effort and replaceable.** Real CMS sample bodies _may_ be supplied
  but are **not** assumed. The mock therefore ships **synthesised minimal stand-ins** (a
  fake `uainMenuBar.js`, a fake case page containing the CMS domains) behind a clear fixture
  seam, and we drop in real captures if/when available **without touching the tests**.
- **One mock; data centres collapse.** Corsham and Farnborough both resolve to the single
  `mock-upstream` — behaviour parity holds; DC-routing itself isn't distinguished unless we
  add a second mock later.
- **CMS IPs → service name.** Point the `*_IP_*` vars at `mock-upstream`; `proxy_pass` via
  `resolver` tolerates a hostname (confirm no path assumes a literal IP).
- **HTTPS upstreams** (blob, SAS, CMS) → self-signed mock cert on `:3443`; tests set
  `NODE_TLS_REJECT_UNAUTHORIZED=0`; nginx skips verify for the mock.
- **Build order (ordering only, all required):** **2** auth handover → **5** CMS proxy +
  `cmsenv.js` (the refactor's blast radius) → **4** CWA/materials → **6** ddei → **1**
  launch → **3** rCMS → **0** health → **X** redundant (characterise _before_ deleting).

### 7.6 Reference mechanics to reuse verbatim

- **env → njs bridge.** nginx exposes `envsubst`'d vars to njs via `js_var $x ${VAR};` →
  njs reads `r.variables.x`. Polaris already does this for the 28 CMS-upstream vars +
  `endpointHttpProtocol` / `websiteHostname` (`nginx.conf` L88–119), so feeding the mock
  env "just works" for `cmsenv.js`. The two `process.env` reads in `nginx.js`
  (`AUTH_HANDOVER_WHITELIST`, `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME`) are supplied to unit
  tests by setting/restoring `process.env`.
- **Config in via `.template` + `envsubst`.** Mount `nginx.conf` →
  `/etc/nginx/templates/nginx.conf.template` and set `NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx`;
  the stock nginx entrypoint renders `${VAR}` at boot. Mount the `.js` njs files alongside
  (no `.template` suffix). Bind-mounts mean config edits need no image rebuild.
- **Mock upstream = header echo.** The default mock handler echoes received headers back as
  JSON — that's how you assert what nginx forwarded (Host, cookies, `cms-auth-values`,
  function keys). Add a few fixture routes returning representative bodies for the
  rewrite/inject paths.
- **`WEBSITE_DNS_SERVER=127.0.0.11`** (Docker's embedded DNS) so nginx's `resolver`
  resolves `mock-upstream`; point every upstream var at that service name.
- **Test-only conf via the include glob.** `nginx.conf` already ends with
  `include global-components*.conf;` — a matching-named file injects test-only nginx (e.g.
  a stub health/location) without editing the main config, exactly as the reference does
  with its `test-only.upgrade-shim.conf`.
- **Unit layer.** esbuild-bundle each njs module → `import()` → drive with
  `createMockRequest({ args, headersIn, headersOut, variables, return(), sendBuffer() })`;
  assert on `r.returnCode` / `r.returnBody` / `headersOut` / `sentBuffer`. Plain `.js`
  bundles fine (no TypeScript toolchain needed). Orchestrator is `run-tests.sh` (the repo's
  `run-integration-tests.js` is deprecated); non-root nginx listens on **:8080**, health on
  `= /` for Polaris.

### 7.7 Coverage matrix — the golden master must hit every path

One row per feature; the suite is "done" only when every §4 location has an assertion.
Body-fixture column flags where the mock must return a representative body (the rest are
redirect / header-echo assertions that need no fixture).

| Feat | Paths (from §4) | Assert | Body fixture |
|---|---|---|---|
| 0 💓 | `= /`, `/robots933456.txt` | 200 + "…online" body / probe answers | — |
| 1 🚀 | `/launch/cms`, `/launch/cin2–5`, `/launch/*-proxy` (10) | 302 + `Location` chain (OutSystems/CMS; uses `DEFAULT_…DOMAIN`) | — |
| 2 🤝 | `/auth-refresh-outbound`, `/polaris`, `/init`, `/auth-refresh-inbound`, `/auth-refresh-termination`, `/auth-refresh-cms-modern-token`, `/dev-login/`, `/api/dev-login-full-cookie/` | 302/403, `Cms-Session-Hint` cookie attrs, whitelist, `cc=` param, DDEI proxied | DDEI echo |
| 3 🧭 | `= /api/navigate-cms`, `= /navigate-cms-close` | IE-mode iframe HTML, `X-InternetExplorerMode`, `getDomainFromCookie` | — (nginx-built) |
| 4 📁 | `/${APP_SUBFOLDER_PATH}`, `/materials`, `= /materials-ui`, materials deep-link, `/materials-ui/`, `/api/`, `/sas-url/`, `/v2/` | proxied to right upstream (echo `Host`), `/materials` 302 handover, `sub_filter` API-domain→`$host` & SAS→`/sas-url/` | body w/ API+SAS domains (synth) |
| 5 🔀 | CMS regexes (`uainGeneratedScript`, `uainMenuBar`, `uacdCDTabs`, `CMSModern/Files`, `^/CMS.*`), `/cin2–5` switch, root `/`, `/ajax/viewer/` | env pick via `__CMSENV`/LB cookie, `replaceCmsDomains`→`$host`, **button/script injection**, cookie set + LB-cookie clear | **CMS bodies (bespoke)** |
| 6 🔌 | `/internal-implementation/{corsham,farnborough}[/modern]/` (4) | internal-only proxy to CMS classic/modern per DC, IP pick | CMS echo |
| X 🗑️ | `/sas-url/` (+ §4 drop-candidates) | characterise current behaviour **before** deletion | as feat 4 |
| — | cross-cutting | security headers (HSTS, `X-Frame-Options`, `Cache-Control: no-store`), IE/Edge `$ieaction`, scheme rewrites | per path |

`global-components*.conf` locations are covered by the parallel repo's own harness; our
suite only smoke-tests that the `include` loads.

_(This harness is the precondition for confidently doing §6. Suggested order overall:
build §7 golden-master → §6.5 md5 → §6.6 scaffold → move features one at a time, each
gated on a green suite.)_
