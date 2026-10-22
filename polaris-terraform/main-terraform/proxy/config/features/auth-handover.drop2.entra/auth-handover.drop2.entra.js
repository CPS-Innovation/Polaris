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
//     -> 302 to Entra /authorize (prompt=none, silent): openid profile offline_access, plus
//        login_hint when the browser has a login hint (D7, below).
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
// TOKEN MODEL (decided 2026-10-15): ONE silent, NEUTRAL sign-in
// (openid profile offline_access — no API scope, so drop2 needs our app reg to expose none), then a
// refresh-token swap per service: a confidential client's refresh token buys an access token for
// any API the app may call (consent = that API pre-authorising our client). No on-behalf-of: that
// needs a token addressed to us, i.e. an exposed scope. The refresh token never leaves this
// callback (no cookie, no log); the browser only ever holds the watchdog token (iframe). The
// id_token is used only to record the login hint (which account signed in); each backend still
// identifies the user from its own token (oid).
//
// LOGIN HINT + INTERACTIVE RETRY (D7, QUIRKS E14): every successful sign-in records WHICH account
// signed in, in a signed, host-wide cookie (cps-entra-login-hint, 30 days), served to apps at
// GET /global-components/entra-login-hint (DELETE forgets it) so their own sign-ins (MSAL loginHint)
// land on the SAME account the MDS deposit is filed under. /init sends it as login_hint, so a
// browser with several accounts signs in silently as the hinted one. When the silent sign-in fails
// in a way a user can fix (several accounts / no session / MFA), a TOP-LEVEL handover retries ONCE
// interactively (prompt=select_account), if ENTRA_INTERACTIVE_ENABLED=true (pre-prod); the choice
// becomes the new hint, with `via` saying why the picker was needed (apps show their "you have
// several accounts" UI only for several-accounts). A several-accounts hint is RE-CHECKED on each
// handover: /init first signs in silently WITHOUT it (the probe); still several (AADSTS16000, or
// another account wins) -> a second silent leg WITH it; else via becomes silent. Prod (off) stays a
// pure prompt=none probe. The iframe never prompts.
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

// The interactive leg (D7) can wait on a person (account picker, MFA), so its state lives longer.
const INTERACTIVE_MAX_AGE_S = 900;
const STATE_SET_OPTS_INTERACTIVE =
  "; Path=" + CALLBACK_PATH + "; HttpOnly; Secure; SameSite=Lax; Max-Age=" + INTERACTIVE_MAX_AGE_S;

// D7: the login hint — which Entra account the last successful sign-in on this host used. Host-wide
// (Path=/) so /init sends it and the hint endpoint serves it; HttpOnly (only those two read it);
// SameSite=None so an app on another site can fetch the endpoint with credentials. HMAC-signed like
// the state (with a kind marker, so neither can stand in for the other), so a sibling subdomain
// can't plant a hint. Every successful sign-in overwrites it; DELETE on the endpoint forgets it.
const HINT_COOKIE = "cps-entra-login-hint";
const HINT_MAX_AGE_S = 30 * 24 * 3600;
const HINT_OPTS = "; Path=/; HttpOnly; Secure; SameSite=None; Max-Age=";

// AD errors a person can resolve at an interactive sign-in: several accounts (AADSTS16000), no
// session (50058), MFA (50076/50079). Conditional Access device blocks (AADSTS53xxx) are excluded:
// an interactive attempt would only strand the user on Microsoft's block page.
const RETRYABLE_AD_ERRORS = ["login_required", "interaction_required", "account_selection_required"];

// How the hint's account was chosen (the hint's `via`, QUIRKS E14), from the AADSTS number that sent
// the silent leg to the picker; any other number is "other". A sign-in without the picker is "silent".
const VIA_BY_AADSTS = { "16000": "several-accounts", "50058": "no-session", "50076": "mfa", "50079": "mfa" };

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

// Interactive retry switch (D7). Read per request, like isConfigured.
function _interactiveEnabled() {
  return process.env.ENTRA_INTERACTIVE_ENABLED === "true";
}

// The browser's login hint, signature-checked; null if absent, unsigned, forged or not a hint.
function _readHint(r) {
  const raw = _cookie(r, HINT_COOKIE);
  if (!raw) return null;
  try {
    const h = _unpackState(raw);
    return h && h.k === "hint" && typeof h.lh === "string" && h.lh ? h : null;
  } catch (e) {
    return null;
  }
}

// The id_token's claims. No signature check: it came straight from the token endpoint in the code
// exchange (our own back-channel call), and only names the account for the hint, never authorises.
function _idClaims(idToken) {
  try {
    return JSON.parse(_b64urlDecode(String(idToken).split(".")[1]));
  } catch (e) {
    return null;
  }
}

// The Set-Cookie for the hint from a sign-in's id_token, or null. login_hint is the optional claim
// Microsoft provides for exactly this (an opaque value); until the app reg emits it,
// preferred_username (the UPN) does the same job. `st` = the flow (absent: a silent sign-in).
function _hintCookie(idToken, st) {
  const c = _idClaims(idToken);
  const lh = c && (c.login_hint || c.preferred_username);
  if (!lh) return null;
  const h = {
    k: "hint",
    lh: String(lh),
    u: String(c.preferred_username || ""),
    oid: String(c.oid || ""),
    tid: String(c.tid || ""),
    at: Math.floor(Date.now() / 1000),
    via: _hintVia(st || {}),
  };
  return HINT_COOKIE + "=" + _packState(h) + HINT_OPTS + HINT_MAX_AGE_S;
}

// The hint's `via`: m = the probe found several accounts this flow; the picker leg (n) names why
// it was needed (ra = the silent leg's AADSTS); otherwise "silent". A hinted silent sign-in can't
// tell whether other accounts exist (the hint picks one), which is why /init probes first (_probe).
function _hintVia(st) {
  if (st.m) return "several-accounts";
  return st.n ? VIA_BY_AADSTS[st.ra] || "other" : "silent";
}

// Probe (begin): a several-accounts hint is re-checked by signing in WITHOUT it first, so via
// follows the browser (the other account signed out -> silent) instead of sticking forever.
function _probe(hint) {
  return !!(hint && hint.via === "several-accounts");
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
  let url = st.r + (st.r.includes("?") ? "&" : "?") + "cc=" + encodeURIComponent(st.cc);
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
  const maxAge = st.n ? INTERACTIVE_MAX_AGE_S : STATE_MAX_AGE_S;
  if (typeof st.iat !== "number" || now - st.iat > maxAge || st.iat - now > STATE_SKEW_S) {
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
      idToken: data.id_token || "",
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

// Why /init must not start Entra for this handover ("" = go ahead). The scopes are read per
// request, like isConfigured: nothing to gain without a service to swap for — top-level only
// deposits to MDS; the iframe also sets the presence cookie (an env with no MDS scope skips the
// round trip on top-level handovers instead of degrading after it).
function _skipReason(term, opts) {
  if (!isConfigured()) return "not-configured";
  const mdsScope = process.env.ENTRA_MDS_SCOPE || "";
  const watchdogScope = process.env.ENTRA_WATCHDOG_SCOPE || "";
  if (term === "top-level" && !mdsScope) return "no-mds-scope";
  if (term === "iframe" && !mdsScope && !watchdogScope) return "no-service-scope";
  if (!opts.cookies) return "no-cookies";
  return "";
}

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
    const why = _skipReason(term, opts);
    if (why) return skip(why);

    const found = await replaceDdei.lookupCmsSession(_header(r, "Host"), opts.cookies);
    if (!found.token) {
      replaceDdei.markAuthInit(r, "entra-degraded");
      _log("drop2 entra degrade — cms-lookup-failed: " + found.reason);
      return skip("cms-lookup-failed");
    }

    const stateHandle = _rand(16);
    const hint = _readHint(r);
    const probe = _probe(hint);
    const sent = probe ? null : hint; // the hint /authorize gets (withheld by the probe)
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
      lh: sent ? 1 : 0,
      p: probe ? 1 : 0,
    };
    const stateCookie = STATE_COOKIE_PREFIX + stateHandle + "=" + _packState(payload);
    if (stateCookie.length > MAX_STATE_COOKIE) {
      // Length only — never the content (it holds the CMS cookies + token).
      return skip("state-too-large", " bytes=" + stateCookie.length);
    }
    const queued = [].concat(r.headersOut["Set-Cookie"] || []);
    r.headersOut["Set-Cookie"] = queued.concat([stateCookie + STATE_SET_OPTS]);

    replaceDdei.markAuthInit(r, "entra");
    // Silent auth: CMS users already have an AAD session (same tenant), so prompt=none yields a
    // bare 302 (code, or error=login_required) with no UI to render — which is also what lets it
    // run framed (AD sets X-Frame-Options: DENY on any RENDERED /authorize response).
    r.return(302, _authorizeRedirect(r, stateHandle, term, "none", sent ? sent.lh : ""));
    return true;
  } catch (e) {
    replaceDdei.markAuthInit(r, "entra-degraded");
    _log("drop2 entra degrade — entra-begin-error: " + String(e));
    return skip("entra-begin-error");
  }
}

// The /authorize URL for a flow: `prompt` "none" (silent) or "select_account" (D7's interactive
// retry); `loginHint` "" = none.
function _authorizeRedirect(r, handle, term, prompt, loginHint) {
  const params = [
    "client_id=" + encodeURIComponent(CLIENT_ID),
    "response_type=code",
    "redirect_uri=" + encodeURIComponent(_redirectUri(r)),
    "scope=" + encodeURIComponent(SIGN_IN_SCOPE),
    "state=" + (term === "iframe" ? "i." : "t.") + handle,
    "response_mode=query",
    "prompt=" + prompt,
  ];
  if (loginHint) params.push("login_hint=" + encodeURIComponent(loginHint));
  return _authorizeUrl() + "?" + params.join("&");
}

// D7: retry a failed silent sign-in ONCE, interactively? Top-level only (an iframe can't show
// Microsoft's pages), with the switch on, for an AD error a person can resolve.
function _canRetry(st, adError, aadsts) {
  return (
    st.term === "top-level" &&
    !st.n &&
    _interactiveEnabled() &&
    RETRYABLE_AD_ERRORS.includes(adError) &&
    aadsts.indexOf("53") !== 0
  );
}

// D7: the same flow (same handle, same destination) again, now with the account picker. No
// login_hint: the user is choosing (that is also how they escape a stale hint); whatever they pick
// becomes the new hint. The state is re-signed with n=1 (never retried twice) and a fresh iat with
// the longer interactive lifetime. p=0: the user's pick is the answer the probe was after, so it is
// not re-checked against the old hint (that would sign them back in as it); lh=0: no hint is sent.
function _retryInteractive(r, st, outcomeExtra, aadsts) {
  const again = Object.assign({}, st, { n: 1, p: 0, lh: 0, ra: aadsts, iat: Math.floor(Date.now() / 1000) });
  r.headersOut["Set-Cookie"] = [STATE_COOKIE_PREFIX + st.s + "=" + _packState(again) + STATE_SET_OPTS_INTERACTIVE];
  replaceDdei.markAuthInit(r, "entra");
  _outcome("retried", "ad-error", _stDestKind(r, st), st.term, outcomeExtra + _legFields(st));
  r.return(302, _authorizeRedirect(r, st.s, st.term, "select_account", ""));
}

// The probe found several accounts: the same flow again, silently, WITH the hint (m=1 records
// the finding for via). Not the probe any more (p=0), so it can't loop; a failure there takes the
// normal path (interactive retry or degrade). Fresh iat: the silent leg's 300s runs from here.
function _retryHinted(r, st, hint, reason, outcomeExtra) {
  const again = Object.assign({}, st, { p: 0, m: 1, lh: 1, iat: Math.floor(Date.now() / 1000) });
  r.headersOut["Set-Cookie"] = [STATE_COOKIE_PREFIX + st.s + "=" + _packState(again) + STATE_SET_OPTS];
  replaceDdei.markAuthInit(r, "entra");
  _outcome("retried", reason, _stDestKind(r, st), st.term, outcomeExtra + _legFields(st));
  r.return(302, _authorizeRedirect(r, st.s, st.term, "none", hint.lh));
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
      _onAdError(r, st, adError, lost);
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
    if (_probeMetOtherAccount(r, st, tok.idToken)) return;
    await _swapAndDeposit(r, st, tok);
  } catch (e) {
    _degrade(r, st, "callback-error: " + String(e));
  }
}

// The callback's AD error, e.g. login_required (no silent AD session). The outcome line records
// which AD error, and its AADSTS number (see _adFields). The probe meeting several accounts -> the
// hinted silent leg; resolvable by a person -> one interactive retry (D7); otherwise degrade,
// never blocking anyone's handover.
function _onAdError(r, st, adError, lost) {
  const fields = _adFields(adError, _arg(r, "error_description"));
  const aadsts = _aadsts(_arg(r, "error_description"));
  const hint = st && st.p ? _readHint(r) : null;
  if (hint && aadsts === "16000") {
    _retryHinted(r, st, hint, "several-accounts", fields);
    return;
  }
  if (st && _canRetry(st, adError, aadsts)) {
    _retryInteractive(r, st, fields, aadsts);
    return;
  }
  _degrade(r, st, "ad-error: " + adError, null, lost, fields);
}

// D7 probe: another account won the unhinted sign-in, so several are still signed in — sign in
// again WITH the hint, so the account the user chose stays the one used (this code is unused).
// True once it has responded.
function _probeMetOtherAccount(r, st, idToken) {
  if (!st.p) return false;
  const hint = _readHint(r);
  const c = _idClaims(idToken);
  if (!hint || !hint.oid || !c || String(c.oid || "") === hint.oid) return false;
  _retryHinted(r, st, hint, "other-account", "");
  return true;
}

// Signed in (`tok` from the code exchange): record the hint, swap, deposit, then succeed or
// degrade. Throws go to the callback's catch.
async function _swapAndDeposit(r, st, tok) {
  // D7: the sign-in worked, so we know which account it chose — record it as the login hint,
  // whatever happens next (later failures don't change which account this browser uses).
  let extra = [];
  const hc = _hintCookie(tok.idToken, st);
  if (hc) extra.push(hc);

  const rt = tok.refreshToken;
  if (!rt) {
    // offline_access not granted: nothing to swap.
    _degrade(r, st, "no-refresh-token", extra);
    return;
  }

  // Presence first (iframe only), so a later MDS failure cannot cost presence its token. A
  // presence failure is logged but is not a degrade: the store is what drop2 is for.
  if (st.term === "iframe" && WATCHDOG_SCOPE) {
    const wd = await _refresh(rt, WATCHDOG_SCOPE);
    if (wd.accessToken) {
      extra = extra.concat(_presenceCookies(wd.accessToken, wd.expiresIn));
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
}

// Success terminal: clear state (+ any presence cookies), then branch on mode.
function _succeed(r, st, extra) {
  replaceDdei.markAuthInit(r, "entra");
  _outcome("ok", "ok", _stDestKind(r, st), st.term, _legFields(st));
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
    _outcome("degraded", code, _stDestKind(r, st), st.term, (outcomeExtra || "") + _legFields(st));
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
  return " ad=" + ad + " aadsts=" + (_aadsts(description) || "none");
}

// The AADSTS number at the start of an error_description, digits only; "" if none.
function _aadsts(description) {
  const m = /^AADSTS(\d{1,8})\b/.exec(String(description || ""));
  return m ? m[1] : "";
}

// D7, on every callback line with a known flow: was a login hint sent (hint=1), and is this the
// interactive leg (int=1)?
function _legFields(st) {
  return " hint=" + (st.lh ? 1 : 0) + " int=" + (st.n ? 1 : 0);
}

// D5: ONE structured line per handover that drop2 handled, for KQL over AppServiceConsoleLogs
// (Polaris global_la) — the rate of each outcome, per destination kind:
//   drop2-outcome result=ok|degraded|skipped|retried reason=<code> term=top-level|iframe dest=polaris|os|other|unknown
// reason=ad-error adds ` ad=<AD error> aadsts=<number|none>` (_adFields); callback lines with a
// known flow add ` hint=0|1 int=0|1` (_legFields). `retried` (D7) is a leg's line when it hands
// over to another leg of the same flow: reason=ad-error -> the interactive leg (which ends int=1);
// reason=several-accounts / other-account -> the probe found several accounts, so a hinted silent
// leg follows. Either way the flow then ends with its own ok/degraded line.
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

// GET /global-components/entra-login-hint — the login hint for apps (D7): JSON
// {loginHint, username, oid, tid, setAt} or null. DELETE forgets it (the next handover then lets the
// user choose again). The CORS headers and OPTIONS preflight are in the .conf.
// js_set $entra_hint_cors_origin: global-components' $cors_origin, but only an https origin —
// the hint endpoint returns who the user is, so no plain-http page gets a credentialed read.
function hintCorsOrigin(r) {
  const o = r.variables.cors_origin || "";
  return o.startsWith("https://") ? o : "";
}

function loginHint(r) {
  r.headersOut["Content-Type"] = "application/json";
  r.headersOut["Cache-Control"] = "no-store";
  if (r.method === "DELETE") {
    r.headersOut["Set-Cookie"] = [HINT_COOKIE + "=deleted" + HINT_OPTS + "0"];
    r.return(200, JSON.stringify({ cleared: true }));
    return;
  }
  if (r.method !== "GET") {
    r.return(405, JSON.stringify({ error: "Method not allowed" }));
    return;
  }
  const h = _readHint(r);
  r.return(200, h ? JSON.stringify({ loginHint: h.lh, username: h.u, oid: h.oid, tid: h.tid, setAt: h.at, via: h.via || "unknown" }) : "null");
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
  loginHint,
  hintCorsOrigin,
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
    hintCookie: _hintCookie,
    tokenUrl: _tokenUrl,
    constants: {
      STATE_COOKIE_PREFIX: STATE_COOKIE_PREFIX,
      TENANT_ID: TENANT_ID,
      TERMINAL_HTML: TERMINAL_HTML,
      PRESENCE_COOKIE: PRESENCE_COOKIE,
      HINT_COOKIE: HINT_COOKIE,
    },
  },
};
