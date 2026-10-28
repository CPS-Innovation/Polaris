// ---------------------------------------------------------------------------
// Non-DDEI init — DDEI's /api/init/ reimplemented entirely in njs (switch-gated), plus the
// rest of the auth handover DDEI answered (termination, cms-modern-token; dev-login reuses
// this module) — see `enabled` at the bottom.
//
// Reached via an internal rewrite from /auth-refresh-inbound when NON_DDEI_INIT_ENABLED or
// ENTRA_STORE_ENABLED is on, or the browser is enrolled (drop1 or drop2) — the gate lives in
// auth-handover.conf. It runs the DDEI
// InitiateCookies pipeline in njs:
//   mint the CMS modern token -> verify it via GraphQL
//   -> set the Cms-Auth-Values cookie -> redirect (polaris-ui-url, or /polaris-ui/go).
//
// DDEI's first step — whitelist the CMS cookies, top up WindowID=MASTER — is NOT
// here: /init (auth-handover appAuthRedirect) does it once for every consumer, so
// `cc` arrives already filtered.
//
// The token mint + GraphQL verify are $host loopback fetches that re-enter the
// proxy's own ^/CMS.* routing (which picks the datacentre from the cookies), so this
// needs NO /internal-implementation/* routes — which is what lets polaris-ddei die.
// Hence the resolver + js_fetch_verify off in the conf.
//
// ISOLATED / NEW-GEN: NOT part of the golden-master before/after. Dormant with the
// switch off. Deliberately simplified vs DDEI (no LB opposite-target retry,
// best-effort version discovery, /polaris-ui/go for case launch). No secrets, no
// Azure AD, no Table Storage. Ported from global-components
// infra/proxy/config/global-components.polaris-non-ddei (the /polaris-non-ddei
// sideways entry is NOT brought across; the cookie arrives here as `cc`).
// Self-contained (no shared imports) so it deletes cleanly.
// ---------------------------------------------------------------------------

// Fallback when version discovery can't read the /CMS redirect. A refined
// production version must always discover (CMS bumps this on release).
const DEFAULT_CMS_VERSION = "CMS.24.0.01";

// Spoof MSIE7/Trident so the loopback fetch is treated as ie+ and falls through
// the proxy's ^/CMS.* IE-mode coercion (nonie+ would get a 402/302) to CMS itself.
const MSIE_UA =
  "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 10.0; WOW64; Trident/7.0; .NET4.0C; .NET4.0E)";

const GO_ROUTE = "/polaris-ui/go";
const FALLBACK_LANDING = "/polaris-ui/";

// DDEI Constants.AuthFailReason* values.
const FAIL_NO_COOKIES = "no-cookies";
const FAIL_CMS_AUTH = "cms-auth-not-valid";
const FAIL_CMS_MODERN = "cms-modern-auth-not-valid";
const FAIL_UNEXPECTED = "unexpected-error";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

// Read an already-decoded query arg (njs r.args handles the percent/+ decoding); "" if absent
// or REPEATED (njs gives an array for ?a=1&a=2: ambiguous, so it reads as missing — as drop2's).
function _arg(r, name) {
  const v = r.args[name];
  return typeof v === "string" ? v : "";
}

function _header(r, name) {
  const v = r.headersIn[name];
  return v !== undefined ? v : "";
}

// njs turns a relative r.return(302, "/x") into an absolute Location using nginx's OWN scheme,
// which behind the TLS-terminating front is http — an extra http->https hop on every redirect
// (DDEI's relative Location never had it). Same fix as auth-handover.js _redirectToAbsoluteUrl:
// resolve relative paths against the forwarded proto + Host; absolute URLs pass through.
function absoluteUrl(r, url) {
  if (url.lastIndexOf("http", 0) === 0) return url;
  return (_header(r, "X-Forwarded-Proto") || "https") + "://" + _header(r, "Host") + url;
}

function _clientIp(r) {
  const xff = _header(r, "X-Forwarded-For");
  return xff ? xff.split(",")[0].trim() : "0.0.0.0";
}

// A v4 GUID for the session correlation id. Not a security token, but we source it from
// Web Crypto (crypto.getRandomValues, present in njs 0.8.5) rather than Math.random so no
// insecure-PRNG lint (SonarQube "make sure this pseudorandom generator is safe") fires here.
function _uuid() {
  const b = new Uint8Array(16);
  crypto.getRandomValues(b);
  b[6] = (b[6] & 0x0f) | 0x40; // version 4
  b[8] = (b[8] & 0x3f) | 0x80; // variant 10xx
  const h = Array.from(b).map((x) => x.toString(16).padStart(2, "0"));
  return (
    h.slice(0, 4).join("") +
    "-" +
    h.slice(4, 6).join("") +
    "-" +
    h.slice(6, 8).join("") +
    "-" +
    h.slice(8, 10).join("") +
    "-" +
    h.slice(10, 16).join("")
  );
}

// Best-effort CMS version discovery: GET /CMS via the proxy's own routing and
// read the versioned path from the redirect (Location, or the followed URL).
async function _discoverCmsVersion(host, cookieHeader) {
  try {
    const resp = await ngx.fetch("https://" + host + "/CMS", {
      method: "GET",
      headers: { "User-Agent": MSIE_UA, Host: host, Cookie: cookieHeader },
    });
    const loc = resp.headers.get("Location");
    if (loc) {
      const m = loc.match(/\/(CMS\.[^/]+)/);
      if (m) {
        return m[1];
      }
    }
    // Some njs builds auto-follow the 302; then the final URL carries the version.
    const finalUrl = resp.url;
    if (finalUrl) {
      const m2 = finalUrl.match(/\/(CMS\.[^/]+)/);
      if (m2) {
        return m2[1];
      }
    }
  } catch (e) {
    // fall through to the default
  }
  return DEFAULT_CMS_VERSION;
}

// Mint the CMS modern session token: scrape SESS_MODERN_USER_SESSION_ID from
// {version}/Includes/uainGeneratedScript.aspx. Empty token => mint failed.
async function _mintModernToken(host, cookieHeader) {
  const versionId = await _discoverCmsVersion(host, cookieHeader);
  const url =
    "https://" + host + "/" + versionId + "/Includes/uainGeneratedScript.aspx";
  const resp = await ngx.fetch(url, {
    method: "GET",
    headers: { "User-Agent": MSIE_UA, Host: host, Cookie: cookieHeader },
  });
  const body = await resp.text();
  const m = body.match(
    /SESS_MODERN_USER_SESSION_ID\s*=\s*['"]([0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12})['"]/,
  );
  return { token: m && m[1] ? m[1] : "", versionId };
}

// Verify the modern token is a live Modern session (DDEI VerifyCmsModernToken).
// A 200 body carrying a non-empty errors[] is a failure, not a pass.
async function _verifyModernToken(host, token, cookieHeader) {
  const resp = await ngx.fetch("https://" + host + "/graphql/", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "cms-api-version": "1",
      "cms-api-sessionid": token,
      Cookie: cookieHeader,
      Host: host,
    },
    body: JSON.stringify({
      query: "query getUser($guid: UUID!) { user(guid: $guid) { partyId } }",
      operationName: "getUser",
      variables: { guid: token },
    }),
  });
  if (!resp.ok) {
    return false;
  }
  const text = await resp.text();
  try {
    const data = JSON.parse(text);
    if (data.errors && data.errors.length > 0) {
      return false;
    }
    return !!(data.data && data.data.user);
  } catch (e) {
    return false;
  }
}

// The Cms-Auth-Values cookie the Polaris gateway reads. Same camelCase JSON as
// DDEI's CmsAuthValuesDto (preferredLoadBalancerTarget omitted — no retry here).
// ASP.NET Core percent-encodes cookie values; encodeURIComponent mirrors that
// (the JSON's cookies field contains ';', which must not break cookie parsing).
function _buildCmsAuthValuesCookie(dto, secure) {
  const value = encodeURIComponent(JSON.stringify(dto));
  return (
    "Cms-Auth-Values=" +
    value +
    "; Path=/api/; HttpOnly" +
    (secure ? "; Secure" : "") +
    "; SameSite=Lax"
  );
}

// DDEI BuildFailureRedirectUrl: append auth-fail-reason to the polaris-ui-url
// (fallback landing when there's no ui url, e.g. a CMS launch).
function _failRedirect(r, polarisUiUrl, reason) {
  const base = polarisUiUrl || FALLBACK_LANDING;
  const delim = base.includes("?") ? "&" : "?";
  r.return(302, absoluteUrl(r, base + delim + "auth-fail-reason=" + reason));
}

function _extractCaseId(q) {
  const m = q.match(/"?caseId"?\s*:\s*"?(\d+)/i);
  return m ? m[1] : "";
}

// ---------------------------------------------------------------------------
// Pipeline, in three steps:
//
//   captureLanding      -> the post-auth redirect intent from the request
//   establishCmsSession -> the token + version /init passed on, or lookupCmsSession
//                          (discover -> mint -> verify, the CMS half)
//   finalize            -> set Cms-Auth-Values + redirect to the landing
//
// handleInitNonDdei composes all three. drop2 (which runs from /init, R0) calls only
// lookupCmsSession — the non-redirecting lookup — and then passes the token on to
// /auth-refresh-inbound, so this path skips its own lookup. drop1 imports nothing from drop2,
// so removing drop2 leaves this file working unchanged.
// ---------------------------------------------------------------------------

// The post-auth redirect intent, captured from the request args.
function captureLanding(r) {
  return { polarisUiUrl: _arg(r, "polaris-ui-url"), q: _arg(r, "q") };
}

// Look up the CMS session for a cookie jar: discover the version -> mint the modern token ->
// verify it against CMS Modern. NON-REDIRECTING: returns { token, versionId } on success, or
// { token: "", versionId: "", reason } (a DDEI auth-fail-reason) — never throws. Used by
// establishCmsSession below and by /init (drop2), where a failure must carry on, not redirect.
async function lookupCmsSession(host, cookieHeader) {
  let mint;
  try {
    mint = await _mintModernToken(host, cookieHeader);
  } catch (e) {
    return { token: "", versionId: "", reason: FAIL_CMS_AUTH };
  }
  if (!mint.token) {
    return { token: "", versionId: "", reason: FAIL_CMS_AUTH };
  }

  let verified = false;
  try {
    verified = await _verifyModernToken(host, mint.token, cookieHeader);
  } catch (e) {
    verified = false;
  }
  if (!verified) {
    return { token: "", versionId: "", reason: FAIL_CMS_MODERN };
  }
  return { token: mint.token, versionId: mint.versionId, reason: "" };
}

// /init has usually already looked the session up (for the Entra deposit) and passes the token +
// version on (R0), so this step need not repeat discover/mint/verify. Only well-formed values
// are taken (a GUID token, a CMS.<n>... version — the version lands in the Cms-Auth-Values JSON);
// anything else is ignored and the session is looked up as before. Trust: these arrive beside
// `cc` in the same URL, which a caller could already hand-craft; a mismatched pair fails at the
// gateway and triggers a reauth, as a bad `cc` does today.
const TOKEN_ARG = "cms-modern-token";
const VERSION_ARG = "cms-version-id";
function _passedSession(r) {
  const token = _arg(r, TOKEN_ARG);
  const versionId = _arg(r, VERSION_ARG);
  if (!/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(token)) return null;
  if (!/^CMS\.[0-9A-Za-z._-]+$/.test(versionId)) return null;
  return { token: token, versionId: versionId };
}

// Establish the CMS session: cookies present -> the token + version passed on from /init, or
// look them up (lookupCmsSession). On any handled failure it performs the fail-redirect (using
// landing.polarisUiUrl) and returns null; on success returns
// { cookieHeader, token, versionId }. The CMS cookie arrives as `cc`
// (appAuthRedirect appended &cc=<cookie> on the 302 to /auth-refresh-inbound),
// already whitelisted and carrying WindowID=MASTER, so it is used as the Cookie
// header as-is. A jar with no CMS cookies arrives as an empty `cc`.
async function establishCmsSession(r, landing) {
  const host = _header(r, "Host");

  const cookieHeader = _arg(r, "cc");
  if (!cookieHeader) {
    _failRedirect(r, landing.polarisUiUrl, FAIL_NO_COOKIES);
    return null;
  }

  const passed = _passedSession(r);
  if (passed) {
    return { cookieHeader: cookieHeader, token: passed.token, versionId: passed.versionId };
  }

  const found = await lookupCmsSession(host, cookieHeader);
  if (!found.token) {
    _failRedirect(r, landing.polarisUiUrl, found.reason);
    return null;
  }
  return {
    cookieHeader: cookieHeader,
    token: found.token,
    versionId: found.versionId,
  };
}

// Build the Cms-Auth-Values Set-Cookie string for an established session (the
// gateway reads this). Session shape matches establishCmsSession's return.
function cmsAuthValuesCookie(r, session) {
  const secure = (_header(r, "X-Forwarded-Proto") || "https") === "https";
  const dto = {
    cookies: session.cookieHeader,
    userIpAddress: _clientIp(r),
    token: session.token,
    sessionCorrelationId: _uuid(),
    sessionCreatedTime: new Date().toISOString(),
    cmsVersionId: session.versionId,
  };
  return _buildCmsAuthValuesCookie(dto, secure);
}

// Finalize: set Cms-Auth-Values (appended to any cookies already queued on the
// response) + redirect by auth-flow mode.
function finalize(r, session, landing) {
  const queued = r.headersOut["Set-Cookie"] || [];
  r.headersOut["Set-Cookie"] = queued.concat([cmsAuthValuesCookie(r, session)]);

  if (landing.polarisUiUrl) {
    // PolarisAuthRedirect: UI passed the post-auth return URL.
    // NOTE: a production version must whitelist this (open-redirect surface) —
    // see docs/PLAN.md Phase 4 / the /auth-refresh-inbound switch discussion.
    r.return(302, absoluteUrl(r, landing.polarisUiUrl));
    return;
  }
  // CmsLaunch: q = {"caseId":n}. Let the UI resolve the URN via /polaris-ui/go.
  const caseId = _extractCaseId(landing.q);
  if (caseId) {
    r.return(
      302,
      absoluteUrl(r, GO_ROUTE + "?ctx=" + encodeURIComponent('{"caseId":' + caseId + "}")),
    );
    return;
  }
  r.return(302, absoluteUrl(r, FALLBACK_LANDING));
}

// ---------------------------------------------------------------------------
// Handler — the DDEI /api/init/ pipeline in njs (drop1's own switch path).
// ---------------------------------------------------------------------------

// Observable marker: which auth-init path answered. DDEI's proxied answer carries none,
// so presence/absence in DevTools (or a curl -I) shows whether a switch or enrolment took
// effect. Values: "non-ddei" (drop1, on /auth-refresh-inbound), "entra" / "entra-degraded"
// (drop2, set by that module on /init and /init/entra-callback). Deliberately carries no
// error detail.
const AUTH_INIT_HEADER = "X-Polaris-Auth-Init";
function markAuthInit(r, value) {
  r.headersOut[AUTH_INIT_HEADER] = value;
}

async function handleInitNonDdei(r) {
  markAuthInit(r, "non-ddei"); // on every answer from this path, success or fail-redirect
  const landing = captureLanding(r);
  try {
    const session = await establishCmsSession(r, landing);
    if (!session) return; // establishCmsSession already fail-redirected
    finalize(r, session, landing);
  } catch (e) {
    // DDEI wraps the whole handler: any unexpected throw -> auth-fail-reason.
    _failRedirect(r, landing.polarisUiUrl, FAIL_UNEXPECTED);
  }
}

// ---------------------------------------------------------------------------
// The switch, and the rest of the auth handover DDEI answered. With drop1 on, the proxy
// makes no auth-handover call to DDEI: /auth-refresh-inbound (above), /auth-refresh-termination,
// /auth-refresh-cms-modern-token and dev-login (feature dev-login, which reuses this module)
// all answer here. Every gate in the confs reads `enabled` (js_set $non_ddei).
// ---------------------------------------------------------------------------

// The global switches, or this browser's enrolment (auth-handover's /auth-refresh-enrol;
// enrolling in drop2 means "Entra at /init + drop1 behind"). Read at REQUEST time from
// process.env, so a MISSING app setting reads as "off" rather than wedging nginx at boot
// (a conf-side `${...}` token envsubst left literal did). Only the literal "true" enables.
// Returns the string "true"/"false" the confs' `if ($non_ddei = "true")` compares.
const ENROL_COOKIE_RE = /(?:^|;\s*)polaris_auth_handover=(drop1|drop2)(?:;|$)/;
function enabled(r) {
  const on =
    process.env.NON_DDEI_INIT_ENABLED === "true" ||
    process.env.ENTRA_STORE_ENABLED === "true" ||
    ENROL_COOKIE_RE.test(_header(r, "Cookie"));
  return on ? "true" : "false";
}

// /auth-refresh-termination — DDEI InSituReauthTermination. The last hop of the Polaris UI's
// in-situ reauth (its polaris-ui-url): JSONP telling the waiting page how it went,
//   <cb>({isSuccess: <no auth-fail-reason>, failReason: "<reason>" })
// byte-for-byte DDEI's shape. Hardened where DDEI echoed its input into script: the callback
// must be a plain identifier (the UI sends jsonp_<n>_<n>) and the reason is a JSON string.
const JSONP_CALLBACK_RE = /^[A-Za-z_$][A-Za-z0-9_$.]{0,99}$/;
function handleTerminationNonDdei(r) {
  markAuthInit(r, "non-ddei");
  const cb = _arg(r, "cb");
  if (!JSONP_CALLBACK_RE.test(cb)) {
    r.headersOut["Content-Type"] = "text/plain; charset=utf-8";
    r.return(400, "cb must be a JavaScript identifier");
    return;
  }
  const failReason = _arg(r, "auth-fail-reason");
  r.headersOut["Content-Type"] = "text/javascript; charset=utf-8";
  r.return(
    200,
    cb + "({isSuccess: " + !failReason.trim() + ", failReason: " + JSON.stringify(failReason) + " })",
  );
}

// /auth-refresh-cms-modern-token — DDEI GetModernToken. global-components' OutSystems handover
// bounces here when /init passed no token on (drop2 off or skipped; with drop2 the token rides
// the handover URL and this is never called). Takes r + cc, looks the CMS session up, and 302s
// to r with every incoming arg except r, plus cms-modern-token (DDEI's QueryHelpers.AddQueryString:
// appended before any #fragment). Like DDEI, r is not whitelisted: the caller supplies both the
// destination and the cookies, so nothing of anyone else's is handed on.
function _addQuery(url, pairs) {
  const hash = url.indexOf("#");
  const base = hash === -1 ? url : url.substring(0, hash);
  const frag = hash === -1 ? "" : url.substring(hash);
  const query = pairs
    .map((p) => encodeURIComponent(p[0]) + "=" + encodeURIComponent(p[1]))
    .join("&");
  return base + (base.includes("?") ? "&" : "?") + query + frag;
}

async function handleCmsModernTokenNonDdei(r) {
  markAuthInit(r, "non-ddei");
  const bad = (msg) => {
    r.headersOut["Content-Type"] = "text/plain; charset=utf-8";
    r.return(400, msg);
  };
  const dest = _arg(r, "r");
  const cc = _arg(r, "cc");
  if (!dest) return bad("Missing required 'r' parameter");
  if (!cc) return bad("Missing required 'cc' parameter");
  if (_arg(r, TOKEN_ARG)) {
    return bad("The 'cms-modern-token' parameter should not be in the request.  Do you have an unintended loop?");
  }

  const found = await lookupCmsSession(_header(r, "Host"), cc);
  if (!found.token) {
    // DDEI answered 500 with the exception; the reason alone is enough to act on.
    r.headersOut["Content-Type"] = "text/plain; charset=utf-8";
    r.return(500, "Could not get the CMS modern token: " + found.reason);
    return;
  }

  const pairs = [];
  Object.keys(r.args).forEach((k) => {
    if (k === "r") return;
    [].concat(r.args[k]).forEach((v) => pairs.push([k, v]));
  });
  pairs.push([TOKEN_ARG, found.token]);
  r.return(302, absoluteUrl(r, _addQuery(dest, pairs)));
}

export default {
  handleInitNonDdei,
  handleTerminationNonDdei,
  handleCmsModernTokenNonDdei,
  enabled,
  // reused by feature auth-handover.drop2.entra and feature dev-login:
  DEFAULT_CMS_VERSION,
  MSIE_UA,
  lookupCmsSession,
  cmsAuthValuesCookie,
  markAuthInit,
  absoluteUrl,
};
