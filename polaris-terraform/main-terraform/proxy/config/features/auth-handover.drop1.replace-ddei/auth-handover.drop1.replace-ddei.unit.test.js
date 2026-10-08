#!/usr/bin/env node
/**
 * Unit tests for auth-handover.drop1.replace-ddei — the njs DDEI /api/init/ replacement.
 * ISOLATED / NEW-GEN: dormant unless NON_DDEI_INIT_ENABLED=true (or a per-user enrolment),
 * so it is outside the golden master. Its establish/finalize core is also exercised via
 * drop2's suite (which reuses it); this file pins drop1's own handler.
 *
 * Uses the same global `ngx.fetch` mock pattern as the drop2 suite.
 */
const { test, assertEqual, assert, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, applyEnv } = require("../../../tests/unit/njs-harness")

let fetchImpl = async () => {
  throw new Error("no ngx.fetch stub set for this test")
}
global.ngx = { fetch: (...args) => fetchImpl(...args), log: () => {}, ERR: 4 }

function res({ status = 200, body = "", headers = {}, url = "" }) {
  return {
    ok: status >= 200 && status < 300,
    status,
    url,
    headers: { get: (k) => (headers[k] !== undefined ? headers[k] : null) },
    text: async () => body,
  }
}

// drop1's mint + verify loopbacks, all succeeding.
function establishRouter() {
  return async (url) => {
    if (/\/CMS$/.test(url)) return res({ headers: { Location: "/CMS.24.0.01/x" } })
    if (url.indexOf("uainGeneratedScript") !== -1)
      return res({ body: "var SESS_MODERN_USER_SESSION_ID = '00000000-0000-0000-0000-000000000001';" })
    if (url.indexOf("/graphql/") !== -1) return res({ body: JSON.stringify({ data: { user: { partyId: 1 } } }) })
    throw new Error("establishRouter: unexpected url " + url)
  }
}

async function markerTests(drop1) {
  console.log("\nhandleInitNonDdei — X-Polaris-Auth-Init marker:")

  await test("success: 302 to the landing with Cms-Auth-Values, marked non-ddei", async () => {
    fetchImpl = establishRouter()
    const r = createMockRequest({
      args: { cc: "ASP.NET_SessionId=x; .CMSAUTHa=y", "polaris-ui-url": "/polaris-ui/case/1" },
      headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" },
    })
    await drop1.handleInitNonDdei(r)
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/case/1", "-> landing, absolute https")
    assert((r.headersOut["Set-Cookie"] || []).some((c) => c.indexOf("Cms-Auth-Values=") === 0), "Cms-Auth-Values set")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "non-ddei", "marker")
  })

  await test("fail-redirect (no cookies) is marked non-ddei too", async () => {
    fetchImpl = establishRouter()
    const r = createMockRequest({ args: { "polaris-ui-url": "/polaris-ui/" }, headersIn: { Host: "proxy.example" } })
    await drop1.handleInitNonDdei(r)
    assertEqual(r.returnCode, 302, "302")
    assert(r.returnBody.indexOf("https://proxy.example/polaris-ui/?auth-fail-reason=no-cookies") === 0, "absolute fail-redirect: " + r.returnBody)
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "non-ddei", "marker")
  })
}

// njs would build a relative Location with nginx's own (http) scheme behind the TLS front.
async function absoluteUrlTests(drop1) {
  console.log("\nabsoluteUrl — redirects never fall back to http behind the TLS front:")
  const req = (headersIn) => createMockRequest({ headersIn })

  await test("relative path -> forwarded proto + Host", async () => {
    assertEqual(drop1.absoluteUrl(req({ Host: "h.example", "X-Forwarded-Proto": "https" }), "/polaris-ui/x?a=1"), "https://h.example/polaris-ui/x?a=1", "abs")
  })

  await test("no X-Forwarded-Proto -> https (never nginx's own http)", async () => {
    assertEqual(drop1.absoluteUrl(req({ Host: "h.example" }), "/p"), "https://h.example/p", "https default")
  })

  await test("an explicit http proto is honoured (local docker)", async () => {
    assertEqual(drop1.absoluteUrl(req({ Host: "localhost:8080", "X-Forwarded-Proto": "http" }), "/p"), "http://localhost:8080/p", "http")
  })

  await test("absolute URLs pass through unchanged", async () => {
    assertEqual(drop1.absoluteUrl(req({ Host: "h.example", "X-Forwarded-Proto": "https" }), "https://other.example/x"), "https://other.example/x", "unchanged")
  })
}

async function main() {
  const restore = applyEnv({ WEBSITE_SCHEME: "https", ENDPOINT_HTTP_PROTOCOL: "https" })
  const drop1 = await loadNjs("features/auth-handover.drop1.replace-ddei/auth-handover.drop1.replace-ddei.js")
  await markerTests(drop1)
  await absoluteUrlTests(drop1)
  restore()
  process.exit(summarise("auth-handover.drop1.replace-ddei (unit)"))
}

main()
