#!/usr/bin/env node
/**
 * Unit tests for the auth-handover.drop2.entra feature (store.js, the /init begin step, the callback).
 * R0 (2026-10-16): the Entra step runs in /init for EVERY handover destination; the callback then
 * carries on exactly as /init would have (302 to r with cc), plus the CMS token for our own
 * /auth-refresh-inbound and for OS handover pages.
 * ISOLATED / NEW-GEN: this path is dormant unless drop2 is armed (ENTRA_STORE_ENABLED=true or a
 * per-user enrolment), so it is outside the golden master. These tests pin the Entra flow's own
 * behaviour: ONE neutral sign-in (openid profile offline_access), then a refresh-token swap per
 * service — MDS for the deposit, and (iframe only) the watchdog for the presence token cookies.
 *
 * Drives `ngx.fetch` through a global `ngx` with a swappable router (the modules read the free
 * `ngx` global, which in node resolves to `global`).
 *
 * Loaded from the REAL config sources via njs-harness (which stages the whole config/ tree, so
 * drop2's imports of drop1 and ./store.js resolve exactly as in production).
 */
const { test, assertEqual, assert, assertIncludes, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, applyEnv } = require("../../../tests/unit/njs-harness")

// --- ngx.fetch mock -------------------------------------------------------
let fetchImpl = async () => {
  throw new Error("no ngx.fetch stub set for this test")
}
let fetchLog = []
let logLines = []
global.ngx = {
  fetch: (url, init) => {
    fetchLog.push({ url, init: init || {} })
    return fetchImpl(url, init || {})
  },
  log: (level, msg) => logLines.push(String(msg)),
  ERR: 4,
}

// A minimal Response, matching the surface the handlers touch.
function res({ status = 200, body = "", headers = {}, url = "" }) {
  return {
    ok: status >= 200 && status < 300,
    status,
    url,
    headers: {
      get: (k) => (headers[k] !== undefined ? headers[k] : null),
    },
    text: async () => body,
  }
}

function findCookie(arr, prefix) {
  return (arr || []).find((c) => c.indexOf(prefix) === 0)
}
function form(body) {
  const o = {}
  String(body || "")
    .split("&")
    .forEach((kv) => {
      const i = kv.indexOf("=")
      if (i > 0) o[decodeURIComponent(kv.slice(0, i))] = decodeURIComponent(kv.slice(i + 1))
    })
  return o
}

const ENV = {
  ENTRA_TENANT_ID: "00000000-0000-0000-0000-0000000000aa",
  ENTRA_CLIENT_ID: "00000000-0000-0000-0000-0000000000cc",
  ENTRA_CLIENT_SECRET: "test-secret",
  ENTRA_STATE_HMAC_SECRET: "test-state-hmac-secret",
  ENTRA_WATCHDOG_SCOPE: "api://00000000-0000-0000-0000-0000000000dd/presence.readwrite",
  ENTRA_MDS_SCOPE: "api://fa-wm-app-test/full_scope",
  WM_MDS_BASE_URL: "https://fa-wm-app-test.azurewebsites.net/api/",
  WM_MDS_ACCESS_KEY: "test-mds-key",
  WEBSITE_SCHEME: "https",
  ENDPOINT_HTTP_PROTOCOL: "https",
  AUTH_HANDOVER_WHITELIST:
    "/auth-refresh-inbound,https://cps.outsystemsenterprise.com/,https://lcc-api.cps.gov.uk/,https://proxy.example/auth-refresh-inbound",
}

const TOKEN = "00000000-0000-0000-0000-000000000001"
const VERSION = "CMS.24.0.01"
const CC = "ASP.NET_SessionId=x; WindowID=MASTER"
// Destinations, as the `r` arg arrives at /init (njs has already decoded the query once).
const DEST_POLARIS = "/auth-refresh-inbound?polaris-ui-url=/polaris-ui/case/1"
const DEST_OS =
  "https://cps.outsystemsenterprise.com/Casework_Patterns/auth-handover.html?src=x&stage=os-cookie-return&r=casework/home"
const DEST_OTHER = "https://lcc-api.cps.gov.uk/api/init?x=1"

// Exactly what /init would 302 to for (dest, cc), plus any extra query.
function continuation(dest, extra) {
  const abs = dest.indexOf("http") === 0 ? dest : "https://proxy.example" + dest
  return abs + (dest.indexOf("?") !== -1 ? "&" : "?") + "cc=" + encodeURIComponent(CC) + (extra || "")
}
const POLARIS_TOKEN_PARAMS = "&cms-modern-token=" + TOKEN + "&cms-version-id=" + VERSION
const OS_TOKEN_PARAMS = "&cms-modern-token=" + TOKEN

// A router that establishes a CMS session (drop1's mint + verify loopbacks).
function establishRouter() {
  return async (url) => {
    if (/\/CMS$/.test(url)) return res({ headers: { Location: "/CMS.24.0.01/x" } })
    if (url.indexOf("uainGeneratedScript") !== -1)
      return res({ body: "var SESS_MODERN_USER_SESSION_ID = '00000000-0000-0000-0000-000000000001';" })
    if (url.indexOf("/graphql/") !== -1)
      return res({ body: JSON.stringify({ data: { user: { partyId: 1 } } }) })
    throw new Error("establishRouter: unexpected url " + url)
  }
}

// ---------------------------------------------------------------------------

async function storeTests(store) {
  console.log("\nstore.js — the MDS backend behind the deposit seam:")

  await test("scope comes from ENTRA_MDS_SCOPE; deposit === mdsDeposit (the seam binding)", () => {
    assertEqual(store.scope, ENV.ENTRA_MDS_SCOPE, "scope")
    assertEqual(store.deposit, store.mdsDeposit, "seam bound to MDS")
  })

  await test("deposit PUTs {cookies, token, expiryTime} to <WM_MDS_BASE_URL>cms-auth-store with bearer + function key", async () => {
    fetchLog = []
    fetchImpl = async () => res({ status: 204 })
    const out = await store.deposit({ cookies: "a=1; b=2", token: "tok-guid" }, "MDS-TOKEN")
    assertEqual(out.ok, true, "ok")
    assertEqual(fetchLog.length, 1, "one call")
    const { url, init } = fetchLog[0]
    assertEqual(url, "https://fa-wm-app-test.azurewebsites.net/api/cms-auth-store", "url (no double slash)")
    assertEqual(init.method, "PUT", "PUT")
    assertEqual(init.headers.Authorization, "Bearer MDS-TOKEN", "bearer")
    assertEqual(init.headers["x-functions-key"], "test-mds-key", "function key, as the gloco /api route sends")
    assertEqual(init.headers.Host, "fa-wm-app-test.azurewebsites.net", "Host")
    const body = JSON.parse(init.body)
    assertEqual(body.cookies, "a=1; b=2", "cookies")
    assertEqual(body.token, "tok-guid", "token")
    assertEqual(body.expiryTime, "2100-01-01T00:00:00Z", "the agreed fixed expiryTime")
    assert(Date.parse(body.expiryTime) > Date.now(), "MDS rejects a non-future expiryTime (400)")
    assertEqual(Object.keys(body).sort().join(","), "cookies,expiryTime,token", "exactly the MDS contract")
  })

  await test("deposit propagates an MDS failure -> {ok:false} with the status", async () => {
    fetchImpl = async () => res({ status: 401, body: "no oid" })
    const out = await store.deposit({ cookies: "a=1", token: "t" }, "MDS-TOKEN")
    assertEqual(out.ok, false, "not ok")
    assert(out.diag.indexOf("HTTP 401") === 0, "diag carries the status")
  })

  await test("deposit without a bearer refuses (no call)", async () => {
    fetchLog = []
    const out = await store.deposit({ cookies: "a=1", token: "t" }, "")
    assertEqual(out.ok, false, "not ok")
    assertEqual(fetchLog.length, 0, "no MDS call")
  })
}

async function helperTests(entra) {
  console.log("\nhelpers — state packing, refresh-token swap, presence cookies:")

  await test("unpackState rejects a tampered payload (HMAC integrity)", () => {
    const packed = entra.__test.packState({ s: "x" })
    const [p, mac] = packed.split(".")
    const tampered = entra.__test.b64urlEncode(JSON.stringify({ s: "evil" })) + "." + mac
    let threw = false
    try {
      entra.__test.unpackState(tampered)
    } catch (e) {
      threw = true
    }
    assert(threw, "tampered state rejected")
    assertEqual(entra.__test.unpackState(p + "." + mac).s, "x", "genuine state accepted")
  })

  await test("rand(n) -> 2n hex chars", () => {
    assert(/^[0-9a-f]{32}$/.test(entra.__test.rand(16)), "32 hex chars")
  })

  await test("refresh: refresh_token grant + client secret, the user's refresh token, the target scope", async () => {
    fetchLog = []
    fetchImpl = async () => res({ body: JSON.stringify({ access_token: "MDS-TOKEN", expires_in: 3599 }) })
    const out = await entra.__test.refresh("RT-1", "api://fa-wm-app-test/full_scope")
    assertEqual(out.accessToken, "MDS-TOKEN", "swapped token")
    assertEqual(out.expiresIn, 3599, "lifetime")
    const f = form(fetchLog[0].init.body)
    assertEqual(f.grant_type, "refresh_token", "grant")
    assertEqual(f.refresh_token, "RT-1", "the refresh token")
    assertEqual(f.scope, "api://fa-wm-app-test/full_scope", "scope")
    assertEqual(f.client_id, ENV.ENTRA_CLIENT_ID, "client id")
    assertEqual(f.client_secret, ENV.ENTRA_CLIENT_SECRET, "client secret (a confidential client's refresh token needs it)")
    assertEqual(f.assertion, undefined, "no OBO assertion")
    assertEqual(f.requested_token_use, undefined, "not on-behalf-of")
    assert(fetchLog[0].url.indexOf("/" + ENV.ENTRA_TENANT_ID + "/oauth2/v2.0/token") !== -1, "tenant token endpoint")
  })

  await test("refresh surfaces AD's error code (e.g. consent missing) but never a token", async () => {
    fetchImpl = async () =>
      res({ status: 400, body: JSON.stringify({ error: "invalid_grant", error_codes: [65001] }) })
    const out = await entra.__test.refresh("RT-1", "api://x/full_scope")
    assertEqual(out.accessToken, "", "no token")
    assert(out.diag.indexOf("invalid_grant") !== -1 && out.diag.indexOf("AADSTS65001") !== -1, "diag: " + out.diag)
  })

  await test("refresh with no scope makes no call", async () => {
    fetchLog = []
    const out = await entra.__test.refresh("RT-1", "")
    assertEqual(out.accessToken, "", "no token")
    assertEqual(out.diag, "no-scope", "diag")
    assertEqual(fetchLog.length, 0, "no AD call")
  })

  await test("refresh with no refresh token makes no call", async () => {
    fetchLog = []
    const out = await entra.__test.refresh("", "api://x/full_scope")
    assertEqual(out.accessToken, "", "no token")
    assertEqual(fetchLog.length, 0, "no AD call")
  })

  await test("presenceCookies: one HttpOnly cookie per presence path, Max-Age = token lifetime", () => {
    const cs = entra.__test.presenceCookies("WATCHDOG-TOKEN", 3599)
    assertEqual(cs.length, 2, "two paths")
    assert(cs[0].indexOf("cms-auth-presence-token=WATCHDOG-TOKEN; Path=/global-components/presence-jsonp;") === 0, cs[0])
    assert(cs[1].indexOf("; Path=/global-components/case-locking;") !== -1, cs[1])
    cs.forEach((c) => assert(/HttpOnly; Secure; SameSite=Lax; Max-Age=3599$/.test(c), c))
  })
}

// /init with Entra on: appAuthRedirect hands over to drop2's begin step after the whitelist check
// and the cookie filter. A request as /init receives it (raw jar in `cookie`, destination in `r`).
function initRequest(args, headersIn) {
  return createMockRequest({
    args: { cookie: "ASP.NET_SessionId=x; Junk=1", ...args },
    headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https", ...(headersIn || {}) },
  })
}

async function initTests(authHandover, entra) {
  console.log("\n/init (appAuthRedirect) with Entra on — the begin step:")
  const on = { ENTRA_STORE_ENABLED: "true" }

  await test("Entra on: CMS lookup, then 302 to Entra /authorize (neutral sign-in, prompt=none); state carries r + cc + token", async () => {
    fetchLog = []
    fetchImpl = establishRouter()
    const restore = applyEnv(on)
    const r = initRequest({ r: DEST_OS })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnCode, 302, "302")
    assert(r.returnBody.indexOf("https://login.microsoftonline.com/" + ENV.ENTRA_TENANT_ID + "/oauth2/v2.0/authorize?") === 0, "-> AD authorize: " + r.returnBody)
    const q = form(r.returnBody.split("?")[1])
    assertEqual(q.scope, "openid profile offline_access", "neutral sign-in")
    assertEqual(q.prompt, "none", "silent")
    assertEqual(q.redirect_uri, "https://proxy.example/init/entra-callback", "callback under /init on this host")
    assert(/^[0-9a-f]{32}$/.test(q.state), "random state handle")
    const cookies = [].concat(r.headersOut["Set-Cookie"] || [])
    assert(cookies.some((c) => c.indexOf("Cms-Session-Hint=") === 0), "the session hint is still set (appended to, not replaced)")
    const sc = findCookie(cookies, "entra_auth_state=")
    assert(!!sc, "state cookie")
    assertIncludes(sc, "; Path=/init/entra-callback;", "state cookie rides only to the callback")
    assertIncludes(sc, "HttpOnly", "HttpOnly")
    const st = entra.__test.unpackState(sc.slice("entra_auth_state=".length).split(";")[0])
    assertEqual(st.r, DEST_OS, "destination")
    assertEqual(st.cc, CC, "the FILTERED cookies, exactly as /init would pass them")
    assertEqual(st.tok, TOKEN, "token")
    assertEqual(st.ver, VERSION, "version")
    assertEqual(st.term, "top-level", "top-level")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker")
  })

  await test("enrol cookie drop2 turns Entra on for that browser with the switch off", async () => {
    fetchImpl = establishRouter()
    const r = initRequest({ r: DEST_POLARIS }, { Cookie: "polaris_auth_handover=drop2" })
    await authHandover.appAuthRedirect(r)
    assert(r.returnBody.indexOf("https://login.microsoftonline.com/") === 0, "-> AD: " + r.returnBody)
  })

  await test("Entra off: no CMS calls, byte-identical to today's /init", async () => {
    fetchLog = []
    const r = initRequest({ r: DEST_OS })
    await authHandover.appAuthRedirect(r)
    assertEqual(fetchLog.length, 0, "no fetch")
    assertEqual(r.returnBody, continuation(DEST_OS), "plain 302 to r with cc")
    assertEqual(findCookie([].concat(r.headersOut["Set-Cookie"] || []), "entra_auth_state="), undefined, "no state cookie")
  })

  await test("Entra on but NOT fully configured (no client id / secret / HMAC key / tenant): never sign in, carry on", async () => {
    for (const k of ["ENTRA_CLIENT_ID", "ENTRA_CLIENT_SECRET", "ENTRA_STATE_HMAC_SECRET", "ENTRA_TENANT_ID"]) {
      fetchLog = []
      const restore = applyEnv({ ...on, [k]: "" })
      const r = initRequest({ r: DEST_OS })
      await authHandover.appAuthRedirect(r)
      restore()
      assertEqual(r.returnBody, continuation(DEST_OS), "carry on without " + k)
      assertEqual(fetchLog.length, 0, "no CMS lookup without " + k)
    }
  })

  await test("Entra on, CMS lookup fails: carry on exactly as today (302 to r with cc)", async () => {
    fetchImpl = async (url) => {
      if (/\/CMS$/.test(url)) return res({ headers: { Location: "/CMS.24.0.01/x" } })
      return res({ body: "no token" })
    }
    const restore = applyEnv(on)
    const r = initRequest({ r: DEST_OS })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, continuation(DEST_OS), "carry on")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker")
  })

  await test("Entra on, a CMS lookup that throws: still carries on", async () => {
    fetchImpl = async () => {
      throw new Error("network down")
    }
    const restore = applyEnv(on)
    const r = initRequest({ r: DEST_OS })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, continuation(DEST_OS), "carry on")
  })

  await test("Entra on, no CMS session cookies: no lookup, carry on", async () => {
    fetchLog = []
    const restore = applyEnv(on)
    const r = initRequest({ r: DEST_OS, cookie: "Junk=1" })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(fetchLog.length, 0, "no fetch")
    assert(r.returnBody.indexOf("https://cps.outsystemsenterprise.com/") === 0, "carry on: " + r.returnBody)
  })

  await test("Entra on, NON-whitelisted r: 403 and no sign-in", async () => {
    fetchLog = []
    const restore = applyEnv(on)
    const r = initRequest({ r: "https://evil.example/x" })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnCode, 403, "403")
    assertEqual(fetchLog.length, 0, "no fetch")
  })

  await test("terminal=iframe is carried into the state", async () => {
    fetchImpl = establishRouter()
    const restore = applyEnv(on)
    const r = initRequest({ terminal: "iframe", q: "x" })
    await authHandover.appAuthRedirect(r)
    restore()
    const sc = findCookie([].concat(r.headersOut["Set-Cookie"] || []), "entra_auth_state=")
    const st = entra.__test.unpackState(sc.slice("entra_auth_state=".length).split(";")[0])
    assertEqual(st.term, "iframe", "iframe")
    assert(st.r.indexOf("/auth-refresh-inbound?") === 0, "legacy shim's synthesised r: " + st.r)
  })
}

// Drive the callback with a valid state cookie + code, routing the exchange, the refresh-token swaps
// (by scope: MDS / watchdog) and MDS. `rt: false` = the exchange returns no refresh token.
async function callbackScenario({
  term,
  exchange = "ok",
  rt = true,
  mdsSwap = "ok",
  watchdogSwap = "ok",
  mdsStatus = 204,
  adError = null,
  env = null,
  dest = DEST_POLARIS,
}) {
  const restore = env ? applyEnv(env) : () => {}
  const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
  restore()
  const st = { s: "STATE-HANDLE", cc: CC, tok: TOKEN, ver: VERSION, r: dest, term: term, corr: "corr-1" }
  fetchLog = []
  logLines = []
  fetchImpl = async (url, init) => {
    if (url.indexOf("/oauth2/v2.0/token") !== -1) {
      const f = form(init.body)
      if (f.grant_type === "authorization_code") {
        if (exchange !== "ok") return res({ status: 400, body: JSON.stringify({ error: "invalid_grant" }) })
        const body = { access_token: "GRAPH-TOKEN", expires_in: 3599, id_token: "ID-TOKEN" }
        if (rt) body.refresh_token = "RT-SECRET-1"
        return res({ body: JSON.stringify(body) })
      }
      if (f.grant_type === "refresh_token") {
        const isMds = f.scope === ENV.ENTRA_MDS_SCOPE
        const ok = isMds ? mdsSwap === "ok" : watchdogSwap === "ok"
        if (!ok) return res({ status: 400, body: JSON.stringify({ error: "invalid_grant", error_codes: [65001] }) })
        return res({
          body: JSON.stringify(
            isMds ? { access_token: "MDS-TOKEN", expires_in: 3599 } : { access_token: "WATCHDOG-TOKEN", expires_in: 3000 },
          ),
        })
      }
      throw new Error("callback: unexpected grant " + f.grant_type)
    }
    if (url.indexOf("/cms-auth-store") !== -1) return res({ status: mdsStatus, body: mdsStatus >= 400 ? "err" : "" })
    throw new Error("callback: unexpected url " + url)
  }
  const r = createMockRequest({
    args: adError ? { error: adError } : { state: st.s, code: "CODE-1" },
    headersIn: {
      Host: "proxy.example",
      "X-Forwarded-Proto": "https",
      Cookie: "entra_auth_state=" + entra.__test.packState(st),
    },
  })
  await entra.handleInitEntraCallback(r)
  return r
}

// The sequence of outbound calls, e.g. "authorization_code > refresh_token:mds > mds".
function callOrder() {
  return fetchLog
    .map((c) => {
      if (c.url.indexOf("cms-auth-store") !== -1) return "mds"
      const f = form(c.init.body)
      if (f.grant_type !== "refresh_token") return f.grant_type
      return "refresh_token:" + (f.scope === ENV.ENTRA_MDS_SCOPE ? "mds" : f.scope === ENV.ENTRA_WATCHDOG_SCOPE ? "watchdog" : f.scope)
    })
    .join(" > ")
}

// The refresh token must never leave the callback: not in a cookie, not in a log line.
function assertRefreshTokenContained(r) {
  ;(r.headersOut["Set-Cookie"] || []).forEach((c) => assert(c.indexOf("RT-SECRET-1") === -1, "refresh token in a cookie: " + c))
  logLines.forEach((l) => assert(l.indexOf("RT-SECRET-1") === -1, "refresh token in a log line: " + l))
  assert(String(r.returnBody || "").indexOf("RT-SECRET-1") === -1, "refresh token in the response")
}

const PRESENCE_PREFIX = "cms-auth-presence-token="
const presenceCookies = (r) => (r.headersOut["Set-Cookie"] || []).filter((c) => c.indexOf(PRESENCE_PREFIX) === 0)

async function callbackTests() {
  console.log("\nhandleInitEntraCallback — exchange -> refresh-token swap per service -> MDS deposit -> carry on / degrade:")

  await test("top-level success, Polaris destination: 302 to r with cc + token + version; no Cms-Auth-Values here", async () => {
    const r = await callbackScenario({ term: "top-level" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "-> /auth-refresh-inbound, with the token passed on")
    const sc = r.headersOut["Set-Cookie"]
    assertEqual(findCookie(sc, "Cms-Auth-Values="), undefined, "Cms-Auth-Values is /auth-refresh-inbound's job now")
    assert(findCookie(sc, "entra_auth_state=deleted") !== undefined, "state cookie cleared")
    assertIncludes(findCookie(sc, "entra_auth_state=deleted"), "Path=/init/entra-callback", "cleared on its own path")
    assertEqual(presenceCookies(r).length, 0, "no presence cookie on top-level")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra")
    assertEqual(callOrder(), "authorization_code > refresh_token:mds > mds", "call order (no watchdog swap at top level)")
    assertEqual(form(fetchLog[0].init.body).scope, "openid profile offline_access", "exchange asks for the SAME scope as /authorize")
    assertEqual(form(fetchLog[1].init.body).refresh_token, "RT-SECRET-1", "the swap uses the exchange's refresh token")
    assertEqual(fetchLog[2].init.headers.Authorization, "Bearer MDS-TOKEN", "MDS gets the swapped token")
    assertRefreshTokenContained(r)
  })

  await test("iframe success: 200 terminal, presence cookies carry the WATCHDOG token (its lifetime), NO Cms-Auth-Values", async () => {
    const r = await callbackScenario({ term: "iframe" })
    assertEqual(r.returnCode, 200, "200")
    assert(r.returnBody.indexOf('data-cms-auth="done"') !== -1, "renders the terminal page")
    assertEqual(findCookie(r.headersOut["Set-Cookie"], "Cms-Auth-Values="), undefined, "no Cms-Auth-Values")
    const pc = presenceCookies(r)
    assertEqual(pc.length, 2, "both presence paths")
    pc.forEach((c) => assert(c.indexOf(PRESENCE_PREFIX + "WATCHDOG-TOKEN;") === 0 && /Max-Age=3000$/.test(c), c))
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra")
    assertEqual(
      callOrder(),
      "authorization_code > refresh_token:watchdog > refresh_token:mds > mds",
      "presence swap BEFORE the MDS swap",
    )
    assertRefreshTokenContained(r)
  })

  await test("top-level success, OS handover page: 302 to r with cc + token (no version)", async () => {
    const r = await callbackScenario({ term: "top-level", dest: DEST_OS })
    assertEqual(r.returnBody, continuation(DEST_OS, OS_TOKEN_PARAMS), "-> OS handover page")
  })

  await test("top-level success, any other destination: 302 to r with cc only (no token)", async () => {
    const r = await callbackScenario({ term: "top-level", dest: DEST_OTHER })
    assertEqual(r.returnBody, continuation(DEST_OTHER), "-> as today")
  })

  await test("Polaris destination on another host, or an OS page at another stage: no token", async () => {
    const otherHost = "https://polaris.cps.gov.uk/auth-refresh-inbound?polaris-ui-url=/materials-ui"
    let r = await callbackScenario({ term: "top-level", dest: otherHost })
    assertEqual(r.returnBody, continuation(otherHost), "another host's inbound gets no token")
    const osOther = "https://cps.outsystemsenterprise.com/Casework_Patterns/auth-handover.html?stage=os-token-return&r=x"
    r = await callbackScenario({ term: "top-level", dest: osOther })
    assertEqual(r.returnBody, continuation(osOther), "only stage=os-cookie-return gets the token")
  })

  await test("absolute Polaris destination on THIS host gets the token", async () => {
    const abs = "https://proxy.example/auth-refresh-inbound?polaris-ui-url=/materials-ui"
    const r = await callbackScenario({ term: "top-level", dest: abs })
    assertEqual(r.returnBody, continuation(abs, POLARIS_TOKEN_PARAMS), "same host")
  })

  await test("iframe with no watchdog scope: no presence cookie, no watchdog swap; the deposit still succeeds", async () => {
    const r = await callbackScenario({ term: "iframe", env: { ...ENV, ENTRA_WATCHDOG_SCOPE: "" } })
    assertEqual(r.returnCode, 200, "terminal")
    assertEqual(presenceCookies(r).length, 0, "no presence cookie")
    assertEqual(callOrder(), "authorization_code > refresh_token:mds > mds", "no watchdog swap")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra (presence is not the store)")
    assertEqual(logLines.filter((l) => l.indexOf("presence") !== -1).length, 0, "presence switched off is not a failure (no log noise)")
  })

  await test("iframe watchdog swap failure: no presence cookie, but the deposit still runs and succeeds", async () => {
    const r = await callbackScenario({ term: "iframe", watchdogSwap: "fail" })
    assertEqual(r.returnCode, 200, "terminal")
    assertEqual(presenceCookies(r).length, 0, "no presence cookie")
    assertEqual(fetchLog.filter((c) => c.url.indexOf("cms-auth-store") !== -1).length, 1, "MDS deposit still made")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra")
  })

  await test("MDS swap failure (consent missing) degrades: still carries on to r (token passed on); no MDS call", async () => {
    const r = await callbackScenario({ term: "top-level", mdsSwap: "fail" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "-> carry on (login not blocked)")
    assertEqual(fetchLog.filter((c) => c.url.indexOf("cms-auth-store") !== -1).length, 0, "no MDS call")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker: entra-degraded")
    assert(logLines.some((l) => l.indexOf("swap-failed") !== -1), "degrade reason logged as swap-failed")
    assertRefreshTokenContained(r)
  })

  await test("MDS swap failure in the iframe still sets the presence cookies (presence is independent of the store)", async () => {
    const r = await callbackScenario({ term: "iframe", mdsSwap: "fail" })
    assertEqual(r.returnCode, 200, "terminal")
    assertEqual(presenceCookies(r).length, 2, "presence cookies kept")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker: entra-degraded")
  })

  await test("MDS failure degrades: top-level carries on; iframe keeps presence cookies", async () => {
    const top = await callbackScenario({ term: "top-level", mdsStatus: 401 })
    assertEqual(top.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "-> carry on")
    assertEqual(top.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker")
    const ifr = await callbackScenario({ term: "iframe", mdsStatus: 500 })
    assertEqual(presenceCookies(ifr).length, 2, "presence cookies kept")
  })

  await test("exchange returns no refresh token (offline_access not granted): degrade, no swaps, no presence", async () => {
    const top = await callbackScenario({ term: "top-level", rt: false })
    assertEqual(top.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "-> carry on")
    assertEqual(top.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker")
    assertEqual(callOrder(), "authorization_code", "no swaps attempted")
    assert(
      logLines.some((l) => /degrade — no-refresh-token$/.test(l)),
      "its own degrade reason, not a failed swap: " + logLines.join(" | "),
    )
    const ifr = await callbackScenario({ term: "iframe", rt: false })
    assertEqual(presenceCookies(ifr).length, 0, "no presence cookie")
  })

  await test("code-exchange failure degrades with NO presence cookie and no swaps", async () => {
    const r = await callbackScenario({ term: "iframe", exchange: "fail" })
    assertEqual(r.returnCode, 200, "terminal")
    assertEqual(presenceCookies(r).length, 0, "no token -> no presence cookie")
    assertEqual(fetchLog.length, 1, "only the failed exchange")
  })

  await test("AD error (login_required) degrades: carries on to r, for every destination type", async () => {
    let r = await callbackScenario({ term: "top-level", adError: "login_required" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "Polaris")
    r = await callbackScenario({ term: "top-level", adError: "login_required", dest: DEST_OS })
    assertEqual(r.returnBody, continuation(DEST_OS, OS_TOKEN_PARAMS), "OS")
    r = await callbackScenario({ term: "top-level", adError: "login_required", dest: DEST_OTHER })
    assertEqual(r.returnBody, continuation(DEST_OTHER), "other: exactly today's redirect")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker")
  })

  await test("a degraded other-destination callback lands EXACTLY where /init with Entra off would", async () => {
    const authHandover = await loadNjs("features/auth-handover/auth-handover.js")
    const off = initRequest({ r: DEST_OTHER, cookie: CC })
    await authHandover.appAuthRedirect(off)
    const degraded = await callbackScenario({ term: "top-level", mdsSwap: "fail", dest: DEST_OTHER })
    assertEqual(degraded.returnBody, off.returnBody, "same URL")
  })

  await test("bad state cookie (tampered): no recoverable destination -> fallback landing", async () => {
    const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
    const r = createMockRequest({
      args: { state: "x", code: "y" },
      headersIn: { Host: "proxy.example", Cookie: "entra_auth_state=eyJ9.bad" },
    })
    await entra.handleInitEntraCallback(r)
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "fallback landing")
  })

  await test("missing state cookie: no recoverable session -> fallback landing", async () => {
    const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
    fetchImpl = async () => {
      throw new Error("should not fetch")
    }
    const r = createMockRequest({ args: { state: "x", code: "y" }, headersIn: { Host: "proxy.example" } })
    await entra.handleInitEntraCallback(r)
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "fallback landing, absolute https (no X-Forwarded-Proto -> https)")
  })
}


// D2 + D5: the state-cookie size guard, and one structured outcome line per handover.
const outcomeLines = () => logLines.filter((l) => l.indexOf("drop2-outcome ") === 0)
function outcomeOf(line) {
  const o = {}
  line
    .slice("drop2-outcome ".length)
    .split(" ")
    .forEach((kv) => {
      const i = kv.indexOf("=")
      o[kv.slice(0, i)] = kv.slice(i + 1)
    })
  return o
}

async function guardAndOutcomeTests(authHandover) {
  console.log("\nD2 state-cookie size guard + D5 drop2-outcome log line:")
  const on = { ENTRA_STORE_ENABLED: "true" }

  await test("D2: a state cookie that would exceed ~3.8 KB -> no Entra, carry on; logs the length only", async () => {
    fetchImpl = establishRouter()
    logLines = []
    const restore = applyEnv(on)
    const big = "ASP.NET_SessionId=x; .CMSAUTH=" + "A".repeat(3500)
    const r = initRequest({ r: DEST_OS, cookie: big })
    await authHandover.appAuthRedirect(r)
    restore()
    assert(r.returnBody.indexOf("https://cps.outsystemsenterprise.com/") === 0, "carry on: " + r.returnBody.slice(0, 80))
    assertEqual(findCookie([].concat(r.headersOut["Set-Cookie"] || []), "entra_auth_state="), undefined, "no state cookie")
    const o = outcomeOf(outcomeLines()[0])
    assertEqual(o.result, "skipped", "skipped")
    assertEqual(o.reason, "state-too-large", "reason")
    assert(/^\d+$/.test(o.bytes) && Number(o.bytes) > 3800, "length logged: " + o.bytes)
    logLines.forEach((l) => assert(l.indexOf("AAAA") === -1, "cookie content in a log line"))
  })

  await test("D2: an ordinary state cookie is well under the guard and Entra begins", async () => {
    fetchImpl = establishRouter()
    const restore = applyEnv(on)
    const r = initRequest({ r: DEST_OS, cookie: "ASP.NET_SessionId=" + "x".repeat(200) + "; .CMSAUTH=" + "y".repeat(600) })
    await authHandover.appAuthRedirect(r)
    restore()
    assert(r.returnBody.indexOf("https://login.microsoftonline.com/") === 0, "began")
    const sc = findCookie([].concat(r.headersOut["Set-Cookie"] || []), "entra_auth_state=")
    assert(sc.split(";")[0].length <= 3800, "under the guard: " + sc.split(";")[0].length)
  })

  await test("D5: begin -> no outcome line yet (the callback writes it)", async () => {
    fetchImpl = establishRouter()
    logLines = []
    const restore = applyEnv(on)
    await authHandover.appAuthRedirect(initRequest({ r: DEST_OS }))
    restore()
    assertEqual(outcomeLines().length, 0, "none at begin")
  })

  await test("D5: begin skipped (not configured / lookup failed) -> one skipped line with dest + term", async () => {
    logLines = []
    let restore = applyEnv({ ...on, ENTRA_CLIENT_SECRET: "" })
    await authHandover.appAuthRedirect(initRequest({ r: DEST_OTHER }))
    restore()
    let o = outcomeOf(outcomeLines()[0])
    assertEqual(outcomeLines().length, 1, "one line")
    assertEqual([o.result, o.reason, o.dest, o.term].join(","), "skipped,not-configured,other,top-level", "fields")
    logLines = []
    fetchImpl = async (url) => (/\/CMS$/.test(url) ? res({ headers: { Location: "/CMS.24.0.01/x" } }) : res({ body: "x" }))
    restore = applyEnv(on)
    await authHandover.appAuthRedirect(initRequest({ r: DEST_POLARIS }))
    restore()
    o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.dest].join(","), "skipped,cms-lookup-failed,polaris", "fields")
  })

  await test("D5: Entra off -> no outcome line at all", async () => {
    logLines = []
    await authHandover.appAuthRedirect(initRequest({ r: DEST_OS }))
    assertEqual(outcomeLines().length, 0, "silent when off")
  })

  await test("D5: callback success -> one ok line; degrade -> one degraded line with a bare reason code", async () => {
    await callbackScenario({ term: "top-level", dest: DEST_OS })
    let lines = outcomeLines()
    assertEqual(lines.length, 1, "one line")
    let o = outcomeOf(lines[0])
    assertEqual([o.result, o.reason, o.dest, o.term].join(","), "ok,ok,os,top-level", "ok fields")

    await callbackScenario({ term: "iframe", mdsSwap: "fail" })
    lines = outcomeLines()
    assertEqual(lines.length, 1, "one line")
    o = outcomeOf(lines[0])
    assertEqual([o.result, o.reason, o.dest, o.term].join(","), "degraded,swap-failed,polaris,iframe", "degraded fields")

    await callbackScenario({ term: "top-level", adError: "login_required", dest: DEST_OTHER })
    o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.dest].join(","), "degraded,ad-error,other", "ad-error is the code; the AD error name stays in the detail line")
  })

  await test("D5: the outcome line never carries a token, a cookie, the destination URL or an identity", async () => {
    await callbackScenario({ term: "iframe" })
    const line = outcomeLines()[0]
    for (const bad of [TOKEN, "RT-SECRET-1", "MDS-TOKEN", "WATCHDOG-TOKEN", "ASP.NET", "polaris-ui", "proxy.example"])
      assert(line.indexOf(bad) === -1, "outcome line leaks " + bad + ": " + line)
    assertEqual(Object.keys(outcomeOf(line)).sort().join(","), "dest,reason,result,term", "exactly these fields")
  })

  await test("D5: unrecoverable state -> one degraded line with dest=unknown", async () => {
    const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
    logLines = []
    const r = createMockRequest({ args: { state: "x", code: "y" }, headersIn: { Host: "proxy.example" } })
    await entra.handleInitEntraCallback(r)
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.dest, o.term].join(","), "degraded,missing-state,unknown,unknown", "fields")
  })
}

async function main() {
  // Module consts read env at load; apply before loading.
  const restore = applyEnv(ENV)
  const store = await loadNjs("features/auth-handover.drop2.entra/store.js")
  const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
  const authHandover = await loadNjs("features/auth-handover/auth-handover.js")

  await storeTests(store)
  await helperTests(entra)
  await initTests(authHandover, entra)
  await callbackTests()
  await guardAndOutcomeTests(authHandover)

  restore()
  process.exit(summarise("auth-handover.drop2.entra (unit)"))
}

main()
