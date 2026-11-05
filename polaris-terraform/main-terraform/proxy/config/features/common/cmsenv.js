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
//
// Only these cookies choose the environment, picked out by NAME; every other cookie is ignored,
// whatever it contains (Cms-Session-Hint's value names a CMS domain, for one):
//   1. __CMSENV (the /cinN switches, dev login) or DEVLOGIN (sent by DDEI's and our dev logins
//      when calling /CMS), when the value names an environment: the explicit choice, so it wins.
//   2. Otherwise the load-balancer cookies, whose NAMES carry the environment. There is no
//      guarantee of an explicit choice: DDEI's other calls carry only the CMS cookies.
// __CMSENV=cpt is not a choice here (#2181): cpt is selected by its LB cookie alone.
// Mirrored in the live config's cmsenv.js (__getCmsEnvInternal) until cutover.
const LB_COOKIE_NAME = /^(?:[cf]-[a-z0-9]+-lbsessioncookie|bigipserver.*)$/;

function detect(cookie) {
  let marker = "";
  const lbNames = [];
  (cookie || "").toLowerCase().split(";").forEach((pair) => {
    const eq = pair.indexOf("=");
    if (eq === -1) return;
    const name = pair.substring(0, eq).trim();
    if (name === "__cmsenv" || name === "devlogin") marker = marker || pair.substring(eq + 1).trim();
    else if (LB_COOKIE_NAME.test(name)) lbNames.push(name);
  });

  if (marker === "cin3" || marker === "default") return "default";
  if (marker === "cin2" || marker === "cin4" || marker === "cin5") return marker;

  const lb = lbNames.join(" ");
  if (lb.includes("cin3")) return "default";
  if (lb.includes("cin2")) return "cin2";
  if (lb.includes("cin4")) return "cin4";
  if (lb.includes("cin5")) return "cin5";
  if (lb.includes("-cpt-lb")) return "cpt";
  // Anything else, including CMO: it has no dedicated upstream (RMAT-242), and the default env
  // is cmo.cps.gov.uk in UAT.
  return "default";
}

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
