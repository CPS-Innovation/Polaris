// ---------------------------------------------------------------------------
// Entra store (drop2) — the Azure AD half of the auth handover (switch-gated).
//
// Runs in /init (R0, 2026-10-16): the one door EVERY app's CMS handover passes through, so every
// user's CMS auth is deposited to MDS whatever the destination (Polaris, OutSystems, LCC, …) —
// without changing any destination's contract. Reached ONLY when ENTRA_STORE_ENABLED=true or the
// browser is enrolled (polaris_auth_handover=drop2), AND drop2 is fully configured. Two entries:
//
//   beginFromInit            (called by auth-handover appAuthRedirect, after its whitelist check
//                             and cookie filter)
//     drop1.lookupCmsSession (discover + mint + verify the modern token, non-redirecting)
//     -> pack {r, cc, token, version, terminal} into a signed first-party state cookie + a
//        random anti-CSRF handle in the OAuth `state` param
//     -> 302 to Entra /authorize (prompt=none, silent): openid profile offline_access.
//     Returns false WITHOUT responding on any failure, so /init carries on exactly as today.
//
//   handleInitEntraCallback  (/init/entra-callback, public — AD 302s the browser here)
//     validate state -> exchange code -> REFRESH TOKEN (the access token it comes with is a
//        Microsoft Graph token, unused)
//     -> IFRAME only: refresh swap -> watchdog token (ENTRA_WATCHDOG_SCOPE) -> presence token
//        cookie on the two presence paths, Max-Age = that token's lifetime ("" scope = none)
//     -> refresh swap -> MDS token (store.scope)
//     -> store.deposit({cookies, token}, mdsToken)   (MDS PUT /cms-auth-store)
//     -> carry on as /init would: TOP-LEVEL 302 to r with cc (+ the CMS token for our own
//        /auth-refresh-inbound and OS handover pages, so they skip their own lookup);
//        IFRAME 200 static terminal page (harness tears the iframe down on its onload).
//
// TOKEN MODEL (PLAN-entra-unified-flow; decided 2026-10-15): ONE silent, NEUTRAL sign-in
// (openid profile offline_access — no API scope, so drop2 needs our app reg to expose none), then a
// refresh-token swap per service: a confidential client's refresh token buys an access token for
// any API the app may call (consent = that API pre-authorising our client). No on-behalf-of: that
// needs a token addressed to us, i.e. an exposed scope. The refresh token never leaves this
// callback (no cookie, no log); the browser only ever holds the watchdog token (iframe). No
// id_token is used: each backend identifies the user from its own token (oid).
//
// TRANSPARENT / ADDITIVE / BEST-EFFORT: any Entra or store failure carries on exactly as /init
// would have (top-level: 302 to r with cc; iframe: still render the terminal, keeping any presence
// cookie already obtained) — every team's handover runs through here, so the store write never
// breaks anyone's login, and never costs presence its token.
//
// SECURITY NOTE (deviates from the plan's "carry state in the OAuth state param"
// assumption): the CMS cookies + modern token are SESSION SECRETS, so they are kept in
// a first-party HttpOnly state cookie and only a random handle travels in the `state`
// param — putting them in `state` would leak them to Microsoft (query string, logs,
// referer). CONFIDENTIALITY, though, is not enough: the cookie is also HMAC-signed
// (STATE_HMAC_SECRET) so it can't be FORGED via cookie-tossing from a sibling cps.gov.uk
// subdomain — without that, the `state`-param check is circular. See QUIRKS.md E1.
//
// ISOLATED / NEW-GEN: NOT part of the golden-master before/after; dormant with the
// switch off. Imports drop1 (lookupCmsSession, markAuthInit, absoluteUrl) + store.js (the
// deposit seam); drop1 imports nothing back. auth-handover imports this module for /init.
// ---------------------------------------------------------------------------

import replaceDdei from "../auth-handover.drop1.replace-ddei/auth-handover.drop1.replace-ddei.js";
import store from "./store.js";
import cryptoModule from "crypto"; // njs built-in — for the state-cookie HMAC

// Azure AD app registration — config comes ONLY from app settings; NO baked defaults (see
// APP-SETTINGS.md). Missing => "" => isConfigured() is false and /init never starts the flow.
// tenant/client are non-secret ids; the client SECRET is a secret. There is NO ENTRA_REDIRECT_URI
// setting: the redirect_uri is derived per-request from the incoming Host (see _redirectUri) — the
// flow always returns to the same host, which must be a redirect URI registered on the app reg.
const TENANT_ID = process.env.ENTRA_TENANT_ID || "";
const CLIENT_ID = process.env.ENTRA_CLIENT_ID || "";
const CLIENT_SECRET = process.env.ENTRA_CLIENT_SECRET || "";
// Callback path — must match the `location = /init/entra-callback` in the .conf (an exact match,
// so the prefix `location /init` cannot catch it). Registered as a redirect URI per host.
const CALLBACK_PATH = "/init/entra-callback";

// Server-side secret that signs the state cookie (integrity). EMPTY => fail closed: _unpackState
// rejects every cookie (and isConfigured() stops /init from starting a flow at all). See
// APP-SETTINGS.md and QUIRKS E1.
const STATE_HMAC_SECRET = process.env.ENTRA_STATE_HMAC_SECRET || "";

// First-party state cookie: holds the (secret) session payload across the AD hop. ONE PER FLOW,
// named by the flow's random handle (entra_auth_state_<handle>), so two handovers in flight in one
// browser (an OS link and the P button in two tabs) never read or clear each other's state. Path
// scopes it to the callback (never every /init request); short-lived; HttpOnly so no script can
// read it. (Several at once are small: nginx allows 4 x 32k request-header buffers.)
const STATE_COOKIE_PREFIX = "entra_auth_state_";
const STATE_SET_OPTS =
  "; Path=" + CALLBACK_PATH + "; HttpOnly; Secure; SameSite=Lax; Max-Age=300";
const STATE_CLEAR_OPTS =
  "; Path=" + CALLBACK_PATH + "; HttpOnly; Secure; SameSite=Lax; Max-Age=0";

// Where the callback lands when the state (and so the destination) is unrecoverable.
const FALLBACK_LANDING = "/polaris-ui/";

// Server-side lifetime of a state cookie. The browser's Max-Age=300 alone would let a captured,
// validly signed cookie be replayed (cookie-tossed) indefinitely, so the signed payload carries its
// issue time (iat, seconds) and host (h), checked on the callback. Small forward skew allowed.
const STATE_MAX_AGE_S = 300;
const STATE_SKEW_S = 60;

// The OAuth `state` param: "<mode>.<handle>", mode "t" (top-level) or "i" (hidden iframe). The
// handle names the flow's cookie; the mode is not secret, and Entra echoes `state` on errors too,
// so a callback whose cookie is gone still knows to end the iframe on the terminal page.
const STATE_PARAM_RE = /^([ti])\.([0-9a-f]{32})$/;

// D2: browsers silently DROP a cookie over ~4 KB (name + value), which would lose the state —
// and with it the destination — at the callback. The state holds r + cc + token, and OS `r` URLs
// are long, so /init checks the packed size and skips Entra (carrying on as today) above this.
const MAX_STATE_COOKIE = 3800;

// The iframe terminal: a bare page whose sole job is to fire `onload` so the harness
// that opened the hidden iframe can destroy it. No script, no data.
const TERMINAL_HTML =
  '<!DOCTYPE html><html lang="en"><head><meta charset="utf-8">' +
  "<title>CMS auth captured</title></head>" +
  '<body data-cms-auth="done"><!-- entra store populated --></body></html>';

// The NEUTRAL sign-in scope: identity + a refresh token, no API. Requested at BOTH /authorize and
// the code exchange: a code is bound to the scopes consented at authorize (AADSTS70011 otherwise).
const SIGN_IN_SCOPE = "openid profile offline_access";

// The watchdog (presence API) scope the refresh token is swapped for, for the presence cookie.
// "" => no presence cookie. Interim pre-prod value: a scope 8d6133af happens to expose (unused by
// drop2's own sign-in), which the
// watchdog accepts today; becomes the watchdog's own app reg scope by config alone (terraform
// proxy_entra.watchdog_scope).
const WATCHDOG_SCOPE = process.env.ENTRA_WATCHDOG_SCOPE || "";

// Presence: the watchdog token, handed to global-components' presence transports in a cookie they
// already read (contract with global-components: name + paths). HttpOnly (no page JS), Secure,
// path-scoped so it rides ONLY the two presence routes; the proxy lifts it into a Bearer upstream.
// Max-Age = the token's own lifetime (expires_in), so the cookie never outlives the token.
// Set on the IFRAME terminal only (the iframe runs in the CMS shell's IE-mode cookie jar, which is
// where the presence calls are made).
const PRESENCE_COOKIE = "cms-auth-presence-token";
const PRESENCE_PATHS = [
  "/global-components/presence-jsonp",
  "/global-components/case-locking",
];

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function _arg(r, name) {
  const v = r.args[name];
  return v !== undefined ? v : "";
}

function _header(r, name) {
  const v = r.headersIn[name];
  return v !== undefined ? v : "";
}

function _cookie(r, name) {
  const raw = r.headersIn["Cookie"];
  if (!raw) return "";
  const m = raw.match(new RegExp("(?:^|;\\s*)" + name + "=([^;]*)"));
  return m ? m[1] : "";
}

function _authorizeUrl() {
  return (
    "https://login.microsoftonline.com/" + TENANT_ID + "/oauth2/v2.0/authorize"
  );
}

function _tokenUrl() {
  return (
    "https://login.microsoftonline.com/" + TENANT_ID + "/oauth2/v2.0/token"
  );
}

// OAuth redirect_uri, derived per-request from the Host — the callback always returns to the
// same host the flow started on, so there is no ENTRA_REDIRECT_URI setting. The value must be a
// redirect URI registered on the app reg (AD rejects unregistered URIs — that is also what bounds
// a spoofed Host). The /authorize and token-exchange calls must pass the SAME value; both derive it.
function _redirectUri(r) {
  return "https://" + _header(r, "Host") + CALLBACK_PATH;
}

// utf-8 <-> base64url via Buffer (NOT btoa/atob — the payload can hold non-Latin1 CMS text).
function _b64urlEncode(str) {
  return Buffer.from(str, "utf8")
    .toString("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
}

function _b64urlDecode(str) {
  return Buffer.from(
    str.replace(/-/g, "+").replace(/_/g, "/"),
    "base64",
  ).toString("utf8");
}

// Random hex handle for the OAuth state — cryptographically secure via Web Crypto
// (crypto.getRandomValues is present in njs 0.8.5). No Math.random fallback: it is not a
// CSPRNG (SonarQube "make sure this pseudorandom generator is safe") and this value guards
// the OAuth state (CSRF), so a weak source would be a real weakness, not lint noise.
function _rand(byteLen) {
  const bytes = new Uint8Array(byteLen);
  crypto.getRandomValues(bytes);
  return Array.from(bytes)
    .map(function (b) {
      return b.toString(16).padStart(2, "0");
    })
    .join("");
}

// Fully configured? Read at REQUEST time (not the load-time consts): with any piece missing the
// round trip could only end on a Microsoft error page (no client / tenant) or fail every state
// check (no HMAC key), so /init must not start it at all.
function isConfigured() {
  return !!(
    process.env.ENTRA_TENANT_ID &&
    process.env.ENTRA_CLIENT_ID &&
    process.env.ENTRA_CLIENT_SECRET &&
    process.env.ENTRA_STATE_HMAC_SECRET
  );
}

// Which kind of destination `r` is, for passing the CMS token on (decided 2026-10-15):
//   "polaris" — our own /auth-refresh-inbound on THIS host (relative, or absolute to this Host):
//               it gets token + version, so drop1 skips discover/mint/verify;
//   "os"      — an OutSystems handover page (…/auth-handover.html?…stage=os-cookie-return…):
//               global-components' os-handover takes the token and skips its own bounce;
//   "other"   — everyone else gets exactly today's r + cc.
function _destKind(r, dest) {
  const thisHost = "https://" + _header(r, "Host") + "/";
  const path = dest.indexOf(thisHost) === 0 ? dest.substring(thisHost.length - 1) : dest;
  if (/^\/auth-refresh-inbound(?:[?#]|$)/.test(path)) return "polaris";
  const m = dest.match(/^https:\/\/[^/?#]+\/[^?#]*\/auth-handover\.html\?([^#]*)/i);
  if (m && /(?:^|&)stage=os-cookie-return(?:&|$)/.test(m[1])) return "os";
  return "other";
}

// Carry on exactly as /init would have: r + "?"/"&" + cc=<cookies> (appAuthRedirect's own
// formula), plus the CMS token for the destinations that can use it.
function _continuationUrl(r, st) {
  let url = st.r + (st.r.indexOf("?") !== -1 ? "&" : "?") + "cc=" + encodeURIComponent(st.cc);
  const kind = _destKind(r, st.r);
  if (st.tok && (kind === "polaris" || kind === "os")) {
    url += "&cms-modern-token=" + encodeURIComponent(st.tok);
    if (kind === "polaris" && st.ver) url += "&cms-version-id=" + encodeURIComponent(st.ver);
  }
  return replaceDdei.absoluteUrl(r, url);
}

// HMAC-SHA256 (base64url) of a string under the state secret.
function _stateMac(payload) {
  return cryptoModule
    .createHmac("sha256", STATE_HMAC_SECRET)
    .update(payload)
    .digest("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
}

// Constant-time string compare (njs has no crypto.timingSafeEqual). MAC length is fixed and not
// secret, so the early length check is fine.
function _constEq(a, b) {
  if (a.length !== b.length) return false;
  let d = 0;
  for (let i = 0; i < a.length; i++) d |= a.charCodeAt(i) ^ b.charCodeAt(i);
  return d === 0;
}

// Pack / unpack the state payload as `base64url(JSON) . HMAC`. The MAC makes the cookie
// tamper-evident: an attacker who can SET a state cookie (e.g. a Domain=cps.gov.uk cookie-toss
// from a sibling subdomain) cannot forge a valid one without STATE_HMAC_SECRET, so the `state`-
// param check can't be satisfied with a self-supplied cookie; iat + h (checked in _loadState) stop
// a REAL one being replayed later or on another host. See QUIRKS E1.
function _packState(st) {
  const payload = _b64urlEncode(JSON.stringify(st));
  return payload + "." + _stateMac(payload);
}

function _unpackState(raw) {
  if (!STATE_HMAC_SECRET) throw new Error("state: no HMAC secret configured");
  const dot = raw.lastIndexOf(".");
  if (dot === -1) throw new Error("state: no MAC");
  const payload = raw.substring(0, dot);
  if (!_constEq(raw.substring(dot + 1), _stateMac(payload))) {
    throw new Error("state: bad MAC");
  }
  return JSON.parse(_b64urlDecode(payload));
}

// The callback's view of its flow: the cookie named by the `state` handle, signature-checked, then
// bound to that handle, this host, and its 300s lifetime. { st } or { st: null, reason }.
function _loadState(r, handle) {
  const raw = _cookie(r, STATE_COOKIE_PREFIX + handle);
  if (!raw) return { st: null, reason: "missing-state" };
  let st;
  try {
    st = _unpackState(raw);
  } catch (e) {
    return { st: null, reason: "bad-state" };
  }
  if (st.s !== handle) return { st: null, reason: "bad-state" };
  const now = Math.floor(Date.now() / 1000);
  if (typeof st.iat !== "number" || now - st.iat > STATE_MAX_AGE_S || st.iat - now > STATE_SKEW_S) {
    return { st: null, reason: "expired-state" };
  }
  if (st.h !== _header(r, "Host")) return { st: null, reason: "wrong-host" };
  return { st: st, reason: "" };
}

// POST a form to the AD token endpoint; returns { accessToken, refreshToken, expiresIn, diag }. On
// an AD error the diag carries AD's error code (e.g. invalid_grant / AADSTS65001 = consent missing)
// — never a token. Shared by the code exchange and the refresh-token swaps.
async function _tokenRequest(fields) {
  const body = fields.join("&");
  try {
    const resp = await ngx.fetch(_tokenUrl(), {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
        Host: "login.microsoftonline.com",
      },
      body: body,
    });
    const text = await resp.text();
    let data = {};
    try {
      data = JSON.parse(text);
    } catch (e) {
      data = {};
    }
    if (!resp.ok || !data.access_token) {
      const codes = Array.isArray(data.error_codes) ? " AADSTS" + data.error_codes.join(",AADSTS") : "";
      return { accessToken: "", refreshToken: "", expiresIn: 0, diag: "HTTP " + resp.status + " " + (data.error || "") + codes };
    }
    return {
      accessToken: data.access_token,
      refreshToken: data.refresh_token || "",
      expiresIn: Number(data.expires_in || 0),
      diag: "ok",
    };
  } catch (e) {
    return { accessToken: "", refreshToken: "", expiresIn: 0, diag: String(e) };
  }
}

// Exchange the authorization code. What we keep is the refresh token (offline_access).
function _exchangeCode(code, redirectUri) {
  return _tokenRequest([
    "client_id=" + encodeURIComponent(CLIENT_ID),
    "client_secret=" + encodeURIComponent(CLIENT_SECRET),
    "code=" + encodeURIComponent(code),
    "redirect_uri=" + encodeURIComponent(redirectUri),
    "grant_type=authorization_code",
    "scope=" + encodeURIComponent(SIGN_IN_SCOPE),
  ]);
}

// Refresh-token swap: the user's refresh token (+ our client secret — a confidential client's
// refresh token is useless without it) for an access token for one API's scope, same user (oid).
// Needs consent for `scope`: the CPS tenant blocks user consent, so the API's app reg pre-authorises
// our client (MDS: azuread_application_pre_authorized in MDS terraform) — otherwise AADSTS65001.
// One swap per API: Entra issues one resource per token.
function _refresh(refreshToken, scope) {
  if (!scope) return { accessToken: "", refreshToken: "", expiresIn: 0, diag: "no-scope" };
  if (!refreshToken) return { accessToken: "", refreshToken: "", expiresIn: 0, diag: "no-refresh-token" };
  return _tokenRequest([
    "client_id=" + encodeURIComponent(CLIENT_ID),
    "client_secret=" + encodeURIComponent(CLIENT_SECRET),
    "grant_type=refresh_token",
    "refresh_token=" + encodeURIComponent(refreshToken),
    "scope=" + encodeURIComponent(scope),
  ]);
}

// The presence cookies for a watchdog token (one per presence path).
function _presenceCookies(accessToken, expiresIn) {
  const maxAge = expiresIn > 0 ? expiresIn : 3600;
  return PRESENCE_PATHS.map(function (p) {
    return (
      PRESENCE_COOKIE + "=" + accessToken +
      "; Path=" + p + "; HttpOnly; Secure; SameSite=Lax; Max-Age=" + maxAge
    );
  });
}

// ---------------------------------------------------------------------------
// /init — look up the CMS session, then begin the AD round-trip.
// ---------------------------------------------------------------------------

// Called by appAuthRedirect after its whitelist check, with the destination and the FILTERED
// cookies it would otherwise 302 to. Resolves true once it has responded (302 to Entra); false
// WITHOUT responding on any failure — /init then carries on exactly as today. Any Set-Cookie
// /init already queued (the session hint) is kept.
async function beginFromInit(r, opts) {
  const term = opts.terminal === "iframe" ? "iframe" : "top-level";
  const skip = (reason, extra) => {
    _outcome("skipped", reason, _destKind(r, opts.redirectUrl || ""), term, extra);
    return false;
  };
  try {
    if (!isConfigured()) return skip("not-configured");
    // Nothing to gain without a service to swap for: top-level only deposits to MDS; the iframe
    // also sets the presence cookie. (Read per request, like isConfigured; e.g. UAT has no MDS
    // scope yet, so its top-level handovers skip the round trip instead of degrading after it.)
    const mdsScope = process.env.ENTRA_MDS_SCOPE || "";
    const watchdogScope = process.env.ENTRA_WATCHDOG_SCOPE || "";
    if (term === "top-level" && !mdsScope) return skip("no-mds-scope");
    if (term === "iframe" && !mdsScope && !watchdogScope) return skip("no-service-scope");
    if (!opts.cookies) return skip("no-cookies");

    const found = await replaceDdei.lookupCmsSession(_header(r, "Host"), opts.cookies);
    if (!found.token) {
      replaceDdei.markAuthInit(r, "entra-degraded");
      _log("drop2 entra degrade — cms-lookup-failed: " + found.reason);
      return skip("cms-lookup-failed");
    }

    const stateHandle = _rand(16);
    const payload = {
      s: stateHandle,
      // The iframe always ends on the terminal page and never reads r, so it isn't carried (keeps
      // the cookie small); the destination KIND is, for the outcome line.
      r: term === "iframe" ? undefined : opts.redirectUrl,
      dk: _destKind(r, opts.redirectUrl || ""),
      cc: opts.cookies,
      tok: found.token,
      ver: found.versionId,
      term: term,
      iat: Math.floor(Date.now() / 1000),
      h: _header(r, "Host"),
      corr: _rand(8),
    };
    const stateCookie = STATE_COOKIE_PREFIX + stateHandle + "=" + _packState(payload);
    if (stateCookie.length > MAX_STATE_COOKIE) {
      // Length only — never the content (it holds the CMS cookies + token).
      return skip("state-too-large", " bytes=" + stateCookie.length);
    }
    const queued = [].concat(r.headersOut["Set-Cookie"] || []);
    r.headersOut["Set-Cookie"] = queued.concat([stateCookie + STATE_SET_OPTS]);

    const params = [
      "client_id=" + encodeURIComponent(CLIENT_ID),
      "response_type=code",
      "redirect_uri=" + encodeURIComponent(_redirectUri(r)),
      "scope=" + encodeURIComponent(SIGN_IN_SCOPE),
      "state=" + (term === "iframe" ? "i." : "t.") + stateHandle,
      "response_mode=query",
      // Silent auth: CMS users already have an AAD session (same tenant), so prompt=none
      // yields a bare 302 (code, or error=login_required) with no UI to render — which is
      // also what lets it run framed (AD sets X-Frame-Options: DENY on any RENDERED
      // /authorize response). See the reference's notes.
      "prompt=none",
    ].join("&");

    replaceDdei.markAuthInit(r, "entra");
    r.return(302, _authorizeUrl() + "?" + params);
    return true;
  } catch (e) {
    replaceDdei.markAuthInit(r, "entra-degraded");
    _log("drop2 entra degrade — entra-begin-error: " + String(e));
    return skip("entra-begin-error");
  }
}

// ---------------------------------------------------------------------------
// /init/entra-callback — code exchange, swaps, store deposit, carry on.
// ---------------------------------------------------------------------------

async function handleInitEntraCallback(r) {
  let st = null;
  try {
    const m = _arg(r, "state").match(STATE_PARAM_RE);
    if (!m) {
      _degrade(r, null, "bad-state-param");
      return;
    }
    const lost = { mode: m[1] === "i" ? "iframe" : "top-level", handle: m[2] };
    const loaded = _loadState(r, lost.handle);
    st = loaded.st;

    const adError = _arg(r, "error");
    if (adError) {
      // e.g. login_required (no silent AD session) — degrade, don't block anyone's handover.
      // The outcome line records which AD error, and its AADSTS number (see _adFields).
      _degrade(r, st, "ad-error: " + adError, null, lost, _adFields(adError, _arg(r, "error_description")));
      return;
    }
    if (!st) {
      _degrade(r, null, loaded.reason, null, lost);
      return;
    }
    const code = _arg(r, "code");
    if (!code) {
      _degrade(r, st, "no-code");
      return;
    }

    const tok = await _exchangeCode(code, _redirectUri(r));
    if (!tok.accessToken) {
      _degrade(r, st, "token-exchange-failed: " + tok.diag);
      return;
    }
    const rt = tok.refreshToken;
    if (!rt) {
      // offline_access not granted: nothing to swap.
      _degrade(r, st, "no-refresh-token");
      return;
    }

    // Presence first (iframe only), so a later MDS failure cannot cost presence its token. A
    // presence failure is logged but is not a degrade: the store is what drop2 is for.
    let extra = [];
    if (st.term === "iframe" && WATCHDOG_SCOPE) {
      const wd = await _refresh(rt, WATCHDOG_SCOPE);
      if (wd.accessToken) {
        extra = _presenceCookies(wd.accessToken, wd.expiresIn);
      } else {
        _log("drop2 entra presence swap failed — " + wd.diag);
      }
    }

    // The auth store: swap for the store backend's token, then deposit.
    const mds = await _refresh(rt, store.scope);
    if (!mds.accessToken) {
      _degrade(r, st, "swap-failed: " + mds.diag, extra);
      return;
    }
    const dep = await store.deposit({ cookies: st.cc, token: st.tok }, mds.accessToken);
    if (!dep.ok) {
      _degrade(r, st, "store-deposit-failed: " + dep.diag, extra);
      return;
    }

    _succeed(r, st, extra);
  } catch (e) {
    _degrade(r, st, "callback-error: " + String(e));
  }
}

// Success terminal: clear state (+ any presence cookies), then branch on mode.
function _succeed(r, st, extra) {
  replaceDdei.markAuthInit(r, "entra");
  _outcome("ok", "ok", _stDestKind(r, st), st.term);
  _finish(r, st, extra);
}

// Best-effort degrade path. Never blocks anyone's handover: top-level carries on to r exactly as
// /init would (the CMS token is still passed on: it was verified at /init, independent of Entra);
// iframe just renders the terminal. With no recoverable state there is no destination: the iframe
// (known from the `state` param's mode) still gets the terminal page; top-level lands on the
// Polaris UI, the only safe choice. `lost` = { mode, handle } from the `state` param, when known.
function _degrade(r, st, reason, extra, lost, outcomeExtra) {
  // Marker only — the reason goes to the error log below, never to the client.
  replaceDdei.markAuthInit(r, "entra-degraded");
  _log("drop2 entra degrade — " + reason);
  const code = reason.split(":")[0].trim();
  if (st) {
    _outcome("degraded", code, _stDestKind(r, st), st.term, outcomeExtra);
    _finish(r, st, extra);
    return;
  }
  _outcome("degraded", code, "unknown", lost ? lost.mode : "unknown", outcomeExtra);
  r.headersOut["Set-Cookie"] = lost ? [STATE_COOKIE_PREFIX + lost.handle + "=deleted" + STATE_CLEAR_OPTS] : [];
  if (lost && lost.mode === "iframe") {
    r.headersOut["Content-Type"] = "text/html; charset=utf-8";
    r.return(200, TERMINAL_HTML);
    return;
  }
  r.return(302, replaceDdei.absoluteUrl(r, FALLBACK_LANDING));
}

// Clear THIS flow's state cookie (+ queue any presence cookies), then iframe terminal or carry on.
function _finish(r, st, extra) {
  r.headersOut["Set-Cookie"] = [STATE_COOKIE_PREFIX + st.s + "=deleted" + STATE_CLEAR_OPTS].concat(extra || []);
  if (st.term === "iframe") {
    // Pure side-channel: store (+ presence) only, static page.
    r.headersOut["Content-Type"] = "text/html; charset=utf-8";
    r.return(200, TERMINAL_HTML);
    return;
  }
  r.return(302, _continuationUrl(r, st));
}

// The destination kind for the outcome line: recorded at /init (dk); else derived from r.
function _stDestKind(r, st) {
  return st.dk || _destKind(r, st.r || "");
}

// The AD error fields for the outcome line: ` ad=<error> aadsts=<number>`. Both come off the
// callback's query string, which anyone can craft, so each is reduced to a safe shape (else
// `other` / `none`). The AADSTS number tells the cases `error` alone conflates, e.g.
// interaction_required covers both 16000 (several signed-in accounts: pick one) and
// 50076 (MFA needed); login_required is 50058 (no AD session at all).
function _adFields(error, description) {
  const ad = /^[a-z_]{1,40}$/.test(error) ? error : "other";
  const m = /^AADSTS(\d{1,8})\b/.exec(description);
  return " ad=" + ad + " aadsts=" + (m ? m[1] : "none");
}

// D5: ONE structured line per handover that drop2 handled, for KQL over AppServiceConsoleLogs
// (Polaris global_la) — the rate of each outcome, per destination kind:
//   drop2-outcome result=ok|degraded|skipped reason=<code> term=top-level|iframe dest=polaris|os|other|unknown
// reason=ad-error adds ` ad=<AD error> aadsts=<number|none>` (_adFields).
// skipped = /init never started Entra (begin); ok / degraded = the callback's end. Codes only:
// never a token, a cookie, a URL or an identity (the free-text detail stays in the degrade line).
// Written once per flow: begin writes it only when it skips; the callback writes the rest.
function _outcome(result, reason, dest, term, extra) {
  _log(
    "drop2-outcome result=" + result + " reason=" + reason + " term=" + term + " dest=" + dest + (extra || ""),
  );
}

// /init/terminal — the hidden iframe's end page when drop2 did not start (appAuthRedirect 302s
// here so the frame never falls through into the Polaris UI). Static, sets nothing; its location
// carries the frame-ancestors CSP that sheds X-Frame-Options: DENY (QUIRKS E2), so it renders in
// the CMS frame and fires the harness's onload.
function terminal(r) {
  r.headersOut["Content-Type"] = "text/html; charset=utf-8";
  r.return(200, TERMINAL_HTML);
}

// Best-effort error-log line. Callers never pass a token.
function _log(msg) {
  try {
    ngx.log(ngx.ERR, msg);
  } catch (e) {
    // logging is best-effort
  }
}

export default {
  beginFromInit,
  handleInitEntraCallback,
  terminal,
  isConfigured,
  // exposed for the unit test (production only calls the entries above):
  __test: {
    packState: _packState,
    unpackState: _unpackState,
    b64urlEncode: _b64urlEncode,
    b64urlDecode: _b64urlDecode,
    refresh: _refresh,
    destKind: _destKind,
    presenceCookies: _presenceCookies,
    rand: _rand,
    authorizeUrl: _authorizeUrl,
    tokenUrl: _tokenUrl,
    constants: {
      STATE_COOKIE_PREFIX: STATE_COOKIE_PREFIX,
      TENANT_ID: TENANT_ID,
      TERMINAL_HTML: TERMINAL_HTML,
      PRESENCE_COOKIE: PRESENCE_COOKIE,
    },
  },
};
