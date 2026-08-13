#!/usr/bin/env node
/**
 * Unit tests for the case-locking feature (case-locking.js) — the two SignalR negotiate
 * filters. Ported from the global-components reference test (esbuild+ts) to this repo's
 * plain-js njs-harness. NEW-GEN: these routes are not in the live monolith, so this is
 * outside the golden master.
 */
const { test, assertEqual, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest } = require("../../../tests/unit/njs-harness")

const SR_URL =
  "https://sr-cms-presence.service.signalr.net/client/?hub=sectionsessionhub&asrs.op=%2Fsection-view"
const SAME_ORIGIN =
  "https://proxy.example.com/global-components/case-locking/api/sr/client/?hub=sectionsessionhub&asrs.op=%2Fsection-view"

// Mirror the reference's request factory: Host + variables.scheme/host defaulted.
function req(uri, opts = {}) {
  return createMockRequest({
    uri,
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

async function main() {
  const cl = await loadNjs("features/case-locking/case-locking.js")
  await dropContentLength(cl)
  await filterBody(cl)
  process.exit(summarise("case-locking.js (unit)"))
}

main()
