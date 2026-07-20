// Shared CMS-environment resolution primitive. A PURE ESM helper: it is NOT
// js_import'd by nginx — only `import`ed by the feature njs modules (cms-proxy.js,
// polaris-ddei.js, auth-handover.js). No feature logic — just the two irreducible
// primitives: `detect` a CMS environment from cookie text, and read a
// per-environment app setting from process.env. Each feature builds its own
// getters (js_set closures, protocol-prefixed proxy targets) on top of these.

// Cookie text -> environment token. NOTE: cin3 maps to "default". The shared
// detection rule: features that need it (e.g. auth-handover's dev-login, which
// detects from the outgoing Set-Cookie) call this rather than duplicate it.
function detect(cookie) {
  cookie = (cookie || "").toLowerCase();
  if (cookie.includes("cin3")) return "default";
  if (cookie.includes("cin2")) return "cin2";
  if (cookie.includes("cin4")) return "cin4";
  if (cookie.includes("cin5")) return "cin5";
  return "default";
}

// Read a per-environment CMS app setting from process.env: the value of
// <ENV>_<name>, where <ENV> is the request's environment (DEFAULT/CIN2/CIN4/CIN5,
// detected from the incoming Cookie header) and <name> is the setting's own
// env-var name — e.g. setting(r, "UPSTREAM_CMS_MODERN_DOMAIN_NAME") ->
// process.env.CIN4_UPSTREAM_CMS_MODERN_DOMAIN_NAME.
function setting(r, name) {
  return process.env[detect(r.headersIn.Cookie).toUpperCase() + "_" + name];
}

// Just the two primitives. The getter factories (upstream/dest) live in each
// feature that needs them, built on `setting` — nothing here is js_set by nginx
// directly, and no getter is shared. `detect` is exposed because a feature may
// need to detect from a different header than the getters do (auth-handover's
// dev-login detects from the outgoing Set-Cookie).
export default {
  detect,
  setting,
};
