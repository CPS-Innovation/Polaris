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
  const scheme = r.headersIn["X-Forwarded-Proto"] || r.variables.scheme || "https";
  const host = r.headersIn["Host"] || r.variables.host;
  const replacement = scheme + "://" + host + "/global-components/case-locking/api/sr";

  const result = data.replace(
    /https?:\/\/[a-zA-Z0-9.-]+\.service\.signalr\.net/g,
    replacement,
  );
  r.sendBuffer(result, flags);
}

export default {
  dropContentLengthForNegotiate,
  filterNegotiateBody,
};
