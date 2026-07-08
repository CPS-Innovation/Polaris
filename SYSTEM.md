# SYSTEM.md — Codebase Navigation

> A reorientation aid for the Polaris repo, written to speed up returning to the
> proxy-refactor work. Pairs with [`PROXY.md`](./PROXY.md) (proxy deep-dive).
> Navigation-first; not exhaustive.

## 1. Repo layout (top level)

| Path | What it is |
|---|---|
| `polaris-terraform/main-terraform/` | **All infrastructure** — one `.tf` file per deployed component, the tfvars, and the proxy's nginx config files (`nginx.conf`, `nginx.js`, `cmsenv.js`, `global-components.*`, `polaris-script.js`). Where the proxy work happens. |
| `polaris-ui/` | The **React SPA** (`polaris-ui`). Config via `.env.<env>` + `src/app/config.ts`. |
| `polaris-devops-pipelines/` | Azure DevOps **build + deploy** pipelines. |
| Backend function-app source | The C#/.NET function apps (gateway, coordinator, pdf-*, text-extractor). |

## 2. Deployed components

Each is defined in `polaris-terraform/main-terraform/` and (except maintenance) has an
identical `…-staging.tf` twin for the deployment slot.

| Component | Terraform file | Purpose |
|---|---|---|
| **CMS proxy** | `app-service-proxy.tf` | nginx reverse proxy — see [`PROXY.md`](./PROXY.md). |
| **SPA** | `app-service-spa.tf` | Serves `polaris-ui` (Node 20, `npx serve`). |
| **Gateway** | `function-gateway.tf` | Polaris API (`fa-polaris…`). |
| **Coordinator** | `function-coordinator.tf` | Durable-functions pipeline orchestration. |
| **PDF generator** | `function-pdf-generator.tf` | Renders documents to PDF. |
| **PDF redactor** | `function-pdf-redactor.tf` | Applies redactions. |
| **PDF thumbnail generator** | `function-pdf-thumbnail-generator.tf` | Page thumbnails. |
| **Text extractor** | `function-text-extractor.tf` | OCR + search indexing. |
| **Maintenance** | `function-maintenance.tf` | Utility / housekeeping. |

External apps the proxy integrates with but that are **not** defined here (referenced via
`local.*`/data sources): **DDEI** (`fa-…-ddei`) and **WM-MDS** (managed data store).

## 3. Where config lives

- **Backend / proxy:** `prod.tfvars` / `uat.tfvars` / `qa.tfvars` supply `var.*`, consumed
  as `app_settings` in the `app-service-*.tf` / `function-*.tf` files. The proxy's config
  files are uploaded as storage blobs by `app-service-proxy.tf` and rendered by `envsubst`
  at container start.
- **UI:** `polaris-ui/.env.<env>` (`.env.production`, `.env.uat`, `.env.qa`,
  `.env.development`) → read in `polaris-ui/src/app/config.ts` as `process.env.REACT_APP_*`.
  Build: `polaris-ui/scripts/build.sh` (`env-cmd -f .env.<env> react-scripts build`), driven
  by `polaris-devops-pipelines/deployments_v2/stages/stage_publish-all-artifacts.yml`.
  A runtime substitutor, `polaris-ui/public/subsititute-config.js`, also exists (see §4).

## 4. Config source-of-truth — reference note (no action planned)

Recorded to explain the **"Joint" column in [`PROXY.md`](./PROXY.md)** and the *implied
dual/multiple use of app settings* across components. We are **not** changing the UI.

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
  ↔ SPA `script_url`).
- **Observed drift:** `REACT_APP_REAUTH_REDIRECT_URL_OUTBOUND` differs by env — prod matches
  tfvars (`/auth-refresh-outbound,/polaris`), but `.env.qa` and `.env.uat` carry hard-coded
  `cinN.cps.gov.uk/polaris` domain lists with no tfvars counterpart.
- **Split-brain reauth:** the UI holds the redirect *strings*; the proxy holds
  `AUTH_HANDOVER_WHITELIST` and *implements* the `/auth-refresh-*`, `/polaris`, `/init`
  endpoints.

## 5. Reorientation quick-start

Re-read these first when returning to the proxy work:
1. [`PROXY.md`](./PROXY.md) and this file.
2. `polaris-terraform/main-terraform/app-service-proxy.tf` (settings + `lifecycle` block).
3. `polaris-terraform/main-terraform/nginx.conf` and `nginx.js`.
4. `polaris-terraform/main-terraform/prod.tfvars` (the values behind the settings).

Handy checks:
- Proxy setting keys: `grep -oE '"[A-Z_]+"' app-service-proxy.tf` (remember they appear
  twice — in `app_settings` and in `lifecycle.ignore_changes`).
- Who else uses a var: `grep -rn "var.<name>" polaris-terraform/main-terraform/`.
