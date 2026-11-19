# QUIRKS — cwa-materials

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

Live line numbers are the live `main-terraform/nginx.conf` (the golden master); the
routes now live in `cwa-materials.conf` in this folder.

---

## D. Config smells and cleanup opportunities

### D3. 🟠 `/v2/` hardcodes the App Insights hostname

**Where (live):** `nginx.conf:940` — `proxy_pass https://uksouth-1.in.applicationinsights.azure.com/v2/;`
**Where (next):** `cwa-materials.conf` (the `/v2/` App Insights route)
**Test:** `cwa-materials.integration.test.js` → "proxies to the App Insights host"

The only upstream with no app setting; the region (`uksouth-1`) is baked in. Also
appears as a `sub_filter` target in the SPA/Materials blocks.

**Suggested fix:** make it an app setting like every other endpoint.

### D8. ⚪ `/materials-ui/{a}/{b}/materials` looks redundant with `/materials-ui/`

**Where (live):** `nginx.conf:701` vs `:709`
**Where (next):** `cwa-materials.conf` (`location ~ ^/materials-ui/[^/]+/[^/]+/materials$` vs `location /materials-ui/`)
**Test:** `cwa-materials.integration.test.js` → "deep-link regex (nginx.conf:701) wins over the /materials-ui/ prefix", "deep-link regex only matches exactly two segments + /materials"

Added by FCT2-15354 for the M-button deep link. Both send the identical path
upstream; the only differences are `proxy_pass` with vs without a URI, and the
regex block having _fewer_ `sub_filter_types`. Strong drop candidate — but verify
against a real deep link first (the URI-vs-bare `proxy_pass` distinction affects
URI normalisation).
