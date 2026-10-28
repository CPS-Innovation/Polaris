#!/usr/bin/env node
/**
 * Unit tests for dev-login — DDEI's /api/login and /api/login-full-cookie in njs (behind the
 * drop1 switch). The CMS login loopbacks are stubbed through a global `ngx.fetch`, as the drop1
 * and drop2 suites do. Pins the contract polaris-e2e and shared.integration-tests rely on.
 */
const { test, assertEqual, assert, assertIncludes, assertNotIncludes, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("../../../tests/unit/njs-harness")

let fetchImpl = async () => {
  throw new Error("no ngx.fetch stub set for this test")
}
let calls = []
global.ngx = {
  fetch: (url, opts) => {
    calls.push({ url, opts })
    return fetchImpl(url, opts)
  },
  log: () => {},
  ERR: 4,
}

function res({ status = 200, body = "", headers = {}, setCookies = [] }) {
  return {
    ok: status >= 200 && status < 300,
    status,
    headers: {
      get: (k) => (headers[k] !== undefined ? headers[k] : null),
      getAll: (k) => (k === "Set-Cookie" ? setCookies : []),
    },
    text: async () => body,
  }
}

const TOKEN = "00000000-0000-0000-0000-000000000001"
const LB = "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN2-cin2.cps.gov.uk_POOL=123; path=/; HttpOnly"

// CMS: /CMS redirects to the version and sets the LB cookie; the login sets .CMSAUTH; the
// modern-token mint + verify (drop1's lookup) succeed.
function cmsRouter({ loginCookies = [".CMSAUTH=auth; path=/; HttpOnly", "ASP.NET_SessionId=s; path=/; HttpOnly"] } = {}) {
  return async (url) => {
    if (/\/CMS$/.test(url)) return res({ status: 302, headers: { Location: "/CMS.25.1.02/" }, setCookies: [LB] })
    if (url.indexOf("/User/uaulLogin.aspx") !== -1) return res({ setCookies: loginCookies })
    if (url.indexOf("uainGeneratedScript") !== -1) return res({ body: "var SESS_MODERN_USER_SESSION_ID = '" + TOKEN + "';" })
    if (url.indexOf("/graphql/") !== -1) return res({ body: JSON.stringify({ data: { user: { partyId: 1 } } }) })
    throw new Error("cmsRouter: unexpected url " + url)
  }
}

function post(body, host = "polaris-dev-notprod.cps.gov.uk") {
  const r = createMockRequest({ method: "POST", headersIn: { Host: host, "X-Forwarded-Proto": "https" } })
  r.requestText = body
  return r
}

async function formTests(dev) {
  console.log("\nGET — the login form:")

  await test("both routes: 200 form with DDEI's field names, the env list, and the cookie reset", async () => {
    for (const h of [dev.devLogin, dev.devLoginFullCookie]) {
      const r = createMockRequest({ headersIn: { Host: "h" } })
      await h(r)
      assertEqual(r.returnCode, 200, "200")
      for (const needle of ['name="username"', 'name="password"', 'type="submit"', 'name="selected-environment"', '<option value="" selected>Default - cms.cps.gov.uk</option>', '<option value="cin2">', '<option value="cin5">']) {
        assertIncludes(r.returnBody, needle, needle)
      }
      const sc = r.headersOut["Set-Cookie"].join("\n")
      assertIncludes(sc, "__CMSENV=deleted", "clears the env marker")
      assertIncludes(sc, "F-CIN5-LBsessioncookie=deleted", "clears an LB cookie")
      assertEqual(r.headersOut["Cache-Control"], "no-store", "no-store")
    }
  })

  await test("an env with no upstream setting is not offered", async () => {
    const restore = applyEnv({ CIN4_UPSTREAM_CMS_DOMAIN_NAME: "" })
    try {
      const r = createMockRequest({ headersIn: { Host: "h" } })
      await dev.devLogin(r)
      assertNotIncludes(r.returnBody, '"cin4"', "no cin4")
    } finally {
      restore()
    }
  })
}

async function loginTests(dev) {
  console.log("\nPOST /dev-login/ — CMS login, cookies to the browser, links into the apps:")

  await test("DDEI's CMS login: GET /CMS with DEVLOGIN, then the credentials POSTed to the discovered version with the LB cookie", async () => {
    calls = []
    fetchImpl = cmsRouter()
    // Built, not literal (keeps secret scanners quiet): a "+" for a space, an "&" in the value.
    const user = ["fake", "user"]
    const pw = ["fake", "pw"]
    await dev.devLogin(post("username=" + user.join("+") + "&" + "password=" + pw.join("%26") + "&selected-environment=cin2"))
    assertEqual(calls.length, 2, "two loopbacks")
    assertEqual(calls[0].url, "https://polaris-dev-notprod.cps.gov.uk/CMS", "home route on this host")
    assertEqual(calls[0].opts.headers.Cookie, "DEVLOGIN=cin2", "env cookie")
    assertEqual(calls[1].url, "https://polaris-dev-notprod.cps.gov.uk/CMS.25.1.02/User/uaulLogin.aspx", "login at the version")
    assertEqual(calls[1].opts.method, "POST", "POST")
    assertIncludes(calls[1].opts.headers.Cookie, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN2-cin2.cps.gov.uk_POOL=123", "LB cookie, no attributes")
    assertNotIncludes(calls[1].opts.headers.Cookie, "HttpOnly", "pairs only")
    assertEqual(calls[1].opts.body, "txtYLoginName=" + user.join("%20") + "&txtNPassword=" + pw.join("%26") + "&hidIsPostBack=1&hidNActXSpellCheckExists=false", "DDEI's form")
  })

  await test("success: CMS cookies + LB cookie + __CMSENV, login-ok, P / M / C links (C for this proxy host)", async () => {
    fetchImpl = cmsRouter()
    const r = post("username=jo&password=x&selected-environment=cin2")
    await dev.devLogin(r)
    assertEqual(r.returnCode, 200, "200")
    const sc = r.headersOut["Set-Cookie"]
    assertEqual(sc[0], ".CMSAUTH=auth; path=/; HttpOnly", "CMS cookies as CMS set them")
    assert(sc.indexOf(LB) !== -1, "the /CMS LB cookie, as the login set none")
    assertEqual(sc[sc.length - 1], "__CMSENV=cin2; path=/", "env marker last")
    assertIncludes(r.returnBody, "data-testid=\"login-ok\"", "e2e's locator")
    assertIncludes(r.returnBody, 'href="/polaris"', "P")
    assertIncludes(r.returnBody, 'href="/polaris?polaris-ui-url=/materials-ui"', "M")
    assertIncludes(r.returnBody, 'href="/launch/dev-proxy"', "C, dev")
    assertIncludes(r.returnBody, "<pre>.CMSAUTH=auth; ASP.NET_SessionId=s; BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN2-cin2.cps.gov.uk_POOL=123</pre>", "the full Cookie header")
    assertIncludes(r.returnBody, ".CMSAUTH=auth; path=/; HttpOnly\nASP.NET_SessionId=s; path=/; HttpOnly", "each Set-Cookie in full")
  })

  await test("C follows the host: QA -> a QA -proxy key; an unknown host -> no C link", async () => {
    fetchImpl = cmsRouter()
    let r = post("username=jo&password=x", "polaris-qa-notprod.cps.gov.uk")
    await dev.devLogin(r)
    assertIncludes(r.returnBody, 'href="/launch/cin2-proxy"', "QA")
    r = post("username=jo&password=x", "localhost:8080")
    await dev.devLogin(r)
    assertNotIncludes(r.returnBody, "/launch/", "no C")
    assertIncludes(r.returnBody, 'href="/polaris"', "still P")
  })

  await test("blank env = default: DEVLOGIN= and __CMSENV=default", async () => {
    calls = []
    fetchImpl = cmsRouter()
    const r = post("username=jo&password=x")
    await dev.devLogin(r)
    assertEqual(calls[0].opts.headers.Cookie, "DEVLOGIN=", "blank env")
    assertEqual(r.headersOut["Set-Cookie"].slice(-1)[0], "__CMSENV=default; path=/", "default")
  })

  await test("a newer LB cookie from the login is kept, the /CMS one not added", async () => {
    const newer = "BIGipServer~x=999; path=/"
    fetchImpl = cmsRouter({ loginCookies: [".CMSAUTH=a; path=/", newer] })
    const r = post("username=jo&password=x")
    await dev.devLogin(r)
    assert(r.headersOut["Set-Cookie"].indexOf(newer) !== -1, "newer kept")
    assert(r.headersOut["Set-Cookie"].indexOf(LB) === -1, "old not added")
  })

  await test("no .CMSAUTH -> ARE NOT logged in, no app links", async () => {
    fetchImpl = cmsRouter({ loginCookies: ["ASP.NET_SessionId=s; path=/"] })
    const r = post("username=jo&password=wrong")
    await dev.devLogin(r)
    assertIncludes(r.returnBody, "ARE NOT", "not logged in")
    assertNotIncludes(r.returnBody, "login-ok", "no ok marker")
    assertNotIncludes(r.returnBody, 'href="/polaris"', "no links")
  })

  await test("400 without a user name, or for an env not offered (DEVLOGIN never arbitrary); no CMS call", async () => {
    calls = []
    fetchImpl = cmsRouter()
    for (const body of ["password=x", "username=jo&selected-environment=cin9", "username=jo&selected-environment=cin2%3B%20x%3Dy"]) {
      const r = post(body)
      await dev.devLogin(r)
      assertEqual(r.returnCode, 400, "400 for " + body)
    }
    assertEqual(calls.length, 0, "no CMS call")
  })

  await test("CMS unreachable -> 502", async () => {
    fetchImpl = async () => {
      throw new Error("boom")
    }
    const r = post("username=jo&password=x")
    await dev.devLogin(r)
    assertEqual(r.returnCode, 502, "502")
  })
}

async function fullCookieTests(dev) {
  console.log("\nPOST /api/dev-login-full-cookie/ — Cms-Auth-Values for the gateway:")
  const authValues = (sc) => JSON.parse(decodeURIComponent(sc.split(";")[0].slice("Cms-Auth-Values=".length)))

  await test("Cms-Auth-Values is the FIRST Set-Cookie (shared.integration-tests reads it), from the login's cookies + minted token", async () => {
    fetchImpl = cmsRouter()
    const r = post("username=jo&password=x&selected-environment=cin2")
    await dev.devLoginFullCookie(r)
    assertEqual(r.returnCode, 200, "200")
    const sc = r.headersOut["Set-Cookie"]
    assert(sc[0].indexOf("Cms-Auth-Values=") === 0, "first: " + sc[0].slice(0, 40))
    assertIncludes(sc[0], "Path=/api/", "gateway path")
    const v = authValues(sc[0])
    assertEqual(v.token, TOKEN, "token")
    assertIncludes(v.cookies, ".CMSAUTH=auth", "CMS cookies")
    assertIncludes(v.cookies, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN2", "LB cookie, so the gateway routes to cin2")
    assertEqual(sc[1], "__CMSENV=cin2; path=/", "env marker")
    assertEqual(sc.length, 2, "the CMS cookies themselves are not handed to the browser (as DDEI)")
    assertIncludes(r.returnBody, 'href="/viewer/landing#/dashboard/' + TOKEN + '"', "Open Modern")
  })

  await test("login failed -> 401, no cookies", async () => {
    fetchImpl = cmsRouter({ loginCookies: [] })
    const r = post("username=jo&password=wrong")
    await dev.devLoginFullCookie(r)
    assertEqual(r.returnCode, 401, "401")
    assertEqual(r.headersOut["Set-Cookie"], undefined, "no cookies")
  })

  await test("no modern token -> 502 with the reason", async () => {
    const base = cmsRouter()
    fetchImpl = async (url, o) => (url.indexOf("/graphql/") !== -1 ? res({ status: 401 }) : base(url, o))
    const r = post("username=jo&password=x")
    await dev.devLoginFullCookie(r)
    assertEqual(r.returnCode, 502, "502")
    assertIncludes(r.returnBody, "cms-modern-auth-not-valid", "reason")
  })
}

async function main() {
  // app-launch (imported for the C link) reads DEFAULT_UPSTREAM_CMS_DOMAIN_NAME at load.
  const restore = applyEnv(cmsEnvObject())
  const dev = await loadNjs("features/dev-login/dev-login.js")
  await formTests(dev)
  await loginTests(dev)
  await fullCookieTests(dev)
  restore()
  process.exit(summarise("dev-login (unit)"))
}

main()
