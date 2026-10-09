# App settings to add before enabling auth-handover drop2

| Setting                   | Reason                                                                                                                       |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `NON_DDEI_INIT_ENABLED`   | Arm drop1 (`"true"`); drop2 extends it. Terraform-managed per env (`var.non_ddei_init_enabled`).                            |
| `ENTRA_STORE_ENABLED`     | Arm drop2 (`"true"`).                                                                                                        |
| `ENTRA_TENANT_ID`         | Entra tenant id.                                                                                                             |
| `ENTRA_CLIENT_ID`         | Our client app reg (QA: `8d6133af-9593-47c6-94d0-5c65e9e310f1`).                                                            |
| `ENTRA_CLIENT_SECRET`     | Its client secret (secret). Used for the code exchange AND the on-behalf-of swap.                                           |
| `ENTRA_STATE_HMAC_SECRET` | HMAC key for the `entra_auth_state` cookie (secret); required — sign/verify fails closed if empty.                          |
| `ENTRA_APP_SCOPE`         | The NEUTRAL scope our app exposes (QA: `api://8d6133af-9593-47c6-94d0-5c65e9e310f1/api.presence.user.readwrite`).           |
| `ENTRA_MDS_SCOPE`         | MDS's delegated scope for the swap (QA: `api://fa-wm-app-ddei-staging/full_scope`; UAT's MDS app reg has no scopes yet).    |

Reused, already present (terraform, Key Vault): `WM_MDS_BASE_URL`, `WM_MDS_ACCESS_KEY`.

Removed with the Table Storage backend (stale values on an App Service are harmless — unused):
`ENTRA_STORAGE_ACCOUNT`, `ENTRA_STORAGE_KEY`, `ENTRA_STORAGE_TABLE`.

Entra prerequisites (not settings): `/init-entra/callback` registered (web) on our app reg for each
host; the MDS delegated permission on our app reg, with our client **pre-authorised** on the MDS app
reg (MDS terraform `global_components_client_id`) for consent.
