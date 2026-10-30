#!/usr/bin/env node
/**
 * Unit tests for auth-handover.drop1.replace-ddei — the njs DDEI /api/init/ replacement.
 * ISOLATED / NEW-GEN: dormant unless NON_DDEI_INIT_ENABLED=true (or a per-user enrolment),
 * so it is outside the golden master. Its non-redirecting lookup is also exercised via
 * drop2's suite (which reuses it); this file pins drop1's own handler.
 *
 * Uses the same global `ngx.fetch` mock pattern as the drop2 suite.
 */
const { test, assertEqual, assert, assertIncludes, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, applyEnv } = require("../../../tests/unit/njs-harness")

let fetchImpl = async () => {
  throw new Error("no ngx.fetch stub set for this test")
}
let fetchCount = 0
global.ngx = {
  fetch: (...args) => {
    fetchCount++
    return fetchImpl(...args)
  },
  log: () => {},
  ERR: 4,
}

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


// R0: /init already looked the CMS session up (for the Entra deposit) and passes the token +
// version on, so /auth-refresh-inbound need not repeat discover/mint/verify.
async function passedTokenTests(drop1) {
  console.log("\nhandleInitNonDdei — token + version passed on from /init:")
  const TOKEN = "11111111-2222-3333-4444-555555555555"
  const cmsAuthValues = (r) => {
    const c = (r.headersOut["Set-Cookie"] || []).find((x) => x.indexOf("Cms-Auth-Values=") === 0)
    return JSON.parse(decodeURIComponent(c.slice("Cms-Auth-Values=".length).split(";")[0]))
  }

  await test("cms-modern-token + cms-version-id present -> no CMS calls; Cms-Auth-Values carries them", async () => {
    fetchCount = 0
    fetchImpl = async () => {
      throw new Error("must not call CMS")
    }
    const r = createMockRequest({
      args: { cc: "ASP.NET_SessionId=x", "polaris-ui-url": "/polaris-ui/case/1", "cms-modern-token": TOKEN, "cms-version-id": "CMS.25.1.02" },
      headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" },
    })
    await drop1.handleInitNonDdei(r)
    assertEqual(fetchCount, 0, "no discover / mint / verify")
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/case/1", "-> landing")
    const v = cmsAuthValues(r)
    assertEqual(v.token, TOKEN, "token")
    assertEqual(v.cmsVersionId, "CMS.25.1.02", "version")
    assertEqual(v.cookies, "ASP.NET_SessionId=x", "cookies")
  })

  await test("token without a version -> looks up as before", async () => {
    fetchCount = 0
    fetchImpl = establishRouter()
    const r = createMockRequest({
      args: { cc: "ASP.NET_SessionId=x", "polaris-ui-url": "/polaris-ui/", "cms-modern-token": TOKEN },
      headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" },
    })
    await drop1.handleInitNonDdei(r)
    assert(fetchCount > 0, "looked up")
    assertEqual(cmsAuthValues(r).token, "00000000-0000-0000-0000-000000000001", "the minted token, not the half-passed one")
  })

  await test("malformed token or version -> ignored, looks up as before", async () => {
    for (const bad of [
      { "cms-modern-token": "not-a-guid", "cms-version-id": "CMS.25.1.02" },
      { "cms-modern-token": TOKEN, "cms-version-id": "CMS.25\"},x" },
    ]) {
      fetchCount = 0
      fetchImpl = establishRouter()
      const r = createMockRequest({
        args: { cc: "ASP.NET_SessionId=x", "polaris-ui-url": "/polaris-ui/", ...bad },
        headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" },
      })
      await drop1.handleInitNonDdei(r)
      assert(fetchCount > 0, "looked up for " + JSON.stringify(bad))
      assertEqual(cmsAuthValues(r).token, "00000000-0000-0000-0000-000000000001", "minted token")
    }
  })

  await test("passed token but no cookies -> still the no-cookies fail-redirect", async () => {
    const r = createMockRequest({
      args: { "polaris-ui-url": "/polaris-ui/", "cms-modern-token": TOKEN, "cms-version-id": "CMS.25.1.02" },
      headersIn: { Host: "proxy.example" },
    })
    await drop1.handleInitNonDdei(r)
    assert(r.returnBody.indexOf("auth-fail-reason=no-cookies") !== -1, r.returnBody)
  })
}

// /init's non-redirecting lookup: the CMS half, reporting instead of redirecting.
async function lookupTests(drop1) {
  console.log("\nlookupCmsSession — non-redirecting (for /init):")

  await test("success -> {token, versionId}", async () => {
    fetchImpl = establishRouter()
    const out = await drop1.lookupCmsSession("proxy.example", "ASP.NET_SessionId=x")
    assertEqual(out.token, "00000000-0000-0000-0000-000000000001", "token")
    assertEqual(out.versionId, "CMS.24.0.01", "version")
  })

  await test("mint yields no token -> {token: '', reason}, never throws", async () => {
    fetchImpl = async (url) => {
      if (/\/CMS$/.test(url)) return res({ headers: { Location: "/CMS.24.0.01/x" } })
      return res({ body: "no token here" })
    }
    const out = await drop1.lookupCmsSession("proxy.example", "ASP.NET_SessionId=x")
    assertEqual(out.token, "", "no token")
    assertEqual(out.reason, "cms-auth-not-valid", "reason")
  })

  await test("verify fails -> {token: '', reason}", async () => {
    fetchImpl = async (url) => {
      if (url.indexOf("/graphql/") !== -1) return res({ status: 401 })
      return establishRouter()(url)
    }
    const out = await drop1.lookupCmsSession("proxy.example", "ASP.NET_SessionId=x")
    assertEqual(out.token, "", "no token")
    assertEqual(out.reason, "cms-modern-auth-not-valid", "reason")
  })

  await test("a network error -> {token: '', reason}, never throws", async () => {
    fetchImpl = async () => {
      throw new Error("boom")
    }
    const out = await drop1.lookupCmsSession("proxy.example", "ASP.NET_SessionId=x")
    assertEqual(out.token, "", "no token")
    assert(!!out.reason, "has a reason")
  })
}

// The one switch every DDEI-backed location reads (js_set $non_ddei).
async function enabledTests(drop1) {
  console.log("\nenabled — the drop1 switch ($non_ddei):")
  const SW = ["NON_DDEI_INIT_ENABLED", "ENTRA_STORE_ENABLED"]
  const withEnv = (env, fn) => {
    const restore = applyEnv(Object.fromEntries(SW.map((k) => [k, env[k] || ""])))
    try {
      return fn()
    } finally {
      restore()
    }
  }
  const on = (cookie) => drop1.enabled(createMockRequest({ headersIn: cookie ? { Cookie: cookie } : {} }))

  await test("off with neither switch nor enrolment", async () => {
    withEnv({}, () => assertEqual(on(""), "false", "off"))
    withEnv({}, () => assertEqual(on("a=1; polaris_auth_handover=drop9"), "false", "unknown mode is off"))
    withEnv({ NON_DDEI_INIT_ENABLED: "TRUE" }, () => assertEqual(on(""), "false", 'only the literal "true"'))
  })

  await test("on for either switch", async () => {
    withEnv({ NON_DDEI_INIT_ENABLED: "true" }, () => assertEqual(on(""), "true", "drop1 switch"))
    withEnv({ ENTRA_STORE_ENABLED: "true" }, () => assertEqual(on(""), "true", "Entra switch"))
  })

  await test("on for a drop1 or drop2 enrolment cookie, wherever it sits in the header", async () => {
    withEnv({}, () => {
      assertEqual(on("polaris_auth_handover=drop1"), "true", "drop1")
      assertEqual(on("a=1; polaris_auth_handover=drop2; b=2"), "true", "drop2, mid-header")
      assertEqual(on("xpolaris_auth_handover=drop1"), "false", "not a suffix match")
    })
  })
}

// DDEI InSituReauthTermination: the JSONP end of the UI's in-situ reauth.
async function terminationTests(drop1) {
  console.log("\nhandleTerminationNonDdei — /auth-refresh-termination:")
  const call = (args) => {
    const r = createMockRequest({ args })
    drop1.handleTerminationNonDdei(r)
    return r
  }

  await test("no auth-fail-reason -> cb({isSuccess: true, failReason: \"\" }), DDEI's exact shape", async () => {
    const r = call({ cb: "jsonp_1730000000000_4242" })
    assertEqual(r.returnCode, 200, "200")
    assertEqual(r.returnBody, 'jsonp_1730000000000_4242({isSuccess: true, failReason: "" })', "body")
    assertEqual(r.headersOut["Content-Type"], "text/javascript; charset=utf-8", "script")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "non-ddei", "marker")
  })

  await test("an auth-fail-reason -> isSuccess false + the reason", async () => {
    const r = call({ cb: "cb", "auth-fail-reason": "cms-auth-not-valid" })
    assertEqual(r.returnBody, 'cb({isSuccess: false, failReason: "cms-auth-not-valid" })', "body")
  })

  await test("a whitespace-only reason is success (DDEI IsNullOrWhiteSpace)", async () => {
    assertEqual(call({ cb: "cb", "auth-fail-reason": "  " }).returnBody, 'cb({isSuccess: true, failReason: "  " })', "body")
  })

  await test("hardened: the reason is a JSON string, a non-identifier cb is refused", async () => {
    assertEqual(call({ cb: "cb", "auth-fail-reason": 'x"});alert(1)//' }).returnBody, 'cb({isSuccess: false, failReason: "x\\"});alert(1)//" })', "escaped")
    for (const cb of ["", "alert(1);cb", "1abc", "a b"]) {
      assertEqual(call({ cb }).returnCode, 400, "400 for " + JSON.stringify(cb))
    }
  })
}

// DDEI GetModernToken: r + cc -> 302 to r with the incoming args (minus r) + cms-modern-token.
async function cmsModernTokenTests(drop1) {
  console.log("\nhandleCmsModernTokenNonDdei — /auth-refresh-cms-modern-token:")
  const TOKEN = "00000000-0000-0000-0000-000000000001"
  const call = async (args) => {
    const r = createMockRequest({ args, headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" } })
    await drop1.handleCmsModernTokenNonDdei(r)
    return r
  }
  const OS = "https://os.example/Casework_Patterns/auth-handover.html?src=x&stage=os-token-return"

  await test("302 to r + every other arg + cms-modern-token (looked up), marked non-ddei", async () => {
    fetchImpl = establishRouter()
    const r = await call({ r: OS, cc: "a=1; b=2", extra: "y z" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, OS + "&cc=a%3D1%3B%20b%3D2&extra=y%20z&cms-modern-token=" + TOKEN, "Location")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "non-ddei", "marker")
  })

  await test("r without a query gets ?; a #fragment stays last", async () => {
    fetchImpl = establishRouter()
    assertEqual((await call({ r: "https://os.example/p#frag", cc: "a=1" })).returnBody, "https://os.example/p?cc=a%3D1&cms-modern-token=" + TOKEN + "#frag", "Location")
  })

  await test("DDEI's 400s: no r, no cc, an incoming cms-modern-token (a loop)", async () => {
    fetchImpl = async () => {
      throw new Error("must not call CMS")
    }
    assertEqual((await call({ cc: "a=1" })).returnCode, 400, "no r")
    assertEqual((await call({ r: OS })).returnCode, 400, "no cc")
    assertEqual((await call({ r: OS, cc: "a=1", "cms-modern-token": TOKEN })).returnCode, 400, "loop")
  })

  await test("lookup fails -> 500 with the reason, no redirect", async () => {
    fetchImpl = async (url) => (url.indexOf("/graphql/") !== -1 ? res({ status: 401 }) : establishRouter()(url))
    const r = await call({ r: OS, cc: "a=1" })
    assertEqual(r.returnCode, 500, "500")
    assertIncludes(r.returnBody, "cms-modern-auth-not-valid", "reason")
  })
}

async function main() {
  const restore = applyEnv({ WEBSITE_SCHEME: "https", ENDPOINT_HTTP_PROTOCOL: "https" })
  const drop1 = await loadNjs("features/auth-handover.drop1.replace-ddei/auth-handover.drop1.replace-ddei.js")
  await markerTests(drop1)
  await absoluteUrlTests(drop1)
  await passedTokenTests(drop1)
  await lookupTests(drop1)
  await enabledTests(drop1)
  await terminationTests(drop1)
  await cmsModernTokenTests(drop1)
  restore()
  process.exit(summarise("auth-handover.drop1.replace-ddei (unit)"))
}

main()
