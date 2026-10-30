# QUIRKS — app-launch

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

Live line numbers are the live `main-terraform/nginx.conf` (the golden master); the
routes now live in `app-launch.conf` in this folder.

---

## D. Config smells and cleanup opportunities

### D12. ⚪ `/launch/*`: one route is env-driven, thirteen hardcode their domains

**Where (live):** `nginx.conf:653-694`
**Where (next):** `app-launch.conf` (`location /launch/`)
**Test:** `app-launch.integration.test.js`

Only `/launch/cms` uses `${DEFAULT_UPSTREAM_CMS_DOMAIN_NAME}`; `/launch/cmo`, `/launch/cpt`,
`/launch/cin2`–`cin5` and all seven `-proxy` variants hardcode `cmo` / `cmscpt` /
`cinN.cps.gov.uk` / `polaris-{uat,qa}-notprod.cps.gov.uk`, including prod-only and test-only OutSystems URLs
in the same file. §4 notes they "can be collapsed into one (maybe needs njs)".
