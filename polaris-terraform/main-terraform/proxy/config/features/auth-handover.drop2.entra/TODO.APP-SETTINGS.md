# App settings to add before enabling auth-handover drop2

| Setting                   | Reason                                                                                                                          |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| `NON_DDEI_INIT_ENABLED`   | Arm drop1 (`"true"`); drop2 extends it.                                                                                         |
| `ENTRA_STORE_ENABLED`     | Arm drop2 (`"true"`).                                                                                                           |
| `ENTRA_TENANT_ID`         | Entra tenant id (required per env; empty if unset).                                                                             |
| `ENTRA_CLIENT_ID`         | Entra app-reg client id (required per env; empty if unset).                                                                     |
| `ENTRA_CLIENT_SECRET`     | Entra app-reg client secret (secret).                                                                                           |
| `ENTRA_STORAGE_ACCOUNT`   | Table Storage account (required; interim — store being replaced).                                                               |
| `ENTRA_STORAGE_KEY`       | Table Storage account key (secret); interim; no IaC path (foreign account) — manual until store swaps to the bearer-token seam. |
| `ENTRA_STORAGE_TABLE`     | Table Storage table name (optional; defaults to `cmsauth`); interim — store being replaced.                                     |
| `ENTRA_STATE_HMAC_SECRET` | HMAC key for the `entra_auth_state` cookie (secret); required — the sign/verify code fails closed if empty.                     |
