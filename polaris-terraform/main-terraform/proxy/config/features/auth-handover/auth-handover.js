import qs from "querystring";
import common from "../common/cms-detection.js";
import ieMode from "../common/ie-mode.js";

const IS_PROXY_SESSION_PARAM_NAME = "is-proxy-session";
const SESSION_HINT_COOKIE_NAME = "Cms-Session-Hint";
const SESSION_HINT_COOKIE_LIFESPAN_MS = 30 * 24 * 60 * 60 * 1000;

function _argsShim(args) {
  if (args["r"]) {
    return args;
  }
  // If we have no r param then we assume we are processing a legacy handover from the /polaris endpoint.
  // The CMS P button has no concept of the r param and assumes this endpoint forwards on to CWA domain.
  // So lets coerce the legacy format to the standard format by creating an r param if one does not exist.
  // Note 1: the expected incoming params in the legacy case are q and referer.
  // Note 2: we use a relative URL rather than a fully-qualified URL as the proxy runs under multiple names
  //  e.g. https://polaris-cmsproxy.azurewebsites.net/ and https://polaris.cps.gov.uk/

  const serializedArgs = qs.stringify(args);
  const clonedArgsToMutate = qs.parse(serializedArgs);
  delete clonedArgsToMutate["cookie"];
  delete clonedArgsToMutate[IS_PROXY_SESSION_PARAM_NAME];
  // Do not serialize cookie into our manufactured r param because cookie will be attached as the cc param later on.
  // Similarly do not include our "is-proxy-session" query parameter as that is artificially added by our
  // simulated proxy endpoint (if the user is using proxied CMS)
  const queryStringWithoutCookie = qs.stringify(clonedArgsToMutate);

  const clonedArgs = qs.parse(serializedArgs);
  clonedArgs["r"] = `/auth-refresh-inbound?${queryStringWithoutCookie}`;
  return clonedArgs;
}

function _redirectToAbsoluteUrl(r, redirectUrl) {
  // It appears that when we redirect with an absolute url, njs will create the location header starting with http://
  //  even if we are handling an https request. If we are running on https://foo then
  //  r.return(302, "https://foo/bar") will redirect to https://foo/bar
  //  r.return(302, "/bar") will redirect to http://foo/bar
  // So lets convert relative redirect to absolute.
  // Note: this almost is not a problem.  When the client comes back with the http://... request nginx will do another
  //  redirect to https as part of the "upgrade http to https" thing.  However the CWA cypress e2e test framework fails
  //  because tests running on https are redirected to an http address
  r.return(
    302,
    redirectUrl.lastIndexOf("http", 0) === 0
      ? redirectUrl
      : `${r.headersIn["X-Forwarded-Proto"]}://${r.headersIn["Host"]}${redirectUrl}`,
  );
}

function _getCookieValue(r, cookieName) {
  const cookies = r.headersIn["Cookie"] || "";
  const match = cookies.match(new RegExp(`(?:^|;\\s*)${cookieName}=([^;]*)`));
  return match ? match[1] : "";
}

function _maybeDecodeURIComponent(value) {
  // Check if value appears not to be URL-encoded
  // (does not contain %XX patterns)
  if (!/%[0-9A-Fa-f]{2}/.test(value)) {
    return value;
  }
  try {
    return decodeURIComponent(value);
  } catch (e) {
    return value;
  }
}

function setSessionHintCookie(r) {
  let cookieValue;
  try {
    const isProxySession = r.args[IS_PROXY_SESSION_PARAM_NAME] === "true";
    const cookie = r.args["cookie"];
    // The environment domain (e.g. cin3.cps.gov.uk) can come from either LB cookie style.
    // Prefer the load-balancing cookie, named [CF]-<TOKEN>-LBsessioncookie (e.g. C-CIN3-...,
    // F-FOO-...), and derive <token>.cps.gov.uk from the TOKEN. Fall back to the legacy
    // BIGipServer* cookie, whose name embeds the domain immediately before _POOL.
    const loadBalancingCookies =
      cookie.match(/(?:^|;\s*)[CF]-[^=;]*-LBsessioncookie/g) || [];
    const cmsDomains = loadBalancingCookies.length
      ? loadBalancingCookies.map(
          (m) =>
            `${m.match(/[CF]-([^=;]*)-LBsessioncookie/)[1].toLowerCase()}.cps.gov.uk`,
        )
      : // Match lowercase subdomain(s) followed by .cps.gov.uk (terminated by _POOL).
        // This avoids matching uppercase prefixes like CPSACP-LTM-CM-WAN-CIN3-.
        cookie.match(
          /[a-z][a-z0-9]*(?:\.[a-z][a-z0-9]*)*\.cps\.gov\.uk(?=_POOL)/g,
        ) || [];

    const handoverEndpoint = isProxySession
      ? `https://${r.headersIn["Host"]}/polaris`
      : cmsDomains.length
        ? // If there is more than one domain string found let's take the first
          // one. Analytics in global nav will tell us if there are ever multiple
          // domains found.
          `https://${cmsDomains[0]}/polaris`
        : null;

    cookieValue = {
      cmsDomains,
      isProxySession,
      handoverEndpoint,
    };
  } catch (error) {
    cookieValue = {
      error,
    };
  } finally {
    const expires = new Date(Date.now() + SESSION_HINT_COOKIE_LIFESPAN_MS);
    r.headersOut["Set-Cookie"] =
      `${SESSION_HINT_COOKIE_NAME}=${encodeURIComponent(
        JSON.stringify(cookieValue),
      )}; Path=/; Expires=${expires.toUTCString()}; Secure; SameSite=None`;
  }
}

function appAuthRedirect(r) {
  // Edge Mode Desired (was the two conf `if ($ieaction …)` gates on /init).
  if (ieMode.coerce(r, "edge", true)) return;
  setSessionHintCookie(r);

  const args = _argsShim(r.args);

  const whitelistedUrls = process.env.AUTH_HANDOVER_WHITELIST ?? "";
  const redirectUrl = args["r"];
  const isWhitelisted = whitelistedUrls
    .split(",")
    .some((url) => redirectUrl.startsWith(url));

  if (isWhitelisted) {
    _redirectToAbsoluteUrl(
      r,
      `${redirectUrl}${
        redirectUrl.includes("?") ? "&" : "?"
      }cc=${encodeURIComponent(args["cookie"] ?? "")}`,
    );
  } else {
    r.return(
      403,
      `HTTP Status 403: this deployment of the /init endpoint will only accept requests with r query parameters that start with one of the following strings: 
${whitelistedUrls}

This request has an r query parameter of ${args["r"]}`,
    );
  }
}

// This is a simulation of the https://cms.cps.gov.uk/polaris endpoint.
//  Primarily useful when users are using CMS delivered through this proxy. In this use case, users are on this proxy
//  domain when using CMS.  We inject a P button and simulated the prod /polaris handover endpoint using this function.
function polarisAuthRedirect(r) {
  // IE Mode Desired (was the conf `if ($ieaction = 'nonie+configurable+')` gate on
  // /polaris). No reject: a non-IE browser that can't switch just proceeds.
  if (ieMode.coerce(r, "ie", false)) return;
  const serializedArgs = qs.stringify(r.args);
  const clonedArgs = qs.parse(serializedArgs);
  clonedArgs.cookie = r.headersIn.Cookie;
  clonedArgs.referer = r.headersIn.Referer;
  clonedArgs[IS_PROXY_SESSION_PARAM_NAME] = "true";

  const querystring = qs.stringify(clonedArgs);
  _redirectToAbsoluteUrl(r, `/init?${querystring}`);
}

function handleAuthRefreshOutbound(r) {
  const tryGetHandoverEndpointFromCookie = () => {
    try {
      const rawCookie = _getCookieValue(r, "Cms-Session-Hint");
      if (rawCookie) {
        const decoded = _maybeDecodeURIComponent(rawCookie);
        const parsed = JSON.parse(decoded);
        if (parsed.handoverEndpoint) {
          return parsed.handoverEndpoint;
        }
      }
    } catch (e) {
      // JSON parse failure: fall through to default
    }
    return null;
  };

  const tryGetDefaultHandoverEndpoint = () => {
    const defaultDomain = process.env["DEFAULT_UPSTREAM_CMS_DOMAIN_NAME"] || "";
    return defaultDomain ? `https://${defaultDomain}/polaris` : null;
  };

  const redirectTarget =
    tryGetHandoverEndpointFromCookie() || tryGetDefaultHandoverEndpoint();

  if (!redirectTarget) {
    r.return(
      502,
      "auth-refresh-outbound: no handoverEndpoint in cookie and no DEFAULT_UPSTREAM_CMS_DOMAIN_NAME configured",
    );
    return;
  }

  const args = r.variables.args || "";
  const redirectUrl = args ? `${redirectTarget}?${args}` : redirectTarget;

  r.headersOut["X-InternetExplorerMode"] = "1";
  r.return(302, redirectUrl);
}

// The cookies the dev-login GET clears: the CMS-env marker plus, per CMS env,
// both BIG-IP pool cookies (ACP/AFP) and both load-balancer session cookies
// (C/F). Generated to a flat "<name>=deleted; ...expired" list — the old config
// hand-wrote all seventeen add_header lines twice.
const EXPIRE = "=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
const DEV_LOGIN_CLEARED = (function () {
  const names = ["__CMSENV"];
  const envs = ["CIN2", "CIN3", "CIN4", "CIN5"];
  for (let i = 0; i < envs.length; i++) {
    const E = envs[i],
      e = E.toLowerCase();
    names.push(
      `BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-${E}-${e}.cps.gov.uk_POOL`,
    );
    names.push(
      `BIGipServer~ent-s221~CPSAFP-LTM-CM-WAN-${E}-${e}.cps.gov.uk_POOL`,
    );
  }
  for (let i = 0; i < envs.length; i++) {
    names.push(`C-${envs[i]}-LBsessioncookie`);
    names.push(`F-${envs[i]}-LBsessioncookie`);
  }
  return names.map((n) => n + EXPIRE);
})();

// Dev-login cookie handling for BOTH /dev-login/ and /api/dev-login-full-cookie/
// (they differ only in their proxy_pass target). A single js_header_filter,
// branching on the request method — replaces the two identical if-blocks of
// add_header directives the two locations used to each carry:
//   GET  — clear the CMS-env marker + BIG-IP/LB cookies (a reset), appended to
//          whatever the upstream set (matching add_header's append semantics).
//   POST — stamp the detected CMS env into a readable __CMSENV cookie so the Open
//          Modern link works; the env is read from the OUTGOING Set-Cookie (what
//          this response is establishing) via common's shared detection rule.
function devLogin(r) {
  if (r.method === "GET") {
    const cookies = r.headersOut["Set-Cookie"] || [];
    r.headersOut["Set-Cookie"] = cookies.concat(DEV_LOGIN_CLEARED);
  } else if (r.method === "POST") {
    const cmsEnv = common.detect((r.headersOut["Set-Cookie"] || [""])[0]);
    const cookies = r.headersOut["Set-Cookie"] || [];
    cookies.push("__CMSENV=" + cmsEnv + "; path=/");
    r.headersOut["Set-Cookie"] = cookies;
  }
}

// ---------------------------------------------------------------------------
// Per-user enrolment (canary) — /auth-refresh-enrol
//
// A tiny page that sets/clears the `polaris_auth_handover` cookie so an INDIVIDUAL
// user can opt into drop1/drop2 ahead of the org-wide switch. The routing gate in
// auth-handover.conf (/auth-refresh-inbound) reads $cookie_polaris_auth_handover and
// gives it PRECEDENCE over the global ENTRA_STORE_ENABLED / NON_DDEI_INIT_ENABLED
// switches. Removing the cookie falls back to those globals (DDEI while they're off).
//
// Lives in the base auth-handover feature (not a drop folder) because it arbitrates
// BETWEEN the drops and DDEI, so it must outlive any single drop. Server-side only:
// the cookie is written here and read by nginx ($cookie_) — never by a script — so it
// is HttpOnly. GET-only; set/clear then 302 back to a clean URL (post/redirect/get) so
// a refresh doesn't re-apply and the address bar shows no action param.
// ---------------------------------------------------------------------------
const ENROL_COOKIE = "polaris_auth_handover";
const ENROL_PATH = "/auth-refresh-enrol";
const ENROL_MODES = ["drop1", "drop2"];
const ENROL_COOKIE_MAX_AGE = 30 * 24 * 60 * 60; // 30 days

function _enrolLabel(mode) {
  if (mode === "drop2") return "drop 2 — Entra store";
  if (mode === "drop1") return "drop 1 — non-DDEI init";
  return "default (follows the server switch)";
}

// Build the Set-Cookie: a mode string enrols for 30 days; "" clears (Max-Age=0 plus a
// past Expires so old IE honours it too). HttpOnly — only nginx reads it.
function _enrolSetCookie(mode) {
  const attrs = "; Path=/; Secure; HttpOnly; SameSite=Lax";
  if (!mode) {
    return (
      ENROL_COOKIE +
      "=" +
      attrs +
      "; Max-Age=0; Expires=Thu, 01 Jan 1970 00:00:00 GMT"
    );
  }
  return (
    ENROL_COOKIE + "=" + mode + attrs + "; Max-Age=" + ENROL_COOKIE_MAX_AGE
  );
}

function _enrolPage(current) {
  const links = ENROL_MODES.map(
    (m) =>
      '<li><a href="' +
      ENROL_PATH +
      "?set=" +
      m +
      '">Enrol in ' +
      _enrolLabel(m) +
      "</a></li>",
  ).join("");
  return (
    '<!DOCTYPE html><html lang="en"><head><meta charset="utf-8">' +
    '<meta name="viewport" content="width=device-width, initial-scale=1">' +
    "<title>Polaris auth handover — enrolment</title>" +
    "<style>body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;max-width:640px;margin:40px auto;padding:0 20px}li{margin:8px 0}code{background:#f0f0f0;padding:2px 6px;border-radius:3px}</style>" +
    "</head><body>" +
    "<h1>Auth handover enrolment</h1>" +
    "<p>You are currently on: <strong>" +
    _enrolLabel(current) +
    "</strong>.</p>" +
    "<ul>" +
    links +
    "</ul>" +
    '<p><a href="' +
    ENROL_PATH +
    '?clear=1">Reset to default</a> (removes the cookie).</p>' +
    "<p><small>This sets a <code>" +
    ENROL_COOKIE +
    "</code> cookie on this browser only; " +
    "it takes effect on your next CMS&nbsp;&rarr;&nbsp;Polaris handover.</small></p>" +
    "</body></html>"
  );
}

function enrol(r) {
  const set = r.args["set"];
  const clear = r.args["clear"];

  // PRG back to the page. Own absolute build (not _redirectToAbsoluteUrl) with a scheme
  // fallback: this page can be opened directly, where X-Forwarded-Proto is absent — that
  // helper would emit "undefined://". Default to https.
  const backToPage = () => {
    const proto = r.headersIn["X-Forwarded-Proto"] || "https";
    r.return(302, proto + "://" + r.headersIn["Host"] + ENROL_PATH);
  };

  // Enrol: only the known modes are accepted (never write an arbitrary cookie value).
  if (set !== undefined && ENROL_MODES.indexOf(set) !== -1) {
    r.headersOut["Set-Cookie"] = [_enrolSetCookie(set)];
    backToPage();
    return;
  }
  // Remove: clear the cookie, fall back to the global switches.
  if (clear !== undefined) {
    r.headersOut["Set-Cookie"] = [_enrolSetCookie("")];
    backToPage();
    return;
  }

  // No (valid) action — render the current enrolment + the enrol/reset links.
  const current = _getCookieValue(r, ENROL_COOKIE);
  r.headersOut["Content-Type"] = "text/html; charset=utf-8";
  r.return(200, _enrolPage(current));
}

// Feature-switch getters for the /auth-refresh-inbound routing gate (js_set).
// Read at REQUEST time from process.env, so a MISSING app setting reads as "off"
// rather than wedging nginx at boot — which is what `set $x "${ENTRA_STORE_ENABLED}"`
// (conf-side envsubst) did when the app setting was absent: envsubst left the token
// literal and nginx died with `[emerg] unknown "entra_store_enabled" variable`.
// js_set carries no `${...}` token, so envsubst never touches it and the config always
// boots. Returns the string "true"/"false" the conf's `if (... = "true")` compares.
// Parity with the old exact `= "true"` test: only the literal "true" enables.
function entraStoreEnabled(r) {
  return process.env.ENTRA_STORE_ENABLED === "true" ? "true" : "false";
}
function nonDdeiInitEnabled(r) {
  return process.env.NON_DDEI_INIT_ENABLED === "true" ? "true" : "false";
}

export default {
  polarisAuthRedirect,
  appAuthRedirect,
  handleAuthRefreshOutbound,
  devLogin,
  enrol,
  entraStoreEnabled,
  nonDdeiInitEnabled,
};
