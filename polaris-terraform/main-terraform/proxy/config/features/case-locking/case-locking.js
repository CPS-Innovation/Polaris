// Feature — case-locking (SignalR presence proxy for CMS case-locking).
//
// Ported from global-components infra/proxy/config/global-components.case-locking
// (.ts -> plain .js; behaviour unchanged). Two js filters that fix up the SignalR
// *negotiate* response so the browser connects SAME-ORIGIN (via our
// /global-components/case-locking/api/sr/ route) instead of cross-origin to
// *.service.signalr.net. All routing/CORS lives in case-locking.conf.
//
// NEW-GEN: these routes do not exist in the live monolith, so this feature is outside
// the golden master (it adds new paths; it touches no existing route).

function dropContentLengthForNegotiate(r) {
  // The body filter rewrites the negotiate JSON, which changes its byte length. nginx
  // computes the response Content-Length before the body filter runs, so the original
  // (upstream) length leaks through and the browser sees ERR_HTTP2_PROTOCOL_ERROR when
  // actual bytes < declared. Remove Content-Length here in the header-output phase;
  // HTTP/2 signals end-of-body via END_STREAM and doesn't require it.
  if (r.uri.endsWith("/negotiate")) {
    delete r.headersOut["Content-Length"];
  }
}

function filterNegotiateBody(r, data, flags) {
  // Only intercept SignalR negotiate responses. All other responses (including
  // WebSocket 101 upgrades and SSE/long-poll bodies) pass through unchanged.
  if (!r.uri.endsWith("/negotiate")) {
    r.sendBuffer(data, flags);
    return;
  }

  // Per-chunk regex replace (matching the Swagger body filter's pattern). This avoids
  // the buffer-then-emit-once approach which (under HTTP/2) can leave the client's
  // content-length expectation misaligned with the rewritten body and trigger
  // ERR_HTTP2_PROTOCOL_ERROR. Negotiate responses are small enough that the URL won't
  // span a chunk boundary in practice.
  //
  // Rewrite to an ABSOLUTE same-origin URL (not path-relative) because the SignalR
  // client constructs `new URL(response.url)` without supplying a base — a relative URL
  // there throws "Invalid URL".
  const scheme =
    r.headersIn["X-Forwarded-Proto"] || r.variables.scheme || "https";
  const host = r.headersIn["Host"] || r.variables.host;
  const replacement =
    scheme + "://" + host + "/global-components/case-locking/api/sr";

  const result = data.replace(
    /https?:\/\/[a-zA-Z0-9.-]+\.service\.signalr\.net/g,
    replacement,
  );
  r.sendBuffer(result, flags);
}

// ---------------------------------------------------------------------------
// Presence JSONP adapter — /global-components/presence-jsonp
//
// A thin adapter over the SAME presence API the SignalR routes above front
// (app-cms-presence-api). The injected CMS client reaches it cross-origin, which the
// IE Internet zone forbids for XHR — but NOT for <script src>. So the client fetches
// via JSONP (script tags) and this handler shims each GET into the backend's real REST
// call: it maps ?op= to POST/PUT/GET/DELETE, lifts the id-token out of the HttpOnly
// `cms-auth-id-token` cookie (set by auth-handover.drop2.entra — this is that cookie's
// consumer) into an Authorization header (token never in the URL), and wraps the JSON
// response as callback(...). The API keeps its pure REST form.
//
// Ported from global-components global-components.cms-auth-v2 (handlePresenceJsonp).
// ---------------------------------------------------------------------------

const _PRESENCE_API_BASE = "https://app-cms-presence-api.azurewebsites.net/api";

// DEV fallback: the static alg:none token the backend accepts today (the same one the
// relay sends). NOT secret (unsigned dev token). Once drop2's callback sets the id-token
// as an HttpOnly cookie on this path, that cookie takes precedence (see below); this just
// lets the flow round-trip before then. Remove before prod (QUIRKS CL7).
const _PRESENCE_DEV_BEARER =
  "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzb3VyY2VfYXBwbGljYXRpb24iOiJQb3N0bWFuIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiZGV2LnVzZXJAY3BzLmdvdi51ayJ9.";

const _PRESENCE_ID_TOKEN_COOKIE = "cms-auth-id-token";

// Token switch: prefer the id-token cookie over the dev-bearer fallback. ON — because
// auth-handover.drop2.entra deliberately writes the DEV token INTO that cookie today (its
// PRESENCE_COOKIE_TOKEN), so reading + sending the cookie proves the whole round-trip while the
// value stays one the backend accepts. When drop2 swaps the cookie to the real Entra id_token
// (once the backend validates them), this stays true and Just Works. If drop2 hasn't run, the
// _PRESENCE_DEV_BEARER fallback below still sends an accepted token.
const _PRESENCE_USE_ID_TOKEN = true;

// Browser can only GET; ?op= selects the backend's real verb/path/body.
const _PRESENCE_OPS = {
  create: {
    method: "POST",
    path: () => "/sessions",
    body: (a) => JSON.stringify({ sectionId: a.sectionId || "" }),
  },
  heartbeat: {
    method: "PUT",
    path: (a) => "/sessions/" + a.sid + "/heartbeat",
    body: () => null,
  },
  poll: {
    method: "GET",
    path: (a) => "/sessions/" + a.sid,
    body: () => null,
  },
  remove: {
    method: "DELETE",
    path: (a) => "/sessions/" + a.sid,
    body: () => null,
  },
};

function _arg(r, name) {
  const v = r.args[name];
  return v !== undefined ? v : "";
}

function _cookie(r, name) {
  const raw = r.headersIn["Cookie"];
  if (!raw) return "";
  const m = raw.match(new RegExp("(?:^|;\\s*)" + name + "=([^;]*)"));
  return m ? m[1] : "";
}

function _maybeDecode(v) {
  try {
    return decodeURIComponent(v);
  } catch (e) {
    return v;
  }
}

async function handlePresenceJsonp(r) {
  const cb = _arg(r, "callback");
  // The one non-negotiable JSONP guard: the callback name is reflected verbatim into an
  // executable script response, so it MUST be a bare identifier or it's XSS.
  if (!/^[A-Za-z_$][A-Za-z0-9_$]*$/.test(cb)) {
    r.headersOut["Content-Type"] = "text/plain; charset=utf-8";
    r.return(400, "invalid callback");
    return;
  }

  r.headersOut["Content-Type"] = "text/javascript; charset=utf-8";
  r.headersOut["Cache-Control"] = "no-store";

  const args = {
    op: _arg(r, "op"),
    sid: _arg(r, "sid"),
    sectionId: _maybeDecode(_arg(r, "sectionId")),
  };

  // hasOwnProperty guard so ?op=constructor/toString can't reach a prototype member.
  const op = Object.prototype.hasOwnProperty.call(_PRESENCE_OPS, args.op)
    ? _PRESENCE_OPS[args.op]
    : null;
  if (!op) {
    r.return(
      200,
      cb + "(" + JSON.stringify({ jsonpError: "unknown op: " + args.op }) + ")",
    );
    return;
  }

  // Which token goes to the backend. While _PRESENCE_USE_ID_TOKEN is false we send the static
  // dev bearer (the only thing the backend accepts today) even though drop2 sets the id-token
  // cookie. When flipped on, prefer the cookie's id-token (encodeURIComponent'd on set, so
  // decode it), falling back to the dev token if absent.
  let token = _PRESENCE_DEV_BEARER;
  if (_PRESENCE_USE_ID_TOKEN) {
    const cookieTok = _cookie(r, _PRESENCE_ID_TOKEN_COOKIE);
    if (cookieTok) {
      token = decodeURIComponent(cookieTok);
    }
  }

  try {
    const headers = { Authorization: "Bearer " + token };
    const fetchOpts = { method: op.method, headers: headers };
    const body = op.body(args);
    if (body) {
      headers["Content-Type"] = "application/json";
      fetchOpts.body = body;
    }

    const resp = await ngx.fetch(_PRESENCE_API_BASE + op.path(args), fetchOpts);
    const text = await resp.text();

    if (resp.status < 200 || resp.status >= 300) {
      // Raw JSONP has no error channel; give the browser callback one.
      r.return(
        200,
        cb +
          "(" +
          JSON.stringify({
            jsonpError: "upstream " + resp.status,
            upstreamBody: text,
          }) +
          ")",
      );
      return;
    }
    // text is already JSON (object for create, array for poll) — hand it back verbatim.
    r.return(200, cb + "(" + (text && text.length ? text : "{}") + ")");
  } catch (e) {
    r.return(200, cb + "(" + JSON.stringify({ jsonpError: String(e) }) + ")");
  }
}

export default {
  dropContentLengthForNegotiate,
  filterNegotiateBody,
  handlePresenceJsonp,
};
