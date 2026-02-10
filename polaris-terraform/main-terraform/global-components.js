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
  // see later for check for localhost with port
]

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
    return value
  }
  try {
    return decodeURIComponent(value)
  } catch (e) {
    return value
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

export default {
  readCmsAuthValues,
  readCorsOrigin,

  handleSessionHint,

  // Export helper functions for vnext
  _getCookieValue,
  _maybeDecodeURIComponent,
}
