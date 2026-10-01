// The CMS-upstream getters — a PUBLISHED CONTRACT, imported by nginx.conf as `cmsenv`.
//
// Why central, and why that name: global-components' feature configs (deployed into
// this proxy from the global-components repo as features/global-components.<x>/ and
// picked up by nginx.conf's `include features/*/*.conf;`) proxy CMS pages themselves,
// and do so with
//
//   js_set $proxyDestination cmsenv.proxyDestinationCorsham;   (etc.)
//
// — exactly the bindings the old monolith offered (js_import templates/cmsenv.js).
// A js_set variable is SERVER-GLOBAL and nginx refuses to boot if two js_sets bind
// the same variable to different "module.function" strings. So every config in this
// server — ours (cms-proxy.conf) and theirs — must bind these variables to these
// exact names. Do NOT rename the module binding or these exports, and do not re-home
// them into a feature: that silently breaks global-components' deploys. The contract
// is written up in docs/PROXY.md ("global-components contract").
//
// Detection and per-env settings come from the one shared rule in cms-detection.js.
import common from "./cms-detection.js";

// A getter for a raw upstream value, and one that prefixes the protocol to build a
// proxy_pass target. `name` is the setting's env-var name.
const upstream = (name) => (r) => common.setting(r, name);
const dest = (name) => (r) =>
  process.env.ENDPOINT_HTTP_PROTOCOL + "://" + common.setting(r, name);

export default {
  proxyDestinationCorsham: dest("UPSTREAM_CMS_IP_CORSHAM"),
  proxyDestinationModernCorsham: dest("UPSTREAM_CMS_MODERN_IP_CORSHAM"),
  upstreamCmsDomainName: upstream("UPSTREAM_CMS_DOMAIN_NAME"),
  upstreamCmsModernDomainName: upstream("UPSTREAM_CMS_MODERN_DOMAIN_NAME"),
  upstreamCmsServicesDomainName: upstream("UPSTREAM_CMS_SERVICES_DOMAIN_NAME"),
};
