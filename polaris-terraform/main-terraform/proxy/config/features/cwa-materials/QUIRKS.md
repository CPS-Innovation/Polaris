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

**Update (#2189, 2026-10-01):** the case URN was removed from the entry points
(`/materials`, `openMaterials()`, `polaris-script.js`), which now build the
single-segment `/materials-ui/{caseId}/materials`. That no longer matches this
two-segment regex, so it falls to the `/materials-ui/` prefix block — the regex is now
reachable only by a hand-built two-segment URL. Even stronger drop candidate (live
still carries it; left in place for parity). **Note (D13):** while #2189 is held back in the next config, the next
config still builds the two-segment `{caseUrn}/{caseId}` URL, so this regex is reachable there.

### D13. 🟠 #2189 (materials URN removal) is HELD BACK in the next config — deliberate divergence

**Where (live):** `nginx.conf` (`/materials`, injected `openMaterials()`), `polaris-script.js` — **have** #2189
**Where (next):** `cwa-materials.conf` (`/materials`), `features/cms-proxy/cms-proxy.conf` (`openMaterials()`),
`features/cms-proxy/polaris-script.js` — **pre-#2189** (URN form)
**Test:** per-config (`isNext`) in `cwa-materials.integration.test.js` and `cms-proxy.integration.test.js`
(`NEXT (pre-#2189)` / `LIVE (#2189)` tests)

#2189 (`4e74401eb`, "Remove case URN from proxy entry point") is on `development`, but was judged **not
ready for QA** (2026-10-06). QA runs the next config, which is pushed out of band, so the next-config
copies were reverted to the URN form (the `/materials?caseUrn=…&caseId=…` handover, the
`{caseUrn}/{caseId}/materials` deep link, `openMaterials()` with its `sURN` / `caseUrn` / `<td>` scan,
and `polaris-script.js` needing both `iCaseId` and `sURN`). The live monolith keeps #2189. This is the
**only** intended behavioural difference between the two configs, and the tests assert each side.

**To release #2189 to the next config:** revert the hold-back commit, then collapse the `isNext` branches
back to the `LIVE (#2189)` expectations.

