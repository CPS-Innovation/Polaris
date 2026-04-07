const SESSION_HINT_COOKIE_NAME = "Cms-Session-Hint"
const CMS_AUTH_VALUES_COOKIE_NAME = "Cms-Auth-Values"
// Plenty of hardcoded stuff elsewhere in the nginx config. Let's keep only things
//  that are sensitive or trigger differences in the ENV/App settings.
const CORS_ALLOWED_ORIGINS = [
  "https://cps.outsystemsenterprise.com",
  "https://cps-tst.outsystemsenterprise.com",
  "https://cps-dev.outsystemsenterprise.com",
  "http://localhost",
  "https://localhost",
  "http://127.0.0.1",
  "https://127.0.0.1",
  "https://lacc-app-ui-spa-staging.azurewebsites.net",
  "https://lacc-app-ui-spa-dev.azurewebsites.net",
  // see later for check for localhost with port
]
const STATE_COOKIE_NAME = "cps-global-components-state";
const STATE_COOKIE_LIFESPAN_MS = 365 * 24 * 60 * 60 * 1000;
function _getHeaderValue(r, headerName) {
  return r.headersIn[headerName] || ""
}
function _getCookieValue(r, cookieName) {
  const cookies = _getHeaderValue(r, "Cookie")
  const match = cookies.match(new RegExp(`(?:^|;\\s*)${cookieName}=([^;]*)`))
  return match ? match[1] : ""
}
function _maybeDecodeURIComponent(value) {
  // Check if value appears not to be URL-encoded
  // (does not contain %XX patterns)
  if (!/%[0-9A-Fa-f]{2}/.test(value)) {
    return value;
  }
  try {
    return decodeURIComponent(value);
  }
  catch (e) {
    return value;
  }
}
function _base64UrlDecode(str) {
  // Replace base64url chars with base64 chars
  str = str.replace(/-/g, "+").replace(/_/g, "/");
  // Pad if necessary
  while (str.length % 4) {
    str += "=";
  }
  return atob(str);
}
function _base64UrlEncode(str) {
  // Encode to base64, then convert to base64url (URL-safe, no padding)
  return btoa(str).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}
// Wrap state for storage (encode). Currently base64url, could add encryption later.
function _wrapState(plaintext) {
  return _base64UrlEncode(plaintext);
}
// Unwrap state from storage (decode). Currently base64url, could add decryption later.
function _unwrapState(wrapped) {
  try {
    return _base64UrlDecode(wrapped);
  }
  catch (e) {
    // If decode fails, return null (corrupted or legacy data)
    return null;
  }
}
function _buildCookieString(name, value, options) {
  let cookie = name + "=" + value;
  if (options === null || options === void 0 ? void 0 : options.Path) {
    cookie += "; Path=" + options.Path;
  }
  if (options === null || options === void 0 ? void 0 : options.Expires) {
    cookie += "; Expires=" + options.Expires.toUTCString();
  }
  if (options === null || options === void 0 ? void 0 : options.Secure) {
    cookie += "; Secure";
  }
  if (options === null || options === void 0 ? void 0 : options.SameSite) {
    cookie += "; SameSite=" + options.SameSite;
  }
  return cookie;
}
function _setCookie(r, name, value, options) {
  const cookie = _buildCookieString(name, value, options);
  // njs headersOut["Set-Cookie"] accepts either string or string[]
  // For multiple cookies, we need to use an array
  const existing = r.headersOut["Set-Cookie"];
  if (existing) {
      if (Array.isArray(existing)) {
          existing.push(cookie);
      }
      else {
          // Convert single string to array and add new cookie
          r.headersOut["Set-Cookie"] = [existing, cookie];
      }
  }
  else {
      // First cookie - use string for compatibility
      r.headersOut["Set-Cookie"] = cookie;
  }
}
function _extractDomainFromSessionHint(r) {
  var cookies = r.headersIn["Cookie"] || "";
  var match = cookies.match(/Cms-Session-Hint=([^;]+)/);
  if (!match)
      return "";
  try {
      var decoded = decodeURIComponent(match[1]);
      var parsed = JSON.parse(decoded);
      var endpoint = parsed.handoverEndpoint || "";
      var domainMatch = endpoint.match(/https?:\/\/([^\/]+)/);
      return domainMatch ? domainMatch[1] : "";
  }
  catch (e) {
      return "";
  }
}

function readCmsAuthValues(r) {
  return _maybeDecodeURIComponent(
    _getHeaderValue(r, CMS_AUTH_VALUES_COOKIE_NAME) ||
      _getCookieValue(r, CMS_AUTH_VALUES_COOKIE_NAME)
    )
}

// For nginx js_set - returns origin if allowed, empty string otherwise
function readCorsOrigin(r) {
  const origin = r.headersIn["Origin"]
  return CORS_ALLOWED_ORIGINS.includes(origin) ||
    origin.endsWith(".cps.gov.uk") ||
    origin.startsWith("http://localhost:") ||
    origin.startsWith("https://localhost:") ||
    origin.startsWith("http://127.0.0.1:") ||
    origin.startsWith("https://127.0.0.1:")
    ? origin
    : ""
}
function handleSessionHint(r) {
  const hintValue = _getCookieValue(r, SESSION_HINT_COOKIE_NAME)
  r.return(200, hintValue ? _maybeDecodeURIComponent(hintValue) : "null")
}
async function handleState(r) {
  if (!["GET", "PUT"].includes(r.method)) {
    // Method not allowed
    r.return(405, JSON.stringify({ error: "Method not allowed" }));
    return;
  }
  r.headersOut["Content-Type"] = "application/json";
  if (r.method === "GET") {
    // Get wrapped state from cookie and unwrap it
    const cookieValue = _getCookieValue(r, STATE_COOKIE_NAME);
    if (!cookieValue) {
      r.return(200, "null");
      return;
    }
    const unwrapped = _unwrapState(_maybeDecodeURIComponent(cookieValue));
    r.return(200, unwrapped !== null ? unwrapped : "null");
    return;
  }
  if (r.method === "PUT") {
    const body = (r.requestText || "").trim();
    // If body is "null" or empty, clear the cookie by setting it to expire in the past
    if (body === "null" || body === "") {
      _setCookie(r, STATE_COOKIE_NAME, "", {
        Path: r.uri,
        Expires: new Date(0), // Expire immediately (clears cookie)
        Secure: true,
        SameSite: "None",
      });
      r.return(200, JSON.stringify({ success: true, path: r.uri, cleared: true }));
      return;
    }
    // Wrap the body and store in cookie
    const wrapped = _wrapState(body);
    _setCookie(r, STATE_COOKIE_NAME, wrapped, {
      Path: r.uri,
      Expires: new Date(Date.now() + STATE_COOKIE_LIFESPAN_MS), // 1 year
      Secure: true,
      SameSite: "None",
    });
    r.return(200, JSON.stringify({ success: true, path: r.uri }));
    return;
  }
}
function handleNavigateCms(r) {
  var ieaction = r.variables.ieaction || "";
  var step = r.args.step || "";
  var proto = r.headersIn["X-Forwarded-Proto"] || "https";
  var host = r.headersIn["Host"] || "";
  // === CLOSE PHASE ===
  if (step === "close") {
    // If in IE mode with configurable: redirect to self to exit IE mode
    if (ieaction === "ie+configurable+") {
      r.headersOut["X-InternetExplorerMode"] = "0";
      r.return(302, proto + "://" + host + r.uri + "?step=close");
      return;
    }
    // Now in Edge (or non-configurable): close the window
    r.headersOut["Content-Type"] = "text/html";
    r.return(200, "<html><body><script>window.close();</script></body></html>");
    return;
  }
  // === OPEN PHASE ===
  // If non-IE browser with configurable IE mode: extract CMS domain from
  // cookie (only available in Edge, not in IE mode) and pass it as a query
  // param so IE mode can use it.
  if (ieaction === "nonie+configurable+") {
    var domain = _extractDomainFromSessionHint(r);
    if (!domain) {
      r.headersOut["Content-Type"] = "text/html";
      r.return(400, "<html><body><p>Error: could not determine CMS domain from session.</p></body></html>");
      return;
    }
    var args = r.variables.args || "";
    var separator = args ? "&" : "";
    r.headersOut["X-InternetExplorerMode"] = "1";
    r.return(302, proto +
      "://" +
      host +
      r.uri +
      "?" +
      args +
      separator +
      "cmsDomain=" +
      encodeURIComponent(domain));
    return;
  }
  // Now in IE mode (or non-configurable): get domain from query param (IE) or cookie (non-configurable)
  var cmsDomain = r.args.cmsDomain || _extractDomainFromSessionHint(r);
  if (!cmsDomain) {
    r.headersOut["Content-Type"] = "text/html";
    r.return(400, "<html><body><p>Error: could not determine CMS domain from session.</p></body></html>");
    return;
  }
  var caseId = r.args.caseId || "";
  var taskId = r.args.taskId || "";
  var iframeSrc = taskId
    ? proto +
        "://" +
        cmsDomain +
        "/CMSModern/Navigation/Notification.html?action=activate_task&screen=case_details&wId=MASTER&taskId=" +
        taskId +
        "&caseId=" +
        caseId
    : proto +
        "://" +
        cmsDomain +
        "/CMSModern/Navigation/Notification.html?action=navigate&screen=case_details&wId=MASTER&caseId=" +
        caseId;
  var closeUrl = "/global-components/navigate-cms?step=close";
  var heading = taskId ? "Opening task in CMS" : "Opening case in CMS";
  r.headersOut["Content-Type"] = "text/html";
  r.return(200, "<!DOCTYPE html>" +
    "<html><head><title>" +
    heading +
    "</title></head>" +
    '<body style="font-family: Arial, sans-serif; margin: 30px;">' +
    '<h1 style="font-size: 24px; font-weight: 700; margin: 0 0 20px 0;">' +
    heading +
    "</h1>" +
    '<div style="border-left: 10px solid #b1b4b6; padding: 15px; margin: 0; clear: both;">' +
    '<p style="font-size: 16px; margin: 0 0 10px 0;">This may take a few seconds.</p>' +
    '<p style="font-size: 16px; margin: 0;">Please do not close this window. It will close automatically when CMS has finished navigating.</p>' +
    "</div>" +
    '<iframe src="' +
    iframeSrc +
    '" style="display:none" onload="window.location.href=\'' +
    closeUrl +
    "'\"></iframe>" +
    "</body></html>");
}
function handleCaseReviewRedirect(r) {
  const proto = r.headersIn["X-Forwarded-Proto"] || "https";
  const host = r.headersIn["Host"] || "";
  // URI is /case-review-redirect/{osSubdomain}/{envFolder}
  // e.g. /case-review-redirect/cps-tst/test
  const parts = r.uri.split("/");
  const osSubdomain = parts[2] || "";
  const envFolder = parts[3] || "";
  if (!osSubdomain || !envFolder) {
    r.return(400, "case-review: expected path /case-review-redirect/{osSubdomain}/{envFolder}");
    return;
  }
  const osDomain = `${osSubdomain}.outsystemsenterprise.com`;
  const caseId = r.args["CMSCaseId"] || "";
  const urn = r.args["URN"] || "";
  if (!caseId) {
    r.return(400, "case-review: CMSCaseId query parameter is required");
    return;
  }
  // Final destination: OutSystems CaseReview app
  const finalDest = `https://${osDomain}/CaseReview/LandingPage?CMSCaseId=${caseId}&URN=${urn}`;
  // Auth handover page with src (our JS), stage, and encoded final destination
  const authHandoverJs = `${proto}://${host}/global-components/${envFolder}/auth-handover.js`;
  const authHandoverPage = `https://${osDomain}/Casework_Patterns/auth-handover.html` +
    `?src=${encodeURIComponent(authHandoverJs)}` +
    `&stage=os-cookie-return` +
    `&r=${encodeURIComponent(finalDest)}`;
  // Redirect through /auth-refresh-outbound which determines the correct
  // CMS polaris endpoint from the Cms-Session-Hint cookie (or default domain)
  r.return(302, `${proto}://${host}/auth-refresh-outbound?r=${encodeURIComponent(authHandoverPage)}`);
}
export default {
  readCmsAuthValues,
  readCorsOrigin,
  handleSessionHint,
  handleState,
  handleNavigateCms,
  handleCaseReviewRedirect,
  // Export helper functions for vnext
  _getCookieValue,
  _maybeDecodeURIComponent,
}
