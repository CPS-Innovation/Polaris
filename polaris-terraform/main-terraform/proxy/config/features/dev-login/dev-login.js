// ---------------------------------------------------------------------------
// Dev login — DDEI's /api/login and /api/login-full-cookie (SimulatedCms, DevelopmentLogin)
// in njs, behind the drop1 switch: auth-handover.conf rewrites /dev-login/ and
// /api/dev-login-full-cookie/ here when $non_ddei is "true"; off, both proxy to DDEI as today.
//
//   GET   the login form (username, password, CMS environment); clears the CMS-env marker +
//         BIG-IP/LB cookies so a fresh login starts clean.
//   POST  log in to CMS as DDEI CmsService.GetCmsCookiesAsync did, via $host loopbacks that
//         re-enter the proxy's own ^/CMS.* routing (the DEVLOGIN cookie picks the CMS env, as
//         cmsenv.detect reads it):
//           1. GET /CMS  -> the CMS version (Location) + the load-balancer cookie
//           2. POST /<version>/User/uaulLogin.aspx with the credentials
//         then
//           /dev-login/                 hands the CMS cookies to the browser (+ __CMSENV) and
//                                       links into the apps;
//           /api/dev-login-full-cookie/ looks the modern token up (drop1) and sets the
//                                       gateway's Cms-Auth-Values cookie instead.
//
// Contract kept from DDEI's pages (polaris-e2e and shared.integration-tests drive them): form
// fields username / password / selected-environment (blank = default), an
// input[type=submit], `strong[data-testid='login-ok']` on success, and on the full-cookie
// POST Cms-Auth-Values is the FIRST Set-Cookie.
// Pre-prod tooling: the credentials go to CMS and nowhere else, and are never logged.
// ---------------------------------------------------------------------------
import cmsenv from "../common/cmsenv.js";
import replaceDdei from "../auth-handover.drop1.replace-ddei/auth-handover.drop1.replace-ddei.js";
import appLaunch from "../app-launch/app-launch.js";

// The cookies a GET clears: the CMS-env marker plus, per CMS env, both BIG-IP pool cookies
// (ACP/AFP) and both load-balancer session cookies (C/F). Generated to a flat
// "<name>=deleted; ...expired" list — the old config hand-wrote all seventeen add_header lines
// twice. Also used by auth-handover's devLogin header filter (the DDEI path).
const EXPIRE = "=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
const CLEARED_COOKIES = (function () {
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

// The environments the form offers beyond the default: the CIN envs this proxy has an upstream
// for (DDEI's OtherEnvs setting, now derived). The value is what the DEVLOGIN cookie carries.
const OTHER_ENVS = ["cin2", "cin4", "cin5"];
function _otherEnvs() {
  return OTHER_ENVS.filter(
    (e) => !!process.env[e.toUpperCase() + "_UPSTREAM_CMS_DOMAIN_NAME"],
  );
}

// The load-balancer cookies: the BIG-IP pool form and the F5 C-/F-<ENV>-LBsessioncookie form.
const LB_COOKIE_RE = /^(?:BIGipServer[^=]*|[CF]-[A-Za-z0-9]+-LBsessioncookie)=/;
const LOGIN_PATH = "/User/uaulLogin.aspx";

// The apps the result page links to: the P button's /polaris (Casework app), the M button's
// materials-ui (without a case), and the C button's casework home via /launch.
const POLARIS_LINK = "/polaris";
const MATERIALS_LINK = "/polaris?polaris-ui-url=/materials-ui";

function _html(s) {
  return String(s)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

// An application/x-www-form-urlencoded body -> { name: value } (first value wins).
function _form(body) {
  const out = {};
  (body || "").split("&").forEach((pair) => {
    if (!pair) return;
    const eq = pair.indexOf("=");
    const k = eq === -1 ? pair : pair.substring(0, eq);
    const v = eq === -1 ? "" : pair.substring(eq + 1);
    try {
      const key = decodeURIComponent(k.replace(/\+/g, " "));
      if (!(key in out)) out[key] = decodeURIComponent(v.replace(/\+/g, " "));
    } catch (e) {
      // a malformed %-sequence: skip that field, as a missing one
    }
  });
  return out;
}

function _setCookies(resp) {
  const h = resp.headers;
  return h.getAll ? h.getAll("Set-Cookie") : [].concat(h.get("Set-Cookie") || []);
}

// Log in to CMS (DDEI GetCmsCookiesAsync). Returns { setCookies, ok }: every Set-Cookie the login
// answered (with attributes, for the browser), plus the /CMS load-balancer cookie when the login
// set no newer one; ok = a .CMSAUTH cookie came back. Throws on a transport error.
async function _cmsLogin(host, username, password, env) {
  const envCookie = "DEVLOGIN=" + env;
  const home = await ngx.fetch("https://" + host + "/CMS", {
    method: "GET",
    headers: { "User-Agent": replaceDdei.MSIE_UA, Host: host, Cookie: envCookie },
  });
  const loc = home.headers.get("Location") || "";
  const m = loc.match(/\/(CMS\.[^/]+)/);
  const versionId = m ? m[1] : replaceDdei.DEFAULT_CMS_VERSION;
  const lb = _setCookies(home).filter((c) => LB_COOKIE_RE.test(c));

  const resp = await ngx.fetch("https://" + host + "/" + versionId + LOGIN_PATH, {
    method: "POST",
    headers: {
      "User-Agent": replaceDdei.MSIE_UA,
      Host: host,
      "Content-Type": "application/x-www-form-urlencoded",
      Cookie: lb.map((c) => c.split(";")[0]).concat([envCookie]).join("; "),
    },
    body:
      "txtYLoginName=" + encodeURIComponent(username) +
      "&txtNPassword=" + encodeURIComponent(password) +
      "&hidIsPostBack=1&hidNActXSpellCheckExists=false",
  });
  let setCookies = _setCookies(resp);
  if (!setCookies.some((c) => LB_COOKIE_RE.test(c))) setCookies = setCookies.concat(lb);
  return {
    setCookies: setCookies,
    ok: resp.status < 400 && setCookies.some((c) => c.indexOf(".CMSAUTH") === 0),
  };
}

function _page(title, body) {
  return (
    '<!DOCTYPE html><html lang="en"><head><meta charset="utf-8">' +
    '<meta name="viewport" content="width=device-width, initial-scale=1">' +
    "<title>" + _html(title) + "</title>" +
    "<style>body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;max-width:640px;margin:40px auto;padding:0 20px}" +
    "label{display:block;margin:12px 0 4px}input,select{width:100%;font-size:1.1rem;padding:8px;box-sizing:border-box}" +
    "input[type=submit]{margin-top:16px;width:auto;padding:8px 24px}li{margin:8px 0}" +
    ".ok{color:#00703c}.not-ok{color:#d4351c}pre{background:#f0f0f0;padding:8px;overflow-x:auto;font-size:.8rem}</style>" +
    "</head><body>" + body + "</body></html>"
  );
}

function _formPage(heading) {
  const def = process.env.DEFAULT_UPSTREAM_CMS_DOMAIN_NAME || "default";
  const options =
    '<option value="" selected>Default - ' + _html(def) + "</option>" +
    _otherEnvs().map((e) => '<option value="' + e + '">' + e + "</option>").join("");
  return _page(
    heading,
    "<h1>" + _html(heading) + "</h1>" +
      '<form method="post">' +
      '<label for="username">User name</label><input id="username" name="username" autocomplete="username">' +
      '<label for="password">Password</label><input id="password" name="password" type="password" autocomplete="current-password">' +
      '<label for="env">CMS environment</label><select id="env" name="selected-environment">' + options + "</select>" +
      '<input type="submit" value="Log in">' +
      "</form>",
  );
}

function _loggedInLine(username, ok) {
  return (
    "<p>Hi " + _html(username) + ", you " +
    (ok
      ? "<strong class=\"ok\" data-testid=\"login-ok\">ARE</strong>"
      : "<strong class=\"not-ok\">ARE NOT</strong>") +
    " logged in to CMS.</p>"
  );
}

function _appLinks(r) {
  const key = appLaunch.proxyLaunchKey(r.headersIn.Host || "");
  const link = (href, text) => '<li><a href="' + _html(href) + '" target="_blank">' + text + "</a></li>";
  return (
    "<ul>" +
    link(POLARIS_LINK, "Casework app (P button)") +
    link(MATERIALS_LINK, "Materials app (M button)") +
    (key ? link("/launch/" + key, "Home page (C button)") : "") +
    "</ul>"
  );
}

function _respond(r, status, cookies, html) {
  if (cookies.length) r.headersOut["Set-Cookie"] = cookies;
  r.headersOut["Content-Type"] = "text/html; charset=utf-8";
  r.headersOut["Cache-Control"] = "no-store";
  r.return(status, html);
}

// The POST's credentials + env, or null (then a 400). Only the offered envs are accepted, so
// the DEVLOGIN cookie never carries an arbitrary value.
function _credentials(r) {
  const f = _form(r.requestText);
  const env = f["selected-environment"] || "";
  if (!f.username || (env && !_otherEnvs().includes(env))) return null;
  return { username: f.username, password: f.password || "", env: env };
}

async function _handle(r, heading, onLoggedIn) {
  if (r.method === "GET") {
    _respond(r, 200, CLEARED_COOKIES, _formPage(heading));
    return;
  }
  if (r.method !== "POST") {
    r.return(405);
    return;
  }
  const c = _credentials(r);
  if (!c) {
    _respond(r, 400, [], _page(heading, "<p>A user name (and a listed CMS environment) is required.</p>"));
    return;
  }
  let login;
  try {
    login = await _cmsLogin(r.headersIn.Host, c.username, c.password, c.env);
  } catch (e) {
    _respond(r, 502, [], _page(heading, "<p>Could not reach CMS.</p>"));
    return;
  }
  await onLoggedIn(c, login);
}

// /dev-login/ — the browser gets the CMS cookies (and __CMSENV, so the Open Modern link works)
// and links into the apps.
async function devLogin(r) {
  const heading = "Polaris developer login";
  await _handle(r, heading, async (c, login) => {
    const marker = "__CMSENV=" + cmsenv.detect("DEVLOGIN=" + c.env) + "; path=/";
    // In full, as DDEI's page did: the Cookie header (paste into a tool / `cc`) and each Set-Cookie.
    const cookieHeader = login.setCookies.map((x) => x.split(";")[0]).join("; ");
    _respond(
      r,
      200,
      login.setCookies.concat([marker]),
      _page(
        heading,
        _loggedInLine(c.username, login.ok) +
          (login.ok ? _appLinks(r) : "") +
          "<p>Cookie header:</p><pre>" + _html(cookieHeader) + "</pre>" +
          "<p>Set-Cookie (as CMS sent them):</p><pre>" + _html(login.setCookies.join("\n")) + "</pre>",
      ),
    );
  });
}

// /api/dev-login-full-cookie/ — the gateway's Cms-Auth-Values (Path=/api/) for the CMS session,
// FIRST of the Set-Cookies (shared.integration-tests reads the first), then __CMSENV.
async function devLoginFullCookie(r) {
  const heading = "Polaris developer login (full cookie)";
  await _handle(r, heading, async (c, login) => {
    if (!login.ok) {
      _respond(r, 401, [], _page(heading, _loggedInLine(c.username, false)));
      return;
    }
    const cookieHeader = login.setCookies.map((x) => x.split(";")[0]).join("; ");
    const found = await replaceDdei.lookupCmsSession(r.headersIn.Host, cookieHeader);
    if (!found.token) {
      _respond(r, 502, [], _page(heading, "<p>Logged in, but no CMS modern token: " + _html(found.reason) + "</p>"));
      return;
    }
    const authValues = replaceDdei.cmsAuthValuesCookie(r, {
      cookieHeader: cookieHeader,
      token: found.token,
      versionId: found.versionId,
    });
    const marker = "__CMSENV=" + cmsenv.detect("DEVLOGIN=" + c.env) + "; path=/";
    _respond(
      r,
      200,
      [authValues, marker],
      _page(
        heading,
        _loggedInLine(c.username, true) +
          '<p><a href="/viewer/landing#/dashboard/' + _html(found.token) + '" target="_blank">Open Modern (new tab)</a></p>' +
          "<p>Cms-Auth-Values is set for <code>/api/</code>.</p>",
      ),
    );
  });
}

export default { devLogin, devLoginFullCookie, CLEARED_COOKIES };
