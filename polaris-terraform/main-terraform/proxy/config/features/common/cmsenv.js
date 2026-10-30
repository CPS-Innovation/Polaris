// CMS environment: the one shared detection rule, and the CMS-upstream getters — a PUBLISHED
// CONTRACT, imported by nginx.conf as `cmsenv`.
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
// Our feature modules `import` this file too (auth-handover, cms-proxy, polaris-ddei),
// for `detect` and `setting` — the two primitives the getters are built on. Those two
// are NOT for js_set: `detect` takes cookie text, not a request.

// Cookie text -> environment token. NOTE: cin3 maps to "default". The shared
// detection rule: features that need it (e.g. auth-handover's dev-login, which
// detects from the outgoing Set-Cookie) call this rather than duplicate it.
function detect(cookie) {
  cookie = (cookie || "").toLowerCase();
  if (cookie.includes("cin3")) return "default";
  if (cookie.includes("cin2")) return "cin2";
  if (cookie.includes("cin4")) return "cin4";
  if (cookie.includes("cin5")) return "cin5";
  if (cookie.includes("-cpt-lb")) return "cpt";
  // CMO has no dedicated upstream (RMAT-242); route it to the default env, which is
  // cmo.cps.gov.uk in UAT.
  if (cookie.includes("-cmo-lb")) return "default";
  return "default";
}
// NOTE (parity with the live config, FCT2-18732): the substring scan is deliberately
// naive — there is no guarantee of a __CMSENV cookie, so we fall through to whatever
// env token appears in the LB/BIG-IP cookies. Known-fragile (a stray "mod"/"cpt" substring
// mis-routes); to be addressed wholesale later, not here.

// Read a per-environment CMS app setting from process.env: the value of
// <ENV>_<name>, where <ENV> is the request's environment (DEFAULT/CIN2/CIN4/CIN5/CPT/CMO,
// detected from the incoming Cookie header) and <name> is the setting's own
// env-var name — e.g. setting(r, "UPSTREAM_CMS_MODERN_DOMAIN_NAME") ->
// process.env.CIN4_UPSTREAM_CMS_MODERN_DOMAIN_NAME.
function setting(r, name) {
  return process.env[detect(r.headersIn.Cookie).toUpperCase() + "_" + name];
}

// A getter for a raw upstream value, and one that prefixes the protocol to build a
// proxy_pass target. `name` is the setting's env-var name.
const upstream = (name) => (r) => setting(r, name);
const dest = (name) => (r) =>
  process.env.ENDPOINT_HTTP_PROTOCOL + "://" + setting(r, name);

export default {
  // The contract (js_set by our confs and global-components').
  proxyDestinationCorsham: dest("UPSTREAM_CMS_IP_CORSHAM"),
  proxyDestinationModernCorsham: dest("UPSTREAM_CMS_MODERN_IP_CORSHAM"),
  upstreamCmsDomainName: upstream("UPSTREAM_CMS_DOMAIN_NAME"),
  upstreamCmsModernDomainName: upstream("UPSTREAM_CMS_MODERN_DOMAIN_NAME"),
  upstreamCmsServicesDomainName: upstream("UPSTREAM_CMS_SERVICES_DOMAIN_NAME"),
  // The primitives, for our feature modules' imports (not for js_set).
  detect,
  setting,
};
