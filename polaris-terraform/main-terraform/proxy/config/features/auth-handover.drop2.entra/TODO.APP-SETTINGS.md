# App settings for auth-handover drop2

All terraform-managed, on both the production slot (`app-service-proxy.tf`) and `staging1`
(`app-service-proxy-staging.tf`, which deploys swap into production), from the per-env
`proxy_entra` variable in `*.tfvars`.

| Setting                   | Source in terraform                                         | Reason                                                                                     |
| ------------------------- | ----------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `NON_DDEI_INIT_ENABLED`   | `var.non_ddei_init_enabled`                                 | Arm drop1 (`"true"`); drop2 extends it.                                                    |
| `ENTRA_STORE_ENABLED`     | `var.proxy_entra.store_enabled`                             | Arm drop2 (`"true"`).                                                                      |
| `ENTRA_TENANT_ID`         | `data.azurerm_client_config.current.tenant_id`              | Entra tenant id.                                                                           |
| `ENTRA_CLIENT_ID`         | `var.proxy_entra.client_id`                                 | Our client app reg (pre-prod: `8d6133af-9593-47c6-94d0-5c65e9e310f1`).                     |
| `ENTRA_CLIENT_SECRET`     | Key Vault `GlobalComponents-Entra-ClientSecret` (manual)    | Its client secret. Used for the code exchange AND every refresh-token swap.                   |
| `ENTRA_STATE_HMAC_SECRET` | `random_password.proxy_entra_state_hmac`                    | HMAC key for the `entra_auth_state` cookie; sign/verify fails closed if empty.             |
| `ENTRA_WATCHDOG_SCOPE`    | `var.proxy_entra.watchdog_scope`                            | The watchdog (presence API) scope the refresh token is swapped for, for the presence cookie. Pre-prod (interim): `api://8d6133af-9593-47c6-94d0-5c65e9e310f1/api.presence.user.readwrite`; prod `""`. Becomes the watchdog's own app reg scope by config alone. |
| `ENTRA_MDS_SCOPE`         | `var.proxy_entra.mds_scope`                                 | MDS's delegated scope for the refresh-token swap (dev `…ddei-dev/full_scope`, QA `…ddei-staging/full_scope`; none in UAT yet). |

Reused, already present (terraform, Key Vault): `WM_MDS_BASE_URL`, `WM_MDS_ACCESS_KEY`.

Removed with the Table Storage backend (stale values on an App Service are harmless — unused):
`ENTRA_STORAGE_ACCOUNT`, `ENTRA_STORAGE_KEY`, `ENTRA_STORAGE_TABLE`.

Prerequisites (not settings), per env where `store_enabled = true`:
- the Key Vault secret `GlobalComponents-Entra-ClientSecret` in `kv_polaris` (the apply fails without it);
- `/init-entra/callback` registered (web) on our app reg for the env's host;
- the MDS delegated permission on our app reg, with our client **pre-authorised** on the MDS app reg
  (MDS terraform `global_components_client_id`) for consent;
- consent on our app reg for Graph `openid profile offline_access` (pre-prod `8d6133af` has it: the
  global-components SPA uses it). Our app reg exposes **no** API: there is no OBO.
