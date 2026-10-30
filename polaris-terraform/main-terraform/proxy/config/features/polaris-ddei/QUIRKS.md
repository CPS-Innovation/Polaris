# QUIRKS — polaris-ddei

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

This feature is slated for deletion (§4 / [`../../../docs/PLAN.md`](../../../docs/PLAN.md)
Phase 3), so both quirks below are things to _drop_ rather than fix.

Live line numbers are the live `main-terraform/{nginx.conf,cmsenv.js}` (the golden
master); the code now lives in `polaris-ddei.{conf,js}` in this folder.

---

## A. Security-relevant

### A4. ⚪ `/internal-implementation/*` is "internal-only" by convention only

**Where (live):** `nginx.conf:852, 887, 921, 956` — 0 `allow`/`deny` directives in any of them
**Where (next):** `polaris-ddei.conf` (`/internal-implementation/{corsham,farnborough}[/modern]/`)
**Test:** `polaris-ddei.integration.test.js` (they are freely reachable in the harness)

The comments say "internal-only route to CMS Classic, used by DDEI", but nothing
in nginx enforces that — it rests entirely on Azure network isolation (the app is
`public_network_access_enabled = false` behind a private endpoint). Correct today,
but there is no defence in depth, and the constraint is invisible to a reader of
the config.

**Suggested fix:** none needed if the network boundary is trusted; worth an
explicit comment, or an `allow`/`deny` pair, given §4 marks these for deletion
anyway.

---

## D. Config smells and cleanup opportunities

### D7. ⚪ The four `*Internal` cmsenv functions are pure delegates

**Where (live):** `cmsenv.js:13-15, 22-24, 31-33, 40-42`
**Where (next):** the `*Internal` js_set names belong to this feature's
`internal-implementation` routes. In the refactor, `polaris-ddei.js` is
self-contained and owns its own getter copies (a deliberate duplication so the
feature can be deleted as one unit) — so collapsing the delegate layer happens
naturally when this feature is dropped.
**Test:** `polaris-ddei.unit.test.js` → "All getters this feature owns, every
environment" — its own copies give the same per-env values that
`common/cmsenv.unit.test.js` pins for the public getters (which live in
`common/cmsenv.js`). The next config has no `*Internal` functions: `polaris-ddei.conf`
js_sets its `*Internal`-named variables straight from those copies.

`proxyDestinationCorshamInternal(r)` is literally `return proxyDestinationCorsham(r)`.
They exist only to give the internal routes a distinct `js_set` name. Free to
collapse in §6 — and removed outright when the feature is deleted in Phase 3.
