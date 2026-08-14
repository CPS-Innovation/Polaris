#!/usr/bin/env node
/**
 * Unit tests for the case-locking feature (case-locking.js) — the two SignalR negotiate
 * filters. Ported from the global-components reference test (esbuild+ts) to this repo's
 * plain-js njs-harness. NEW-GEN: these routes are not in the live monolith, so this is
 * outside the golden master.
 */
const { test, assert, assertEqual, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest } = require("../../../tests/unit/njs-harness")

// ngx.fetch mock for handlePresenceJsonp (the filters don't use ngx). Swap fetchImpl per test.
let fetchImpl = async () => {
  throw new Error("no ngx.fetch stub set for this test")
}
global.ngx = { fetch: (...args) => fetchImpl(...args), log: () => {}, ERR: 4 }
const presRes = (status, body) => ({ status, text: async () => body })

const SR_URL =
  "https://sr-cms-presence.service.signalr.net/client/?hub=sectionsessionhub&asrs.op=%2Fsection-view"
const SAME_ORIGIN =
  "https://proxy.example.com/global-components/case-locking/api/sr/client/?hub=sectionsessionhub&asrs.op=%2Fsection-view"

// Mirror the reference's request factory: Host + variables.scheme/host defaulted.
function req(uri, opts = {}) {
  return createMockRequest({
    uri,
    args: opts.args || {},
    headersIn: { Host: "proxy.example.com", ...(opts.headersIn || {}) },
    headersOut: { ...(opts.headersOut || {}) },
    variables: { scheme: "https", host: "proxy.example.com", ...(opts.variables || {}) },
  })
}

async function dropContentLength(cl) {
  console.log("\ndropContentLengthForNegotiate:")

  await test("strips Content-Length on negotiate responses", () => {
    const r = req("/global-components/case-locking/api/section-view/negotiate", {
      headersOut: { "Content-Length": "601" },
    })
    cl.dropContentLengthForNegotiate(r)
    assertEqual(r.headersOut["Content-Length"], undefined, "removed")
  })

  await test("leaves Content-Length alone on non-negotiate responses", () => {
    const r = req("/global-components/case-locking/api/section-view", {
      headersOut: { "Content-Length": "100" },
    })
    cl.dropContentLengthForNegotiate(r)
    assertEqual(r.headersOut["Content-Length"], "100", "preserved")
  })
}

async function filterBody(cl) {
  console.log("\nfilterNegotiateBody:")

  await test("rewrites the SignalR Service URL on a negotiate response", () => {
    const r = req("/global-components/case-locking/api/section-view/negotiate")
    cl.filterNegotiateBody(r, JSON.stringify({ url: SR_URL, accessToken: "the-token" }), { last: true })
    const out = JSON.parse(r.sentBuffer)
    assertEqual(out.url, SAME_ORIGIN, "rewritten to absolute same-origin URL")
    assertEqual(out.accessToken, "the-token", "accessToken untouched")
  })

  await test("uses X-Forwarded-Proto for scheme when present (over r.variables.scheme)", () => {
    const r = req("/global-components/case-locking/api/section-view/negotiate", {
      headersIn: { Host: "proxy.example.com", "X-Forwarded-Proto": "https" },
      variables: { scheme: "http", host: "proxy.example.com" },
    })
    cl.filterNegotiateBody(r, JSON.stringify({ url: "https://sr-cms-presence.service.signalr.net/client/?hub=h" }), {
      last: true,
    })
    assertEqual(
      JSON.parse(r.sentBuffer).url,
      "https://proxy.example.com/global-components/case-locking/api/sr/client/?hub=h",
    )
  })

  await test("leaves non-SignalR-Service negotiate URLs alone", () => {
    const r = req("/global-components/case-locking/api/section-view/negotiate")
    const body = JSON.stringify({ url: "https://something-else.example.com/client/?hub=foo", accessToken: "x" })
    cl.filterNegotiateBody(r, body, { last: true })
    assertEqual(JSON.parse(r.sentBuffer).url, "https://something-else.example.com/client/?hub=foo", "unchanged")
  })

  await test("passes non-negotiate URIs straight through", () => {
    const r = req("/global-components/case-locking/api/section-view")
    const body = '{"some":"payload"}'
    cl.filterNegotiateBody(r, body, { last: true })
    assertEqual(r.sentBuffer, body, "unchanged")
  })

  await test("emits each chunk after replacement (no buffering)", () => {
    const r = req("/global-components/case-locking/api/section-view/negotiate")
    cl.filterNegotiateBody(r, '{"url":"https://sr-cms-presence.service.signalr.net/client/?hub=h"', { last: false })
    assertEqual(
      r.sentBuffer,
      '{"url":"https://proxy.example.com/global-components/case-locking/api/sr/client/?hub=h"',
      "rewritten chunk emitted immediately",
    )
  })

  await test("passes through bytes that don't contain a SignalR Service URL", () => {
    const r = req("/global-components/case-locking/api/section-view/negotiate")
    const body = "not json at all"
    cl.filterNegotiateBody(r, body, { last: true })
    assertEqual(r.sentBuffer, body, "unchanged")
  })
}

async function presenceJsonp(cl) {
  console.log("\nhandlePresenceJsonp — JSONP adapter over the presence API:")
  const PJ = "/global-components/presence-jsonp"
  const withCookie = { Cookie: "cms-auth-id-token=THE-TOKEN" }

  await test("rejects a non-identifier callback (XSS guard) -> 400 text/plain", async () => {
    const r = req(PJ, { args: { callback: "alert(1)", op: "poll", sid: "s1" } })
    await cl.handlePresenceJsonp(r)
    assertEqual(r.returnCode, 400, "400")
    assertEqual(r.returnBody, "invalid callback", "no script emitted")
  })

  await test("unknown op -> jsonpError wrapped in the callback (200 text/javascript)", async () => {
    const r = req(PJ, { args: { callback: "cb", op: "bogus" }, headersIn: withCookie })
    await cl.handlePresenceJsonp(r)
    assertEqual(r.returnCode, 200, "200")
    assertEqual(r.headersOut["Content-Type"], "text/javascript; charset=utf-8", "js content-type")
    assertEqual(r.returnBody, 'cb(' + JSON.stringify({ jsonpError: "unknown op: bogus" }) + ')')
  })

  await test("create -> POST /sessions with sectionId body; sends the id-token COOKIE value as Bearer (_PRESENCE_USE_ID_TOKEN=true)", async () => {
    let captured
    fetchImpl = async (url, opts) => {
      captured = { url, opts }
      return presRes(200, '{"sid":"new"}')
    }
    const r = req(PJ, { args: { callback: "cb", op: "create", sectionId: "sec-1" }, headersIn: withCookie })
    await cl.handlePresenceJsonp(r)
    assertEqual(captured.url, "https://app-cms-presence-api.azurewebsites.net/api/sessions", "url")
    assertEqual(captured.opts.method, "POST", "POST")
    // Switch on: the cookie value (drop2 writes the dev token here today) is decoded + sent —
    // this is the path that proves the round-trip. In prod drop2 later writes the real id_token.
    assertEqual(captured.opts.headers.Authorization, "Bearer THE-TOKEN", "Bearer from the cookie")
    assertEqual(JSON.parse(captured.opts.body).sectionId, "sec-1", "sectionId body")
    assertEqual(r.returnBody, 'cb({"sid":"new"})', "wraps upstream JSON verbatim")
  })

  await test("heartbeat -> PUT /sessions/<sid>/heartbeat, no body", async () => {
    let captured
    fetchImpl = async (url, opts) => {
      captured = { url, opts }
      return presRes(200, "{}")
    }
    const r = req(PJ, { args: { callback: "cb", op: "heartbeat", sid: "S9" }, headersIn: withCookie })
    await cl.handlePresenceJsonp(r)
    assertEqual(captured.url, "https://app-cms-presence-api.azurewebsites.net/api/sessions/S9/heartbeat", "url")
    assertEqual(captured.opts.method, "PUT", "PUT")
    assertEqual(captured.opts.body, undefined, "no body")
  })

  await test("no id-token cookie -> dev bearer fallback", async () => {
    let captured
    fetchImpl = async (url, opts) => {
      captured = { url, opts }
      return presRes(200, "[]")
    }
    const r = req(PJ, { args: { callback: "cb", op: "poll", sid: "S1" } })
    await cl.handlePresenceJsonp(r)
    assert(captured.opts.headers.Authorization.indexOf("Bearer eyJ") === 0, "dev bearer used")
    assertEqual(r.returnBody, "cb([])", "wraps array verbatim")
  })

  await test("upstream non-2xx -> jsonpError with status + upstream body", async () => {
    fetchImpl = async () => presRes(503, "down")
    const r = req(PJ, { args: { callback: "cb", op: "poll", sid: "S1" }, headersIn: withCookie })
    await cl.handlePresenceJsonp(r)
    assertEqual(r.returnBody, 'cb(' + JSON.stringify({ jsonpError: "upstream 503", upstreamBody: "down" }) + ')')
  })

  await test("empty upstream body -> cb({})", async () => {
    fetchImpl = async () => presRes(200, "")
    const r = req(PJ, { args: { callback: "cb", op: "poll", sid: "S1" }, headersIn: withCookie })
    await cl.handlePresenceJsonp(r)
    assertEqual(r.returnBody, "cb({})")
  })

  await test("fetch throw -> jsonpError in the callback (never a blank 500)", async () => {
    fetchImpl = async () => {
      throw new Error("boom")
    }
    const r = req(PJ, { args: { callback: "cb", op: "poll", sid: "S1" }, headersIn: withCookie })
    await cl.handlePresenceJsonp(r)
    assert(r.returnBody.indexOf('cb({"jsonpError":') === 0, "wrapped error, callback preserved")
  })
}

async function main() {
  const cl = await loadNjs("features/case-locking/case-locking.js")
  await dropContentLength(cl)
  await filterBody(cl)
  await presenceJsonp(cl)
  process.exit(summarise("case-locking.js (unit)"))
}

main()
