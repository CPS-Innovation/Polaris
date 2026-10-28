# PROXY.md — The Polaris CMS Proxy

> Working notes for a refactor of the nginx CMS proxy, and the **first thing to
> re-read when returning to this work**. Opens with repo-wide navigation, then goes
> deep on **which Terraformed AppSettings drive the proxy** and how the proxy is put
> together. Navigation-first up top; exhaustive below.
>
> Companion docs: [`../config/QUIRKS.md`](../config/QUIRKS.md) — every bug/oddity
> found while building the test harness (cross-cutting quirks + the index; feature
> quirks live in each `../config/features/<name>/QUIRKS.md`);
> [`PLAN.md`](./PLAN.md) — the phased roadmap.
> The harness itself lives in [`../tests`](../tests) (see [`../README.md`](../README.md)).

## Codebase navigation

> Repo-wide orientation (folded in from the former `SYSTEM.md`). Navigation-first,
> not exhaustive — the proxy deep-dive starts at §1 below.

### Repo layout (top level)

| Path                                | What it is                                                                                                                                                                                                                        |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `polaris-terraform/main-terraform/` | **All infrastructure** — one `.tf` file per deployed component, the tfvars, and the proxy's nginx config files (`nginx.conf`, `nginx.js`, `cmsenv.js`, `global-components.*`, `polaris-script.js`). Where the proxy work happens. |
| `polaris-ui/`                       | The **React SPA** (`polaris-ui`). Config via `.env.<env>` + `src/app/config.ts`.                                                                                                                                                  |
| `polaris-devops-pipelines/`         | Azure DevOps **build + deploy** pipelines.                                                                                                                                                                                        |
| Backend function-app source         | The C#/.NET function apps (gateway, coordinator, pdf-\*, text-extractor).                                                                                                                                                         |

### Deployed components

Each is defined in `polaris-terraform/main-terraform/` and (except maintenance) has an
identical `…-staging.tf` twin for the deployment slot.

| Component                   | Terraform file                        | Purpose                                             |
| --------------------------- | ------------------------------------- | --------------------------------------------------- |
| **CMS proxy**               | `app-service-proxy.tf`                | nginx reverse proxy — the subject of this doc (§1+). |
| **SPA**                     | `app-service-spa.tf`                  | Serves `polaris-ui` (Node 20, `npx serve`).         |
| **Gateway**                 | `function-gateway.tf`                 | Polaris API (`fa-polaris…`).                        |
| **Coordinator**             | `function-coordinator.tf`             | Durable-functions pipeline orchestration.           |
| **PDF generator**           | `function-pdf-generator.tf`           | Renders documents to PDF.                           |
| **PDF redactor**            | `function-pdf-redactor.tf`            | Applies redactions.                                 |
| **PDF thumbnail generator** | `function-pdf-thumbnail-generator.tf` | Page thumbnails.                                    |
| **Text extractor**          | `function-text-extractor.tf`          | OCR + search indexing.                              |
| **Maintenance**             | `function-maintenance.tf`             | Utility / housekeeping.                             |

External apps the proxy integrates with but that are **not** defined here (referenced via
`local.*`/data sources): **DDEI** (`fa-…-ddei`) and **WM-MDS** (managed data store).

### Where config lives

- **Backend / proxy:** `prod.tfvars` / `uat.tfvars` / `qa.tfvars` supply `var.*`, consumed
  as `app_settings` in the `app-service-*.tf` / `function-*.tf` files. The proxy's config
  files are uploaded as storage blobs by `app-service-proxy.tf` and rendered by `envsubst`
  at container start.
- **UI:** `polaris-ui/.env.<env>` (`.env.production`, `.env.uat`, `.env.qa`,
  `.env.development`) → read in `polaris-ui/src/app/config.ts` as `process.env.REACT_APP_*`.
  Build: `polaris-ui/scripts/build.sh` (`env-cmd -f .env.<env> react-scripts build`), driven
  by `polaris-devops-pipelines/deployments_v2/stages/stage_publish-all-artifacts.yml`.
  A runtime substitutor, `polaris-ui/public/subsititute-config.js`, also exists (see the
  **Config source-of-truth** appendix).

### Reorientation quick-start

Re-read these first when returning to the proxy work:

1. This doc — the navigation above, then the proxy deep-dive below.
2. `polaris-terraform/main-terraform/app-service-proxy.tf` (settings + `lifecycle` block).
3. `polaris-terraform/main-terraform/nginx.conf` and `nginx.js`.
4. `polaris-terraform/main-terraform/prod.tfvars` (the values behind the settings).

Handy checks:

- Proxy setting keys: `grep -oE '"[A-Z_]+"' app-service-proxy.tf` (remember most appear
  twice — in `app_settings` and in `lifecycle.ignore_changes`; the 9 drop1/drop2 settings
  are deliberately only in `app_settings`, see §5).
- Who else uses a var: `grep -rn "var.<name>" polaris-terraform/main-terraform/`.

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
| `CPT_UPSTREAM_CMS_IP_CORSHAM`                     | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CPT_UPSTREAM_CMS_MODERN_IP_CORSHAM`              | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CPT_UPSTREAM_CMS_IP_FARNBOROUGH`                 | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CPT_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH`          | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CPT_UPSTREAM_CMS_DOMAIN_NAME`                    | var.cms_details             | 🟢 **Proxy-only**      | 1 🚀, 2 🤝, 5 🔀, 6 🔌                       |
| `CPT_UPSTREAM_CMS_SERVICES_DOMAIN_NAME`           | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
| `CPT_UPSTREAM_CMS_MODERN_DOMAIN_NAME`             | var.cms_details             | 🟢 **Proxy-only**      | 5 🔀, 6 🔌                                   |
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
| `NON_DDEI_INIT_ENABLED`                           | var.non_ddei_init_enabled   | 🟢 **Proxy-only**      | 2 🤝 — drop1 switch: all auth handover off DDEI (`process.env`) |
| `ENTRA_STORE_ENABLED`                             | var.proxy_entra             | 🟢 **Proxy-only**      | 2 🤝 — drop2 switch (`process.env`)          |
| `ENTRA_INTERACTIVE_ENABLED`                       | var.proxy_entra             | 🟢 **Proxy-only**      | 2 🤝 — drop2 interactive retry               |
| `ENTRA_TENANT_ID`                                 | client config               | 🟢 **Proxy-only**      | 2 🤝 — drop2                                 |
| `ENTRA_CLIENT_ID`                                 | var.proxy_entra             | 🟢 **Proxy-only**      | 2 🤝 — drop2                                 |
| `ENTRA_CLIENT_SECRET`                             | kv secret (drop2 envs)      | 🟢 **Proxy-only**      | 2 🤝 — drop2                                 |
| `ENTRA_WATCHDOG_SCOPE`                            | var.proxy_entra             | 🟢 **Proxy-only**      | 2 🤝 — drop2                                 |
| `ENTRA_MDS_SCOPE`                                 | var.proxy_entra             | 🟢 **Proxy-only**      | 2 🤝 — drop2 (MDS deposit)                   |
| `ENTRA_STATE_HMAC_SECRET`                         | random_password             | 🟢 **Proxy-only**      | 2 🤝 — drop2 (cookie signing)                |
| `WM_MDS_BASE_URL`                                 | wm-mds                      | 🟢 **Proxy-only**      | global-components, 2 🤝 (drop2 store)        |
| `WM_MDS_ACCESS_KEY`                               | kv secret                   | 🟢 **Proxy-only**      | global-components, 2 🤝 (drop2 store)        |
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

**Tally (87 keys total):** Proxy-only = 60 (35 the CMS-upstream family — DEFAULT, CPT,
CIN2, CIN4, CIN5 × 7; 9 the drop1/drop2 switches + `ENTRA_*`), Shared with app = 2,
Fleet-platform = 25.

### How each value actually reaches nginx (3 injection paths — easy to trip on)

1. **`envsubst` `${VAR}`** — the default; most keys are substituted into `.conf`
   templates at container start.
2. **`js_var` bridge** (live only) — the 35 CMS-upstream vars are declared `js_var $...`
   in the live `nginx.conf` so the live `cmsenv.js` can read them as `r.variables[...]`.
   The next config has **no** such bridge.
3. **`process.env`** — live: only two keys, only in `nginx.js` (`AUTH_HANDOVER_WHITELIST`
   and `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME`). Next: the main njs path — the CMS-upstream
   settings via `common/cmsenv.js` `setting` (`<ENV>_UPSTREAM_CMS_*`), plus
   `ENDPOINT_HTTP_PROTOCOL`, `WEBSITE_SCHEME`, `WEBSITE_HOSTNAME`, the two above, the
   switches (`NON_DDEI_INIT_ENABLED`, `ENTRA_*`) and drop2's `WM_MDS_*`. Read at request
   time, so a missing setting reads as empty/off instead of breaking boot.

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
  - `/launch/{cms,cmo,cpt,cin2..cin5}` (+ `-proxy` variants, 14 in all) → static 302s into
    OutSystems/CMS chains.
- **CMS proxying (njs `cmsenv.js` chooses upstream by `__CMSENV`/LB cookie):**
  Classic (`~ ^/CMS.*`), Modern (`~ ^/CMSModern/Files.*`, `/`), script/button injection
  (`uainGeneratedScript.aspx`, `uainMenuBar.js`, `uacdCDTabs.aspx`), env switch
  (`/cpt`,`/cin2`,`/cin3`,`/cin4`,`/cin5` set `__CMSENV` + clear LB cookies), dev-login
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

Every `location` in the **live** `nginx.conf`, in file order, with the feature id it was
sliced into (§6.4). The line numbers are into the live monolith (the golden master);
`(?)` marks a couple where the original intent was uncertain. Scoped to `nginx.conf` only;
the `include global-components*.conf` at line 996 adds more locations
(`global-components.conf`), covered on its own track.

| Line | `location`                                      | Feature (draft — edit me)                        | Feature id                 | Notes                                                                             |
| ---: | ----------------------------------------------- | ------------------------------------------------ | -------------------------- | --------------------------------------------------------------------------------- |
|  129 | `= /`                                           | Health check (readiness "online")                | 0 💓 - health check        |                                                                                   |
|  134 | `= /api/navigate-cms`                           | CMS navigation (IE-mode iframe)                  | 3 🧭 - rCMS feature        | Candidate to move to global components.                                           |
|  150 | `= /navigate-cms-close`                         | CMS navigation (close helper)                    | 3 🧭 - rCMS feature        | Candidate to move to global components. Why not /api/navigate-cms-close?          |
|  159 | `= /robots933456.txt`                           | Azure platform probe                             | 0 💓 - health check        | Could be dropped, might be to do with SSL provisioning?                           |
|  164 | `/polaris-script.js`                            | Serve injected client script                     | 5 🔀 - CMS proxy           | Inject buttons                                                                    |
|  171 | `~ ^/CMS\..*/Includes/uainGeneratedScript.aspx` | CMS button injection (casework-tools URL)        | 5 🔀 - CMS proxy           |                                                                                   |
|  212 | `~ ^/CMS\..*/Noexpiry/Toolbar/uainMenuBar.js`   | CMS button injection (Polaris/Materials buttons) | 5 🔀 - CMS proxy           | Inline js could be put in njs file                                                |
|  264 | `~ ^/CMS\..*/Case/uacdCDTabs.aspx`              | CMS script injection (polaris-script.js)         | 5 🔀 - CMS proxy           |                                                                                   |
|  294 | `~ ^/CMSModern/Files.*`                         | CMS Modern proxy (Files / Services)              | 5 🔀 - CMS proxy           |                                                                                   |
|  339 | `~ ^/CMS.*`                                     | CMS Classic proxy (general fallback)             | 5 🔀 - CMS proxy           |                                                                                   |
|  385 | `/cpt`                                          | CMS env switch (cpt)                             | 5 🔀 - CMS proxy           |                                                                                   |
|  421 | `/cin3`                                         | CMS env switch (cin3)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  455 | `/cin2`                                         | CMS env switch (cin2)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  489 | `/cin4`                                         | CMS env switch (cin4)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  523 | `/cin5`                                         | CMS env switch (cin5)                            | 5 🔀 - CMS proxy           |                                                                                   |
|  557 | `/`                                             | CMS Modern proxy (root)                          | 5 🔀 - CMS proxy           | Has always been a bit dodgy just being on the root                                |
|  593 | `/ajax/viewer/`                                 | CMS Modern proxy (ajax viewer)                   | 5 🔀 - CMS proxy           |                                                                                   |
|  633 | `/auth-refresh-outbound`                        | Auth handover (outbound → CMS `/polaris`)        | 2 🤝 - auth handover       |                                                                                   |
|  641 | `/polaris`                                      | Auth handover (simulated CMS `/polaris`)         | 5 🔀 - CMS proxy           | ... and auth 2 🤝 - auth handover                                                 |
|  653 | `/launch/cms`                                   | Launch redirect (CMS)                            | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  656 | `/launch/cmo`                                   | Launch redirect (cmo)                            | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  659 | `/launch/cpt`                                   | Launch redirect (cpt)                            | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  662 | `/launch/cin2`                                  | Launch redirect (cin2)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  665 | `/launch/cin3`                                  | Launch redirect (cin3)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  668 | `/launch/cin4`                                  | Launch redirect (cin4)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  671 | `/launch/cin5`                                  | Launch redirect (cin5)                           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  674 | `/launch/cms-proxy`                             | Launch redirect (CMS, proxied session)           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  677 | `/launch/cmo-proxy`                             | Launch redirect (cmo, proxied session)           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  680 | `/launch/cpt-proxy`                             | Launch redirect (cpt, proxied session)           | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  683 | `/launch/cin2-proxy`                            | Launch redirect (cin2, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  686 | `/launch/cin3-proxy`                            | Launch redirect (cin3, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  689 | `/launch/cin4-proxy`                            | Launch redirect (cin4, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  692 | `/launch/cin5-proxy`                            | Launch redirect (cin5, proxied session)          | 1 🚀 - app launch from CMS | These can be collapsed into one (maybe needs njs)                                 |
|  697 | `/init`                                         | Auth handover (whitelist + `Cms-Session-Hint`)   | 2 🤝 - auth handover       |                                                                                   |
|  711 | `/auth-refresh-inbound`                         | Auth handover (inbound → DDEI)                   | 2 🤝 - auth handover       |                                                                                   |
|  715 | `/auth-refresh-termination`                     | Auth handover (in-situ termination → DDEI)       | 2 🤝 - auth handover       |                                                                                   |
|  723 | `/auth-refresh-cms-modern-token`                | Auth handover (CMS Modern token → DDEI)          | 2 🤝 - auth handover       | Could be refactored away                                                          |
|  728 | `/${APP_SUBFOLDER_PATH}`                        | Polaris SPA proxy                                | 4 📁 - CWA/materials       | reads polaris-ui                                                                  |
|  736 | `/materials`                                    | Materials UI (handover redirect)                 | 4 📁 - CWA/materials       |                                                                                   |
|  746 | `= /materials-ui`                               | Materials UI (redirect → `/materials-ui/`)       | 4 📁 - CWA/materials       |                                                                                   |
|  750 | `~ ^/materials-ui/[^/]+/[^/]+/materials$`       | Materials UI proxy (deep-link) `(?)`             | 4 📁 - CWA/materials       |                                                                                   |
|  758 | `/materials-ui/`                                | Materials UI proxy                               | 4 📁 - CWA/materials       |                                                                                   |
|  767 | `/api/`                                         | Gateway API proxy                                | 4 📁 - CWA/materials       |                                                                                   |
|  784 | `/sas-url/`                                     | Blob storage SAS proxy                           | X 🗑️ - redundant           | Can be deleted                                                                    |
|  790 | `/dev-login/`                                   | Dev login via DDEI (non-prod)                    | 2 🤝 - auth handover       | Allow devs to log into CMS without hitting proxy UI, is also 5 🔀 - CMS proxy     |
|  821 | `/api/dev-login-full-cookie/`                   | Dev login, full cookie (non-prod)                | 2 🤝 - auth handover       | Allow devs to log into CMS without hitting proxy UI, is also 4 📁 - CWA/materials |
|  852 | `/internal-implementation/corsham/`             | Internal CMS route (Corsham, Classic)            | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  887 | `/internal-implementation/corsham/modern/`      | Internal CMS route (Corsham, Modern)             | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  921 | `/internal-implementation/farnborough/`         | Internal CMS route (Farnborough, Classic)        | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  956 | `/internal-implementation/farnborough/modern/`  | Internal CMS route (Farnborough, Modern)         | 6 🔌 - polaris ddei        | Can be deleted once auth handover code is refactored                              |
|  989 | `/v2/`                                          | Telemetry (App Insights ingestion)               | 4 📁 - CWA/materials       |                                                                                   |

## 5. Learnings / gotchas

- **Three injection paths** (§2) — always confirm _how_ a var is consumed before moving it.
- **"DEFAULT" means CIN3.** The `DEFAULT_*` CMS upstream set is the CIN3/production CMS.
- **`FORCE_REFRESH_CONFIG`** is an md5 of `nginx.conf`, `nginx.js`, `cmsenv.js`,
  `polaris-script.js`, `global-components.conf`, `global-components.js`, plus
  `local.proxy_next_config_hash` (the whole next tree + `start-next.sh`). Editing any of
  them changes this setting and forces the app to pick up new config on deploy.
- **The prod slot's `app_settings` are mirrored in `lifecycle.ignore_changes` — except the
  9 drop1/drop2 settings** (`NON_DDEI_INIT_ENABLED` + 8 `ENTRA_*`), deliberately left
  out so terraform manages them. The staging slot (`app-service-proxy-staging.tf`) has
  **no** `ignore_changes`. Add/rename a setting → `app_settings` in **both** files, plus
  the prod `ignore_changes` unless terraform should own its value.
- **`-staging` twin.** `app-service-proxy-staging.tf` mirrors this file for the slot.
- **Reauth is split-brain (reference).** The UI holds the redirect _strings_
  (`REACT_APP_REAUTH_*`, case-review, bulk-um) while the proxy holds
  `AUTH_HANDOVER_WHITELIST` and _implements_ the `/auth-refresh-*`, `/polaris`, `/init`
  endpoints. Note `AUTH_HANDOVER_WHITELIST` itself is **Proxy-only** (nothing else reads
  it); the "split" is about the auth-handover _behaviour_ living in two components at
  once, not a shared setting — see the **Config source-of-truth** appendix below.

## 6. Target architecture — feature-sliced config (refactor direction)

**Goal.** One nginx sub-config `include` per feature id (§4), and one njs module per
feature **where a feature needs request-time JS** — driven off `include`s the way
`global-components.conf` already is. Avoid a catch-all script, with **one deliberate
exception** (the shared CMS-env module, below) that the code genuinely forces.

> **Status (mostly landed).** The slice and its terraform plumbing are done; what remains
> is the `replaceCmsDomains` decision and finishing the cutover.
>
> | Step | State |
> | --- | --- |
> | §6.5 terraform genericisation (blob/`md5`) | ✅ done (`proxy_next_*` in `app-service-proxy.tf`) |
> | §6.6 feature slicing (locations → `features/`) | ✅ done — `--next` green |
> | njs split into per-feature modules | 🟡 mostly done — modules + shared `common/cmsenv.js`; remaining: the `replaceCmsDomains` decision (QUIRKS B1) |
> | Cutover (boot the next config) | 🟡 terraform-controlled (#2190): `proxy_next_config_enabled` sets the Startup Command `sh /etc/nginx/templates/start-next.sh` on both slots (true in dev/QA/UAT/prod); rollback = `false` + apply. Done when applied everywhere, then delete the live monolith |
>
> Note the built layout differs from the original sketch below in two ways, kept here for
> the reasoning: features are **folders** (`features/<name>/<name>.{conf,js}`), not flat
> numbered files; and the shared module is **`common/cmsenv.js`** (there is no file
> called `common.js`; a separate `cms-detection.js` was tried and folded back in), with `getDomainFromCookie` living in `global-components.js` rather
> than a separate `navigate-cms.js`.

### 6.1 Where today's njs files landed

| Live file                           | Concern                                                                                             | Landed at (in `config/features/`)                                                    |
| ----------------------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `nginx.js`                          | auth handover (feat **2**) — already single-concern                                                 | `auth-handover/auth-handover.js`                                                     |
| `cmsenv.js` → `getDomainFromCookie` | navigate-cms (feat **3**), self-contained (own regex, no shared core)                               | `global-components/global-components.js` (the `/api/navigate-cms` route's home)      |
| `cmsenv.js` → CMS-env resolution    | resolution + upstream rewrite, shared by feat **5**, **6**, and the dev-login part of **2**         | **`common/cmsenv.js`** (the one shared module — see §6.2)                            |
| `cmsenv.js` → internal getters      | the `*Internal` upstream getters for the DC-internal routes                                          | `polaris-ddei/polaris-ddei.js` (self-contained; own getter copies — see QUIRKS D7)  |
| `polaris-script.js`                 | client-side button script, served/injected under feat **5** (buttons drive **1** & **4**)           | `cms-proxy/polaris-script.js` — static asset beside its feature (browser JS, not njs) |

There is also an `app-launch/app-launch.js` (feat **1**) that emerged during the slice.

### 6.2 The one shared module — `common/cmsenv.js`

Every CMS-proxy / internal / dev-login path funnels through the CMS-env detection
(cookie → `default/cpt/cin2/cin4/cin5`) and the `<ENV>_UPSTREAM_CMS_*` setting-name
convention (the live monolith reads them as `r.variables[cmsEnv + 'UpstreamCms…']` via its
`js_var` bridge; the next config reads `process.env` through `cmsenv.setting`). Splitting that detection per-feature would force copies of the cin
logic (drift risk — recall cin5 was added later), so it stays one module:
**`common/cmsenv.js`**, single-responsibility ("resolve the CMS upstream for the current
environment"), _not_ a utils junk-drawer. It holds the two primitives (`detect`, `setting`)
our feature modules `import`, and the five getters that are the global-components contract
(§6.8). The primitives briefly lived in their own `cms-detection.js` to keep that contract
surface minimal; folded back (2026-10-20) because nothing outside this config imported it,
and extra exports on a `js_import`ed module bind nothing (only `js_set` names do).

Two deliberate departures from the original "one shared module holds everything" sketch,
both landed and worth knowing:

- **`replaceCmsDomains*` / `__replaceContent` live in `cms-proxy/cms-proxy.js`**, not the
  shared module — cms-proxy is their only caller (and the known-bug decision, QUIRKS **B1**,
  is scoped there).
- **`polaris-ddei` keeps its own copies of the upstream getters** rather than importing the
  shared ones, so the feature stays independently deletable (QUIRKS **D7**); the small
  duplication is intentional.

> Later optimisation to shrink even this: move the cookie→env detection into an nginx
> `map $http_cookie $cms_env { ~*cin2 cin2; ~*cin4 cin4; ~*cin5 cin5; default default; }`
> in the parent, so per-feature njs just reads `r.variables.cms_env` and
> `common/cmsenv.js` loses its detection half. Optional; not required to start.

### 6.3 What always stays in the parent `nginx.conf` (never per-feature)

The server-scope preamble: the parent-level `js_import`s (`ieMode`, and `cmsenv` under its
contract name — feature modules are imported by their own confs), `js_set $ieaction`,
`limit_req_zone` ×2, `resolver` (or the future `map`),
security-header defaults, and the `include` list. Plus the **ordering contract**: all CMS
regexes (the cms-proxy injectors + the `^/CMS.*` catch-all) live together in the cms-proxy
include, so include order stays free (see §5 / precedence notes).

### 6.4 As-built layout

Features are **folders**, each self-contained (conf + njs + its tests + its `QUIRKS.md`),
under `config/` — the parent `nginx.conf` is preamble + `include features/*/*.conf;`:

```
config/
  nginx.conf                       # parent: preamble + ordered includes only
  QUIRKS.md                        # cross-cutting quirks + the index
  features/
    app-launch/        app-launch.conf   + app-launch.js
    auth-handover/     auth-handover.conf + auth-handover.js        (was nginx.js)
    auth-handover.drop1.replace-ddei/
                       …drop1.replace-ddei.conf + .js               (NON_DDEI_INIT_ENABLED: the $non_ddei gate;
                                                                     njs /api/init/, termination, cms-modern-token)
    auth-handover.drop2.entra/
                       …drop2.entra.conf + .js + store.js            (ENTRA_STORE_ENABLED; Entra + MDS deposit)
    cms-proxy/         cms-proxy.conf     + cms-proxy.js + polaris-script.js
                       (holds ALL CMS regexes — the ordering contract; + fixtures/)
    common/            cmsenv.js + ie-mode.js                        (SHARED: CMS env + contract; IE mode)
    cwa-materials/     cwa-materials.conf
    dev-login/         dev-login.conf     + dev-login.js             (DDEI's dev login in njs, behind $non_ddei)
    global-components/ global-components.conf + global-components.js (getDomainFromCookie)
    polaris-ddei/      polaris-ddei.conf  + polaris-ddei.js          (own getter copies)
    # each folder also carries <name>.unit.test.js / <name>.integration.test.js / QUIRKS.md
```

`global-components.conf` remains on its own track (existing, separate). Cross-feature
references are fine and expected: the `/polaris` location (cms-proxy conf) calls
`auth-handover.js`; features that need CMS-env resolution import `common/cmsenv.js`.
Redundant routes (e.g. `/sas-url/`, §4) were **pruned**, not sliced — there is no
`redundant` feature.

### 6.5 Terraform mechanics — genericised blob/`md5` wiring ✅ done

**Landed.** `app-service-proxy.tf` now uploads the whole `config/` tree via a `fileset`
(`local.proxy_next_*`): `.conf` → `*.conf.template`, `.js` as-is, `*.test.js` / `fixtures/`
excluded (guarded by `tests/unit/deploy-safety.unit.test.js`), and a single
`local.proxy_next_config_hash` appended to `FORCE_REFRESH_CONFIG`. The refactored root
ships as `nginx-next.conf.template` beside the `start-next.sh` blob; it is booted when
`var.proxy_next_config_enabled` sets the Startup Command to run `start-next.sh` (otherwise
the image default boots the live monolith). The original problem and the approach it
took, kept for reference:

Every config file used to be wired **three times** by hand, per file:

1. an explicit `azurerm_storage_blob` (uploaded to the container mounted at
   `/etc/nginx/templates`; `.conf` → `*.conf.template` for `envsubst`, `.js` → plain and
   `js_import`ed as `templates/<x>.js`);
2. an entry in the hand-built `FORCE_REFRESH_CONFIG` md5 string
   (`app-service-proxy.tf:78` — note the stray `::` between `cmsenv.js` and
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

### 6.6 Feature slicing ✅ done — (was: empty-file scaffold to prove the loop)

**Landed.** Every `location` has been sliced into `features/<name>/<name>.conf` and
`./run-tests.sh --next` is green. The zero-risk scaffold approach that got us there, kept
for reference:

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

- **Regex ordering:** all CMS regexes stay in `cms-proxy/cms-proxy.conf`, current relative
  order.
- **No duplicate literal locations** across includes (nginx won't boot).
- **Server-scope directives** (`js_import` / `js_var` / `map` / `limit_req_zone`) stay in
  the parent — feature files contain `location {}` blocks only.
- **`include` order** only matters for the CMS-regex file; the wildcard
  `include features/*/*.conf` is safe because the colliding regexes are all co-located in
  the one cms-proxy conf.

_(§6.5 and §6.6 are done. Remaining: the `replaceCmsDomains` decision, QUIRKS **B1**, and
finishing the cutover — terraform-controlled via `proxy_next_config_enabled` (#2190): apply
it everywhere, then delete the live copies.)_

### 6.8 The global-components contract — what this config promises foreign configs

The **global-components repo** deploys its own nginx configs into this proxy (vnext,
case-locking, cms-auth-presence, …) and is their **source of truth** —
they are deployed from there, ad hoc, not from here. Its `infra/proxy/config/main/` is
a stand-in for *this* base config, so its configs can only rely on what the base
provides. Both worlds (live monolith and this refactor) must offer the same surface:

| The base provides                                                                     | Live monolith                        | Next config (`config/`)                                    |
| ------------------------------------------------------------------------------------- | ------------------------------------ | ---------------------------------------------------------- |
| a wildcard include of their `.conf.template` blobs ¹                                  | root: `include global-components*.conf;` | `features/global-components.<x>/global-components.<x>.conf`, loaded by the existing `include features/*/*.conf;` |
| their `global-components.<x>.js` (their confs `js_import templates/global-components.<x>.js`) | root blob                            | the **same** root blob (shared by both worlds)             |
| njs module bound as **`cmsenv`** exporting `proxyDestination[Modern]Corsham`, `upstreamCms{,Modern,Services}DomainName` | `js_import templates/cmsenv.js`      | `js_import cmsenv from …/features/common/cmsenv.js`        |
| `$proxyDestination[Modern]`, `$upstreamCms*DomainName` bound to **exactly** `cmsenv.<fn>` ² | ✅                                   | ✅ (cms-proxy.conf binds to `cmsenv.*`)                    |
| `$cors_origin` (js_set), `@gloco_preflight`                                            | `global-components.conf`             | `features/global-components/`                              |
| `log_format cms_log`, `limit_req_zone cmsproxy`, `$ieaction`                           | ✅                                   | ✅ (`nginx.conf`)                                          |

¹ So global-components' deploy writes each `.conf` TWICE (root for the live world, a
`features/global-components.<x>/` folder for the next one) and each `.js` once (root). The
conf text is identical in both places. The next config deliberately has NO root
`global-components*.conf` include: both worlds share one blob container while dual-running,
and the live world's own `global-components.conf` sits at that root — it would load on top
of `features/global-components/` (duplicate locations, no boot). The `features/` folder
must be exactly one level deep (`include features/*/*.conf`) and must not reuse one of our
feature names.
² A `js_set` variable is server-global; nginx refuses to boot if two js_sets bind one
variable to different `module.function` strings. So these names are frozen: don't rename
`cmsenv` or its exports, or move the getters back into a feature (unit-pinned by
`common/cmsenv.unit.test.js`).

**Location namespaces:** their routes must not duplicate ours. cms-auth-presence's flow is
`/polaris-presence` + `/init-presence/*`; our drop2's only routes are `/init/entra-callback`, `/init/terminal` and `/global-components/entra-login-hint` (it runs from `/init` itself). polaris-non-ddei's
`/init-non-ddei` DOES duplicate drop1 — never deploy its `.conf` alongside the next config.

## 7. Testing the proxy — integration + unit (the refactor safety net)

**Built and green.** The golden-master harness described here as a plan now exists and
runs against both the live and `next` configs. Full mechanics — layout, how to run it,
live-vs-next, conventions — live in **[`../README.md`](../README.md)**; this section keeps
only the _why_ and the coverage matrix that the suite is measured against.

**Why.** The whole §6 refactor (slice config into feature includes, consolidate the shared
CMS-env logic) is only safe if we can prove behaviour is unchanged after each move. So we
feed in every relevant app setting ourselves, boot the existing config against a mock
upstream, and manipulate the env / cookies to exercise every path — first to characterise
today's behaviour, then to guarantee parity through the slice and the later consolidation.
The **§2 grid is the env manifest** (`tests/integration/docker/cmsproxy.mock.env`); the
mock echoes each request back as JSON so tests assert what nginx forwarded.

**Golden-master ordering.** The suite was written against the **current** config first, so
it encodes today's behaviour; every §6 step is validated by re-running the same green suite,
and behaviour drift shows up as a red test pinned to the step that caused it. `QUIRK:` tests
pin the known-wrong behaviours (see [`../config/QUIRKS.md`](../config/QUIRKS.md)) so a
refactor cannot change them silently.

### 7.1 Coverage matrix — the golden master must hit every path

One row per feature; the suite is "done" only when every §4 location has an assertion.
Body-fixture column flags where the mock must return a representative body (the rest are
redirect / header-echo assertions that need no fixture).

| Feat | Paths (from §4)                                                                                                                                                                     | Assert                                                                                                                    | Body fixture                    |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | ------------------------------- |
| 0 💓 | `= /`, `/robots933456.txt`                                                                                                                                                          | 200 + "…online" body / probe answers                                                                                      | —                               |
| 1 🚀 | `/launch/cms`, `/launch/cmo`, `/launch/cpt`, `/launch/cin2–5`, `/launch/*-proxy` (14)                                                                                                | 302 + `Location` chain (OutSystems/CMS; uses `DEFAULT_…DOMAIN`)                                                           | —                               |
| 2 🤝 | `/auth-refresh-outbound`, `/polaris`, `/init`, `/auth-refresh-inbound`, `/auth-refresh-termination`, `/auth-refresh-cms-modern-token`, `/dev-login/`, `/api/dev-login-full-cookie/` | 302/403, `Cms-Session-Hint` cookie attrs, whitelist, `cc=` param, DDEI proxied                                            | DDEI echo                       |
| 3 🧭 | `= /api/navigate-cms`, `= /navigate-cms-close`                                                                                                                                      | IE-mode iframe HTML, `X-InternetExplorerMode`, `getDomainFromCookie`                                                      | — (nginx-built)                 |
| 4 📁 | `/${APP_SUBFOLDER_PATH}`, `/materials`, `= /materials-ui`, materials deep-link, `/materials-ui/`, `/api/`, `/sas-url/`, `/v2/`                                                      | proxied to right upstream (echo `Host`), `/materials` 302 handover, `sub_filter` API-domain→`$host` & SAS→`/sas-url/`     | body w/ API+SAS domains (synth) |
| 5 🔀 | CMS regexes (`uainGeneratedScript`, `uainMenuBar`, `uacdCDTabs`, `CMSModern/Files`, `^/CMS.*`), `/cpt` + `/cin2–5` switch, root `/`, `/ajax/viewer/`                                | env pick via `__CMSENV`/LB cookie, `replaceCmsDomains`→`$host`, **button/script injection**, cookie set + LB-cookie clear | **CMS bodies (bespoke)**        |
| 6 🔌 | `/internal-implementation/{corsham,farnborough}[/modern]/` (4)                                                                                                                      | internal-only proxy to CMS classic/modern per DC, IP pick                                                                 | CMS echo                        |
| X 🗑️ | `/sas-url/` (+ §4 drop-candidates)                                                                                                                                                  | characterise current behaviour **before** deletion                                                                        | as feat 4                       |
| —    | cross-cutting                                                                                                                                                                       | security headers (HSTS, `X-Frame-Options`, `Cache-Control: no-store`), IE/Edge `$ieaction`, scheme rewrites               | per path                        |

Configs deployed by the global-components repo are not tested here: they are that repo's,
deployed at its own risk and tested there.

_(This harness was the precondition for §6, and both its groundwork steps are now done:
the golden master is green, §6.5 (terraform genericisation) and §6.6 (feature slicing) have
landed. What remains is the `replaceCmsDomains` decision and finishing the
terraform-controlled cutover — see §6.)_

## Appendix — Config source-of-truth (reference note)

> Recorded to explain the 🟡 **Shared with app** scope in §2 and the _implied
> dual/multiple use of app settings_ across components. We are **not** changing the UI.

- The same logical settings (feature flags, reauth URLs, redirect URLs, global-components
  URL, private-beta groups, pipeline-refresh interval, local-storage expiry, redaction-offline
  flag) are defined in **both** the tfvars **and** the UI `.env.<env>` files — two
  unsynchronised copies with no shared source.
- **Which copy wins:** Create-React-App inlines `REACT_APP_*` from `.env.<env>` at **build
  time**. `subsititute-config.js` only rewrites literal `--REACT_APP_X--` placeholder tokens
  at container start, and no `.env` file contains such tokens — so the SPA's Terraform
  `REACT_APP_*` `app_settings` (in `app-service-spa.tf`) are effectively **inert**; the
  `.env.<env>` values are what users get.
- **Genuinely shared Terraform vars** (single source, consumed by two components):
  `var.polaris_ui_sub_folder` (proxy `APP_SUBFOLDER_PATH` ↔ SPA OAuth `redirect_uris` ↔ UI
  `package.json` `PUBLIC_URL`) and `var.cps_global_components` (proxy `blob_storage_domain`
  ↔ SPA `script_url`). These are the two 🟡 rows in §2.
- **Observed drift:** `REACT_APP_REAUTH_REDIRECT_URL_OUTBOUND` differs by env — prod matches
  tfvars (`/auth-refresh-outbound,/polaris`), but `.env.qa` and `.env.uat` carry hard-coded
  `cinN.cps.gov.uk/polaris` domain lists with no tfvars counterpart.
- **Split-brain reauth:** the UI holds the redirect _strings_; the proxy holds
  `AUTH_HANDOVER_WHITELIST` and _implements_ the `/auth-refresh-*`, `/polaris`, `/init`
  endpoints (see the split-brain bullet in §5).
