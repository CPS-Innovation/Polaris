// ---------------------------------------------------------------------------
// Entra store (drop2) — the Azure AD half of the auth handover (switch-gated).
//
// Extends drop1 (auth-handover.drop1.replace-ddei). Reached ONLY when
// ENTRA_STORE_ENABLED=true, via an internal rewrite from /auth-refresh-inbound (the
// switch lives in auth-handover.conf, beside drop1's). Two njs handlers over one core:
//
//   handleInitEntra          (/init-entra, internal)
//     drop1.establishCmsSession (whitelist cookies -> mint + verify modern token)
//     -> pack {cookies, token, version, landing, terminal} into a first-party state
//        cookie + a random anti-CSRF handle in the OAuth `state` param
//     -> 302 to Entra /authorize (prompt=none, silent).
//
//   handleInitEntraCallback  (/init-v2/callback, public — AD 302s the browser here)
//     validate state -> exchange code -> id_token -> validate claims -> extract OID
//     -> store.deposit(oid, {cookies, modernToken, correlationId, email}, {idToken})
//     -> set the id-token cookie
//     -> finalize: TOP-LEVEL 302 to the landing (+ Cms-Auth-Values, ADDITIVE);
//        IFRAME 200 static terminal page (harness tears the iframe down on its onload).
//
// TRANSPARENT / ADDITIVE / BEST-EFFORT: any Entra or store failure degrades to plain
// drop1 behaviour (top-level: still set Cms-Auth-Values + land the user; iframe: just
// render the terminal) — the store write never breaks the user's login.
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
// switch off. Imports drop1 (establish/finalize) + store.js (the deposit seam); drop1
// imports nothing back, so the dependency is one-directional and drop2 removes cleanly.
// ---------------------------------------------------------------------------

import replaceDdei from "../auth-handover.drop1.replace-ddei/auth-handover.drop1.replace-ddei.js";
import store from "./store.js";
import cryptoModule from "crypto"; // njs built-in — for the state-cookie HMAC (same as store.js)

// Azure AD app registration — config comes ONLY from app settings; NO baked defaults (see
// TODO.APP-SETTINGS.md). Missing => "" => the flow fails claims/exchange and degrades to drop1.
// tenant/client are non-secret ids; the client SECRET is a secret. There is NO ENTRA_REDIRECT_URI
// setting: the redirect_uri is derived per-request from the incoming Host (see _redirectUri) — the
// flow always returns to the same host, which must be a redirect URI registered on the app reg.
const TENANT_ID = process.env.ENTRA_TENANT_ID || "";
const CLIENT_ID = process.env.ENTRA_CLIENT_ID || "";
const CLIENT_SECRET = process.env.ENTRA_CLIENT_SECRET || "";
// Callback path — must match the `location = /init-v2/callback` in the .conf.
const CALLBACK_PATH = "/init-v2/callback";

// Server-side secret that signs the state cookie (integrity). EMPTY => fail closed: _unpackState
// rejects every cookie, so the callback degrades to drop1 — drop2 will not run without it. See
// TODO.APP-SETTINGS.md and QUIRKS E1.
const STATE_HMAC_SECRET = process.env.ENTRA_STATE_HMAC_SECRET || "";

// First-party state cookie: holds the (secret) session payload across the AD hop.
// Path scopes it to the callback; short-lived; HttpOnly so no script can read it.
const STATE_COOKIE = "entra_auth_state";
const STATE_SET_OPTS =
  "; Path=/init-v2; HttpOnly; Secure; SameSite=Lax; Max-Age=300";
const STATE_CLEAR_OPTS =
  "; Path=/init-v2; HttpOnly; Secure; SameSite=Lax; Max-Age=0";

// The iframe terminal: a bare page whose sole job is to fire `onload` so the harness
// that opened the hidden iframe can destroy it. No script, no data.
const TERMINAL_HTML =
  '<!DOCTYPE html><html lang="en"><head><meta charset="utf-8">' +
  "<title>CMS auth captured</title></head>" +
  '<body data-cms-auth="done"><!-- entra store populated --></body></html>';

const OIDC_SCOPE = "openid profile email";

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

// Random hex handle for state/nonce — cryptographically secure via Web Crypto
// (crypto.getRandomValues is present in njs 0.8.5). No Math.random fallback: it is not a
// CSPRNG (SonarQube "make sure this pseudorandom generator is safe") and this value guards
// the OAuth state/nonce (CSRF), so a weak source would be a real weakness, not lint noise.
function _rand(byteLen) {
  const bytes = new Uint8Array(byteLen);
  crypto.getRandomValues(bytes);
  return Array.from(bytes)
    .map(function (b) {
      return b.toString(16).padStart(2, "0");
    })
    .join("");
}

function _decodeJwtPayload(token) {
  const parts = token.split(".");
  if (parts.length !== 3) return null;
  try {
    return JSON.parse(_b64urlDecode(parts[1]));
  } catch (e) {
    return null;
  }
}

// Validate id_token claims. Returns "" when valid, else a short reason code.
function _validateClaims(claims, nonce) {
  if (claims.nonce !== nonce) return "nonce";
  if (claims.tid !== TENANT_ID) return "tid";
  const iss = String(claims.iss || "");
  const validIssuers = [
    "https://sts.windows.net/" + TENANT_ID + "/",
    "https://login.microsoftonline.com/" + TENANT_ID + "/v2.0",
  ];
  if (validIssuers.indexOf(iss) === -1) return "iss";
  const exp = Number(claims.exp || 0);
  if (!exp || exp < Math.floor(Date.now() / 1000)) return "exp";
  return "";
}

// Rebuild the drop1 session shape from the unpacked state payload.
function _sessionFrom(st) {
  return { cookieHeader: st.cc, token: st.tok, versionId: st.ver };
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
// tamper-evident: an attacker who can SET entra_auth_state (e.g. a Domain=cps.gov.uk cookie-toss
// from a sibling subdomain) cannot forge a valid one without STATE_HMAC_SECRET, so the circular
// `state`-param check can no longer be satisfied with a self-supplied cookie. See QUIRKS E1.
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

// Exchange the authorization code for tokens at the AD token endpoint.
async function _exchangeCode(code, redirectUri) {
  const body = [
    "client_id=" + encodeURIComponent(CLIENT_ID),
    "client_secret=" + encodeURIComponent(CLIENT_SECRET),
    "code=" + encodeURIComponent(code),
    "redirect_uri=" + encodeURIComponent(redirectUri),
    "grant_type=authorization_code",
    "scope=" + encodeURIComponent(OIDC_SCOPE),
  ].join("&");
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
    if (!resp.ok) {
      return { idToken: "", accessToken: "", diag: "HTTP " + resp.status };
    }
    const data = JSON.parse(text);
    return {
      idToken: data.id_token || "",
      accessToken: data.access_token || "",
      diag: "ok",
    };
  } catch (e) {
    return { idToken: "", accessToken: "", diag: String(e) };
  }
}

// ---------------------------------------------------------------------------
// /init-entra — establish the CMS session (drop1), then begin the AD round-trip.
// ---------------------------------------------------------------------------

async function handleInitEntra(r) {
  const landing = replaceDdei.captureLanding(r);
  try {
    // The CMS half is drop1's, unchanged (fail-redirects on its own failures).
    const session = await replaceDdei.establishCmsSession(r, landing);
    if (!session) return;

    // top-level (default) vs iframe expansion. The harness supplies terminal=iframe;
    // threading it through /polaris -> /init is future harness wiring (see QUIRKS.md).
    const terminal = _arg(r, "terminal") === "iframe" ? "iframe" : "top-level";

    const stateHandle = _rand(16);
    const nonce = _rand(16);
    const payload = {
      s: stateHandle,
      n: nonce,
      cc: session.cookieHeader,
      tok: session.token,
      ver: session.versionId,
      ui: landing.polarisUiUrl || "",
      q: landing.q || "",
      term: terminal,
      corr: _rand(8),
    };
    r.headersOut["Set-Cookie"] = [
      STATE_COOKIE + "=" + _packState(payload) + STATE_SET_OPTS,
    ];

    const params = [
      "client_id=" + encodeURIComponent(CLIENT_ID),
      "response_type=code",
      "redirect_uri=" + encodeURIComponent(_redirectUri(r)),
      "scope=" + encodeURIComponent(OIDC_SCOPE),
      "state=" + stateHandle,
      "nonce=" + nonce,
      "response_mode=query",
      // Silent auth: CMS users already have an AAD session (same tenant), so prompt=none
      // yields a bare 302 (code, or error=login_required) with no UI to render — which is
      // also what lets it run framed (AD sets X-Frame-Options: DENY on any RENDERED
      // /authorize response). See the reference's notes.
      "prompt=none",
    ].join("&");

    r.return(302, _authorizeUrl() + "?" + params);
  } catch (e) {
    _degrade(r, null, landing, "entra-begin-error: " + String(e));
  }
}

// ---------------------------------------------------------------------------
// /init-v2/callback — code exchange, claims validation, store deposit, finalize.
// ---------------------------------------------------------------------------

async function handleInitEntraCallback(r) {
  let st = null;
  try {
    const adError = _arg(r, "error");
    if (adError) {
      // e.g. login_required (no silent AD session) — degrade, don't block login.
      st = _recoverState(r);
      _degrade(r, st, null, "ad-error: " + adError);
      return;
    }

    const raw = _cookie(r, STATE_COOKIE);
    if (!raw) {
      // No recoverable session data — best we can do is land on the fallback.
      _degrade(r, null, null, "missing-state");
      return;
    }
    try {
      st = _unpackState(raw);
    } catch (e) {
      _degrade(r, null, null, "bad-state");
      return;
    }

    if (_arg(r, "state") !== st.s) {
      _degrade(r, st, null, "state-mismatch");
      return;
    }
    const code = _arg(r, "code");
    if (!code) {
      _degrade(r, st, null, "no-code");
      return;
    }

    const tok = await _exchangeCode(code, _redirectUri(r));
    if (!tok.idToken) {
      _degrade(r, st, null, "token-exchange-failed: " + tok.diag);
      return;
    }
    const claims = _decodeJwtPayload(tok.idToken);
    if (!claims) {
      _degrade(r, st, null, "token-decode-failed");
      return;
    }
    const claimErr = _validateClaims(claims, st.n);
    if (claimErr) {
      _degrade(r, st, null, "claims-invalid: " + claimErr);
      return;
    }

    const oid = String(claims.oid || "");
    const email = String(
      claims.email || claims.upn || claims.preferred_username || "",
    );
    if (!oid) {
      _degrade(r, st, null, "no-oid");
      return;
    }

    // The main event: deposit through the swap-out seam.
    const dep = await store.deposit(
      oid,
      {
        cookies: st.cc,
        modernToken: st.tok,
        correlationId: st.corr,
        email: email,
      },
      { idToken: tok.idToken, accessToken: tok.accessToken },
    );
    if (!dep.ok) {
      _degrade(r, st, null, "store-deposit-failed: " + dep.diag);
      return;
    }

    _succeed(r, st);
  } catch (e) {
    _degrade(r, st, null, "callback-error: " + String(e));
  }
}

// Success terminal: clear state, then branch on mode. (The real idToken lands in the store
// deposit; drop2 no longer sets a browser-side id-token cookie — that presence-jsonp consumer
// was experimental and has been removed.)
function _succeed(r, st) {
  const cookies = [STATE_COOKIE + "=deleted" + STATE_CLEAR_OPTS];
  if (st.term === "iframe") {
    // Pure side-channel: store only, no Cms-Auth-Values, static page.
    r.headersOut["Set-Cookie"] = cookies;
    r.headersOut["Content-Type"] = "text/html; charset=utf-8";
    r.return(200, TERMINAL_HTML);
    return;
  }
  // Top-level: additive — drop1.finalize appends Cms-Auth-Values and 302s to the landing.
  r.headersOut["Set-Cookie"] = cookies;
  replaceDdei.finalize(r, _sessionFrom(st), { polarisUiUrl: st.ui, q: st.q });
}

// Best-effort degrade path. Never blocks the user's login: top-level still establishes
// Cms-Auth-Values + lands (plain drop1 behaviour); iframe just renders the terminal.
function _degrade(r, st, landingFallback, reason) {
  try {
    ngx.log(ngx.ERR, "drop2 entra degrade — " + reason);
  } catch (e) {
    // logging is best-effort
  }
  const clear = STATE_COOKIE + "=deleted" + STATE_CLEAR_OPTS;

  if (st && st.term === "iframe") {
    r.headersOut["Set-Cookie"] = [clear];
    r.headersOut["Content-Type"] = "text/html; charset=utf-8";
    r.return(200, TERMINAL_HTML);
    return;
  }
  if (st) {
    r.headersOut["Set-Cookie"] = [clear];
    replaceDdei.finalize(r, _sessionFrom(st), { polarisUiUrl: st.ui, q: st.q });
    return;
  }
  // No recoverable state. Prefer a captured landing (begin-time failure) if we have one.
  const ui = landingFallback && landingFallback.polarisUiUrl;
  r.return(302, ui || "/polaris-ui/");
}

// Recover state without throwing (used on the AD-error branch).
function _recoverState(r) {
  const raw = _cookie(r, STATE_COOKIE);
  if (!raw) return null;
  try {
    return _unpackState(raw);
  } catch (e) {
    return null;
  }
}

export default {
  handleInitEntra,
  handleInitEntraCallback,
  // exposed for the unit test (production only calls the two handlers):
  __test: {
    packState: _packState,
    unpackState: _unpackState,
    b64urlEncode: _b64urlEncode,
    b64urlDecode: _b64urlDecode,
    decodeJwtPayload: _decodeJwtPayload,
    validateClaims: _validateClaims,
    rand: _rand,
    authorizeUrl: _authorizeUrl,
    tokenUrl: _tokenUrl,
    constants: {
      STATE_COOKIE: STATE_COOKIE,
      TENANT_ID: TENANT_ID,
      TERMINAL_HTML: TERMINAL_HTML,
    },
  },
};
