// Feature 6 🔌 — polaris-ddei internal routes.
//
// This feature is self-contained (it is slated for deletion — QUIRKS.md A4
// / PLAN.md Phase 3), so it owns EVERY CMS-upstream getter its conf js_sets,
// built from its own getter factories over common.setting. Some compute the same
// value as getters in cms-proxy.js (the Corsham dests + domain getters) — that
// duplication is deliberate: it keeps this a clean, independently-deletable unit
// and lets its conf js_set its own *Internal-suffixed nginx variables with no
// server-global collision against cms-proxy's. Deleting the feature = delete this
// file + its conf, touching nothing else.
import common from "../common/cms-detection.js";

// Getter factories, built on common.setting (kept feature-local — see common/cms-detection.js).
const upstream = (name) => (r) => common.setting(r, name);
const dest = (name) => (r) => process.env.ENDPOINT_HTTP_PROTOCOL + "://" + common.setting(r, name);

export default {
  // shared-value getters (own copies; also in cms-proxy.js)
  proxyDestinationCorsham: dest("UPSTREAM_CMS_IP_CORSHAM"),
  proxyDestinationModernCorsham: dest("UPSTREAM_CMS_MODERN_IP_CORSHAM"),
  upstreamCmsDomainName: upstream("UPSTREAM_CMS_DOMAIN_NAME"),
  upstreamCmsModernDomainName: upstream("UPSTREAM_CMS_MODERN_DOMAIN_NAME"),
  upstreamCmsServicesDomainName: upstream("UPSTREAM_CMS_SERVICES_DOMAIN_NAME"),
  // getters unique to this feature (Farnborough dests + all four DC IPs)
  proxyDestinationFarnborough: dest("UPSTREAM_CMS_IP_FARNBOROUGH"),
  proxyDestinationModernFarnborough: dest("UPSTREAM_CMS_MODERN_IP_FARNBOROUGH"),
  upstreamCmsIpCorsham: upstream("UPSTREAM_CMS_IP_CORSHAM"),
  upstreamCmsModernIpCorsham: upstream("UPSTREAM_CMS_MODERN_IP_CORSHAM"),
  upstreamCmsIpFarnborough: upstream("UPSTREAM_CMS_IP_FARNBOROUGH"),
  upstreamCmsModernIpFarnborough: upstream("UPSTREAM_CMS_MODERN_IP_FARNBOROUGH"),
};
