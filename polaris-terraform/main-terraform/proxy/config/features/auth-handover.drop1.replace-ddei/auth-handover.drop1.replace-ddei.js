// ---------------------------------------------------------------------------
// Non-DDEI init — DDEI's /api/init/ reimplemented entirely in njs (switch-gated).
//
// Reached ONLY when NON_DDEI_INIT_ENABLED=true, via an internal rewrite from
// /auth-refresh-inbound (the switch lives in auth-handover.conf). It runs the DDEI
// InitiateCookies pipeline in njs:
//   whitelist the CMS cookies -> mint the CMS modern token -> verify it via GraphQL
//   -> set the Cms-Auth-Values cookie -> redirect (polaris-ui-url, or /polaris-ui/go).
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

// Cookie-name roots kept from the incoming cookie blob (DDEI WhitelistedCookieNameRoots),
// plus the new F5 form (C-CIN3-LBsessioncookie / F-CIN3-...) matched by suffix —
// which folds in the production proxy's _shimBigIpCookies step.
const WHITELIST_ROOTS = [
  "ASP.NET_SessionId",
  "UID",
  "WindowID",
  "CMSUSER",
  ".CMSAUTH",
  "BIGipServer",
];

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
const FAIL_NO_CMSAUTH = "no-cmsauth-cookie";
const FAIL_CMS_AUTH = "cms-auth-not-valid";
const FAIL_CMS_MODERN = "cms-modern-auth-not-valid";
const FAIL_UNEXPECTED = "unexpected-error";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

// Read an already-decoded query arg (njs r.args handles the percent/+ decoding).
function _arg(r, name) {
  const v = r.args[name];
  return v !== undefined ? v : "";
}

function _header(r, name) {
  const v = r.headersIn[name];
  return v !== undefined ? v : "";
}

function _clientIp(r) {
  const xff = _header(r, "X-Forwarded-For");
  return xff ? xff.split(",")[0].trim() : "0.0.0.0";
}

// A v4-ish GUID for the session correlation id. Math.random is fine here — it's
// a correlation/audit id, not a security token.
function _uuid() {
  const hex = (n) => {
    let s = "";
    for (let i = 0; i < n; i++) {
      s += Math.floor(Math.random() * 16).toString(16);
    }
    return s;
  };
  const y = (8 + Math.floor(Math.random() * 4)).toString(16);
  return hex(8) + "-" + hex(4) + "-4" + hex(3) + "-" + y + hex(3) + "-" + hex(12);
}

function _isWhitelisted(tok) {
  const name = tok.split("=")[0];
  if (/LBsessioncookie$/i.test(name)) {
    return true;
  }
  return WHITELIST_ROOTS.some((root) => tok.indexOf(root) === 0);
}

// Faithful port of DDEI CookieHelpers.GetWhitelistedCookies: split on SPACE,
// keep tokens whose name starts with a whitelisted root, rejoin with SPACE,
// then append WindowID=MASTER if absent. Returns "" if nothing survives.
function _whitelistCookies(cc) {
  const kept = cc
    .split(" ")
    .filter((tok) => tok !== "" && _isWhitelisted(tok))
    .join(" ")
    .replace(/^\s+|\s+$/g, "");
  if (kept === "") {
    return "";
  }
  if (kept.indexOf("WindowID") === -1) {
    const delim = kept.charAt(kept.length - 1) === ";" ? "" : ";";
    return kept + delim + " WindowID=MASTER";
  }
  return kept;
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
  const m = body.match(/SESS_MODERN_USER_SESSION_ID\s*=\s*'([^']+)'/);
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
  const delim = base.indexOf("?") !== -1 ? "&" : "?";
  r.return(302, base + delim + "auth-fail-reason=" + reason);
}

function _extractCaseId(q) {
  const m = q.match(/"?caseId"?\s*:\s*"?(\d+)/i);
  return m ? m[1] : "";
}

// ---------------------------------------------------------------------------
// Pipeline, split into three reusable steps so the drop2 Entra-store feature can
// insert its OIDC round-trip between "establish" and "finalize":
//
//   captureLanding      -> the post-auth redirect intent from the /init request
//   establishCmsSession -> whitelist cookies -> mint -> verify (the CMS half)
//   finalize            -> set Cms-Auth-Values + redirect to the landing
//
// handleInitNonDdei composes all three (drop1's own switch path). drop2 calls
// establishCmsSession, carries the result through Azure AD in its state cookie, then
// calls cmsAuthValuesCookie/finalize in its callback — so the Cms-Auth-Values write
// stays IDENTICAL across both drops (see auth-handover.drop2.entra). drop1 imports
// nothing from drop2, so removing drop2 leaves this file working unchanged.
// ---------------------------------------------------------------------------

// The post-auth redirect intent, captured from the /init request args. Carried by
// value so drop2 can stash it in its state cookie and finalize on the AD callback
// (a different request that no longer has these args).
function captureLanding(r) {
  return { polarisUiUrl: _arg(r, "polaris-ui-url"), q: _arg(r, "q") };
}

// Establish the CMS session: cookies present -> whitelist (+WindowID=MASTER) ->
// mint the modern token -> verify against CMS Modern. On any handled failure it
// performs the fail-redirect (using landing.polarisUiUrl) and returns null; on
// success returns { cookieHeader, token, versionId }. The CMS cookie arrives as
// `cc` (appAuthRedirect appended &cc=<cookie> on the 302 to /auth-refresh-inbound).
async function establishCmsSession(r, landing) {
  const host = _header(r, "Host");

  const cc = _arg(r, "cc");
  if (!cc) {
    _failRedirect(r, landing.polarisUiUrl, FAIL_NO_COOKIES);
    return null;
  }

  const cookieHeader = _whitelistCookies(cc);
  if (!cookieHeader) {
    _failRedirect(r, landing.polarisUiUrl, FAIL_NO_CMSAUTH);
    return null;
  }

  let mint;
  try {
    mint = await _mintModernToken(host, cookieHeader);
  } catch (e) {
    _failRedirect(r, landing.polarisUiUrl, FAIL_CMS_AUTH);
    return null;
  }
  if (!mint.token) {
    _failRedirect(r, landing.polarisUiUrl, FAIL_CMS_AUTH);
    return null;
  }

  let verified = false;
  try {
    verified = await _verifyModernToken(host, mint.token, cookieHeader);
  } catch (e) {
    verified = false;
  }
  if (!verified) {
    _failRedirect(r, landing.polarisUiUrl, FAIL_CMS_MODERN);
    return null;
  }

  return { cookieHeader: cookieHeader, token: mint.token, versionId: mint.versionId };
}

// Build the Cms-Auth-Values Set-Cookie string for an established session (the
// gateway reads this). Session shape matches establishCmsSession's return. Kept
// separate from finalize so drop2's iframe path can set the cookie WITHOUT a redirect.
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
// response — drop2 queues its id-token cookie first) + redirect by auth-flow mode.
function finalize(r, session, landing) {
  const queued = r.headersOut["Set-Cookie"] || [];
  r.headersOut["Set-Cookie"] = queued.concat([cmsAuthValuesCookie(r, session)]);

  if (landing.polarisUiUrl) {
    // PolarisAuthRedirect: UI passed the post-auth return URL.
    // NOTE: a production version must whitelist this (open-redirect surface) —
    // see docs/PLAN.md Phase 4 / the /auth-refresh-inbound switch discussion.
    r.return(302, landing.polarisUiUrl);
    return;
  }
  // CmsLaunch: q = {"caseId":n}. Let the UI resolve the URN via /polaris-ui/go.
  const caseId = _extractCaseId(landing.q);
  if (caseId) {
    r.return(
      302,
      GO_ROUTE + "?ctx=" + encodeURIComponent('{"caseId":' + caseId + "}"),
    );
    return;
  }
  r.return(302, FALLBACK_LANDING);
}

// ---------------------------------------------------------------------------
// Handler — the DDEI /api/init/ pipeline in njs (drop1's own switch path).
// ---------------------------------------------------------------------------

async function handleInitNonDdei(r) {
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

export default {
  handleInitNonDdei,
  // reused by feature auth-handover.drop2.entra:
  captureLanding,
  establishCmsSession,
  cmsAuthValuesCookie,
  finalize,
};
