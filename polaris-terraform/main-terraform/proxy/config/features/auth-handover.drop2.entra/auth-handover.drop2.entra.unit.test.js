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
// The per-flow state cookie: entra_auth_state_<handle>=<payload>; …
const STATE_PREFIX = "entra_auth_state_"
const cookieValue = (sc) => sc.split(";")[0].split("=").slice(1).join("=")
const cookieName = (sc) => sc.split("=")[0]
const nowSec = () => Math.floor(Date.now() / 1000)
const H1 = "a".repeat(32)
const H2 = "b".repeat(32)
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
    assert(/^t\.[0-9a-f]{32}$/.test(q.state), "state = t.<random handle> (top-level): " + q.state)
    const cookies = [].concat(r.headersOut["Set-Cookie"] || [])
    assert(cookies.some((c) => c.indexOf("Cms-Session-Hint=") === 0), "the session hint is still set (appended to, not replaced)")
    const sc = findCookie(cookies, STATE_PREFIX)
    assert(!!sc, "state cookie")
    assertIncludes(sc, "; Path=/init/entra-callback;", "state cookie rides only to the callback")
    assertIncludes(sc, "HttpOnly", "HttpOnly")
    const st = entra.__test.unpackState(cookieValue(sc))
    assertEqual(st.r, DEST_OS, "destination")
    assertEqual(st.cc, CC, "the FILTERED cookies, exactly as /init would pass them")
    assertEqual(st.tok, TOKEN, "token")
    assertEqual(st.ver, VERSION, "version")
    assertEqual(st.term, "top-level", "top-level")
    assertEqual(cookieName(sc), STATE_PREFIX + q.state.slice(2), "the cookie is named by this flow's handle")
    assertEqual(st.s, q.state.slice(2), "payload carries the handle")
    assert(Math.abs(st.iat - nowSec()) <= 2, "issued-at stamped: " + st.iat)
    assertEqual(st.h, "proxy.example", "bound to the host")
    assertEqual(st.dk, "os", "destination kind recorded")
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
    assertEqual(findCookie([].concat(r.headersOut["Set-Cookie"] || []), STATE_PREFIX), undefined, "no state cookie")
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
    const sc = findCookie([].concat(r.headersOut["Set-Cookie"] || []), STATE_PREFIX)
    const st = entra.__test.unpackState(cookieValue(sc))
    assertEqual(st.term, "iframe", "iframe")
    assertEqual(st.r, undefined, "no destination carried: the iframe always ends on the terminal page")
    assertEqual(st.dk, "polaris", "destination kind still recorded (for the outcome line)")
    const q = form(r.returnBody.split("?")[1])
    assert(/^i\.[0-9a-f]{32}$/.test(q.state), "state = i.<handle> (iframe): " + q.state)
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
  adErrorDescription = null,
  env = null,
  dest = DEST_POLARIS,
  handle = H1,
  stPatch = null,
  stateParam = null,
  extraCookie = "",
  idClaims = null,
  watchdogToken = "WATCHDOG-TOKEN",
}) {
  // Applied for the load (module consts) AND the call (per-request switches).
  const restore = env ? applyEnv(env) : () => {}
  const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
  const st = {
    s: handle,
    cc: CC,
    tok: TOKEN,
    ver: VERSION,
    r: term === "iframe" ? undefined : dest,
    dk: entra.__test.destKind(createMockRequest({ headersIn: { Host: "proxy.example" } }), dest),
    term: term,
    iat: nowSec(),
    h: "proxy.example",
    corr: "corr-1",
    ...(stPatch || {}),
  }
  const sp = stateParam !== null ? stateParam : (term === "iframe" ? "i." : "t.") + handle
  fetchLog = []
  logLines = []
  fetchImpl = async (url, init) => {
    if (url.indexOf("/oauth2/v2.0/token") !== -1) {
      const f = form(init.body)
      if (f.grant_type === "authorization_code") {
        if (exchange !== "ok") return res({ status: 400, body: JSON.stringify({ error: "invalid_grant" }) })
        const body = { access_token: "GRAPH-TOKEN", expires_in: 3599, id_token: idClaims ? jwt(idClaims) : "ID-TOKEN" }
        if (rt) body.refresh_token = "RT-SECRET-1"
        return res({ body: JSON.stringify(body) })
      }
      if (f.grant_type === "refresh_token") {
        const isMds = f.scope === ENV.ENTRA_MDS_SCOPE
        const ok = isMds ? mdsSwap === "ok" : watchdogSwap === "ok"
        if (!ok) return res({ status: 400, body: JSON.stringify({ error: "invalid_grant", error_codes: [65001] }) })
        return res({
          body: JSON.stringify(
            isMds ? { access_token: "MDS-TOKEN", expires_in: 3599 } : { access_token: watchdogToken, expires_in: 3000 },
          ),
        })
      }
      throw new Error("callback: unexpected grant " + f.grant_type)
    }
    if (url.indexOf("/cms-auth-store") !== -1) return res({ status: mdsStatus, body: mdsStatus >= 400 ? "err" : "" })
    throw new Error("callback: unexpected url " + url)
  }
  const r = createMockRequest({
    args: adError
      ? Object.assign({ error: adError, state: sp }, adErrorDescription === null ? {} : { error_description: adErrorDescription })
      : { state: sp, code: "CODE-1" },
    headersIn: {
      Host: "proxy.example",
      "X-Forwarded-Proto": "https",
      Cookie: [STATE_PREFIX + handle + "=" + entra.__test.packState(st), extraCookie].filter(Boolean).join("; "),
    },
  })
  await entra.handleInitEntraCallback(r)
  restore()
  return r
}

// An unsigned JWT carrying `claims` (the callback reads, never verifies, the id_token's claims).
function jwt(claims) {
  const b = (o) => Buffer.from(JSON.stringify(o)).toString("base64").replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "")
  return b({ alg: "none" }) + "." + b(claims) + ".sig"
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
    assert(findCookie(sc, STATE_PREFIX + H1 + "=deleted") !== undefined, "this flow's state cookie cleared")
    assertIncludes(findCookie(sc, STATE_PREFIX + H1 + "=deleted"), "Path=/init/entra-callback", "cleared on its own path")
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
      args: { state: "t." + H1, code: "y" },
      headersIn: { Host: "proxy.example", Cookie: STATE_PREFIX + H1 + "=eyJ9.bad" },
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
    assertEqual(findCookie([].concat(r.headersOut["Set-Cookie"] || []), STATE_PREFIX), undefined, "no state cookie")
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
    const sc = findCookie([].concat(r.headersOut["Set-Cookie"] || []), STATE_PREFIX)
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
    assertEqual([o.result, o.reason, o.dest].join(","), "degraded,ad-error,other", "ad-error is the code")
  })

  await test("D5: an AD error adds ad=<error> aadsts=<number>: no session vs several accounts vs MFA", async () => {
    const cases = [
      ["login_required", "AADSTS50058: A silent sign-in request was sent but no user is signed in.", "login_required,50058"],
      ["interaction_required", "AADSTS16000: SelectUserAccount", "interaction_required,16000"],
      ["interaction_required", "AADSTS50076: Due to a configuration change", "interaction_required,50076"],
      ["consent_required", null, "consent_required,none"],
    ]
    for (const [adError, adErrorDescription, want] of cases) {
      await callbackScenario({ term: "top-level", adError, adErrorDescription })
      const lines = outcomeLines()
      assertEqual(lines.length, 1, "one line")
      const o = outcomeOf(lines[0])
      assertEqual([o.ad, o.aadsts].join(","), want, adError + " fields")
      assertEqual(o.reason, "ad-error", "reason stays ad-error")
    }
    // Lost state (no cookie) still records the AD error.
    await callbackScenario({ term: "iframe", adError: "login_required", adErrorDescription: "AADSTS50058: x", extraCookie: "", stateParam: "i." + H2 })
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.term, o.ad, o.aadsts].join(","), "degraded,ad-error,iframe,login_required,50058", "lost-state fields")
  })

  await test("D5: hostile error / error_description can't inject into the outcome line", async () => {
    await callbackScenario({
      term: "top-level",
      adError: "x reason=ok result=ok",
      adErrorDescription: "AADSTS1 dest=os token=SECRET",
    })
    const line = outcomeLines()[0]
    assert(line.indexOf("SECRET") === -1 && line.indexOf("result=ok") === -1, line)
    const o = outcomeOf(line)
    assertEqual([o.result, o.reason, o.ad, o.aadsts].join(","), "degraded,ad-error,other,1", "sanitised")
    assertEqual(line.split(" ").length, 9, "no extra fields: " + line)
  })

  await test("D5: the outcome line never carries a token, a cookie, the destination URL or an identity", async () => {
    await callbackScenario({ term: "iframe" })
    const line = outcomeLines()[0]
    for (const bad of [TOKEN, "RT-SECRET-1", "MDS-TOKEN", "WATCHDOG-TOKEN", "ASP.NET", "polaris-ui", "proxy.example"])
      assert(line.indexOf(bad) === -1, "outcome line leaks " + bad + ": " + line)
    assertEqual(Object.keys(outcomeOf(line)).sort().join(","), "dest,hint,int,reason,result,term", "exactly these fields")
  })

  await test("D5: unrecoverable state -> one degraded line with dest=unknown", async () => {
    const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
    logLines = []
    let r = createMockRequest({ args: { state: "t." + H1, code: "y" }, headersIn: { Host: "proxy.example" } })
    await entra.handleInitEntraCallback(r)
    let o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.dest, o.term].join(","), "degraded,missing-state,unknown,top-level", "mode known from the state param")
    logLines = []
    r = createMockRequest({ args: { state: "x", code: "y" }, headersIn: { Host: "proxy.example" } })
    await entra.handleInitEntraCallback(r)
    o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.dest, o.term].join(","), "degraded,bad-state-param,unknown,unknown", "malformed state param")
  })
}


// The iframe never falls through into Polaris: whenever drop2 does not start, /init sends the hidden
// iframe to the frameable /init/terminal page (served by drop2's `terminal` handler).
async function iframeTerminalTests(authHandover, entra) {
  console.log("\nterminal=iframe when drop2 does not start -> /init/terminal (never Polaris in the hidden frame):")
  const on = { ENTRA_STORE_ENABLED: "true" }
  const TERMINAL = "https://proxy.example/init/terminal"

  await test("Entra on, not configured -> /init/terminal", async () => {
    const restore = applyEnv({ ...on, ENTRA_CLIENT_SECRET: "" })
    const r = initRequest({ terminal: "iframe", q: "x" })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, TERMINAL, "terminal")
  })

  await test("Entra on, CMS lookup fails -> /init/terminal", async () => {
    fetchImpl = async (url) => (/\/CMS$/.test(url) ? res({ headers: { Location: "/CMS.24.0.01/x" } }) : res({ body: "x" }))
    const restore = applyEnv(on)
    const r = initRequest({ terminal: "iframe", q: "x" })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, TERMINAL, "terminal")
  })

  await test("Entra on, no CMS cookies -> /init/terminal", async () => {
    const restore = applyEnv(on)
    const r = initRequest({ terminal: "iframe", q: "x", cookie: "Junk=1" })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, TERMINAL, "terminal")
  })

  await test("Entra on, state too large -> /init/terminal", async () => {
    fetchImpl = establishRouter()
    const restore = applyEnv(on)
    const r = initRequest({ terminal: "iframe", q: "x", cookie: "ASP.NET_SessionId=x; .CMSAUTH=" + "A".repeat(3500) })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, TERMINAL, "terminal")
  })

  await test("Entra on and it starts -> still the Entra redirect (the callback ends on the terminal)", async () => {
    fetchImpl = establishRouter()
    const restore = applyEnv(on)
    const r = initRequest({ terminal: "iframe", q: "x" })
    await authHandover.appAuthRedirect(r)
    restore()
    assert(r.returnBody.indexOf("https://login.microsoftonline.com/") === 0, "-> Entra")
  })

  await test("top-level with drop2 not starting is unchanged: carry on to r", async () => {
    const restore = applyEnv({ ...on, ENTRA_CLIENT_SECRET: "" })
    const r = initRequest({ r: DEST_OS })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, continuation(DEST_OS), "carry on")
  })

  await test("the terminal handler: 200, the static page, no cookies set", async () => {
    const r = createMockRequest({ headersIn: { Host: "proxy.example" } })
    entra.terminal(r)
    assertEqual(r.returnCode, 200, "200")
    assertEqual(r.returnBody, entra.__test.constants.TERMINAL_HTML, "the terminal page")
    assertEqual(r.headersOut["Content-Type"], "text/html; charset=utf-8", "html")
    assertEqual(r.headersOut["Set-Cookie"], undefined, "sets nothing")
  })
}


// Review fixes (2026-10-19): per-flow state cookie, replay bounds, iframe flag in `state`, the
// pointless-round-trip skip, and the iframe state without `r`.
async function reviewFixTests(authHandover, entra) {
  console.log("\nper-flow state, replay bounds, iframe flag, no-service-scope skip:")
  const on = { ENTRA_STORE_ENABLED: "true" }

  await test("two overlapping flows: each callback finds ITS OWN state and clears only its own cookie", async () => {
    const other = (await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")).__test.packState({
      s: H2, cc: CC, tok: TOKEN, ver: VERSION, r: DEST_OTHER, dk: "other", term: "top-level", iat: nowSec(), h: "proxy.example",
    })
    const r = await callbackScenario({ term: "top-level", dest: DEST_OS, extraCookie: STATE_PREFIX + H2 + "=" + other })
    assertEqual(r.returnBody, continuation(DEST_OS, OS_TOKEN_PARAMS), "flow A lands on A's destination, not B's")
    const sc = [].concat(r.headersOut["Set-Cookie"] || [])
    assert(!!findCookie(sc, STATE_PREFIX + H1 + "=deleted"), "A's cookie cleared")
    assertEqual(findCookie(sc, STATE_PREFIX + H2 + "="), undefined, "B's cookie untouched")
  })

  await test("an Entra error is matched to its own flow too (by the echoed state)", async () => {
    const other = (await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")).__test.packState({
      s: H2, cc: CC, tok: TOKEN, ver: VERSION, r: DEST_OTHER, dk: "other", term: "top-level", iat: nowSec(), h: "proxy.example",
    })
    const r = await callbackScenario({
      term: "top-level", dest: DEST_OS, adError: "login_required", extraCookie: STATE_PREFIX + H2 + "=" + other,
    })
    assertEqual(r.returnBody, continuation(DEST_OS, OS_TOKEN_PARAMS), "A's destination")
  })

  await test("a state param for a flow whose cookie is absent -> unrecoverable (never another flow's state)", async () => {
    const r = await callbackScenario({ term: "top-level", dest: DEST_OS, stateParam: "t." + H2 })
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "fallback landing")
    assertEqual(fetchLog.length, 0, "no exchange")
  })

  await test("a payload whose handle differs from its cookie name is rejected", async () => {
    const r = await callbackScenario({ term: "top-level", stPatch: { s: H2 } })
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "fallback landing")
    assertEqual(fetchLog.length, 0, "no exchange")
  })

  await test("replay: a state cookie older than 300s is never redeemed (server-side, not just the browser's Max-Age)", async () => {
    const r = await callbackScenario({ term: "top-level", stPatch: { iat: nowSec() - 301 } })
    assertEqual(fetchLog.length, 0, "no exchange")
    assert(logLines.some((l) => l.indexOf("drop2-outcome result=degraded reason=expired-state") === 0), logLines.join(" | "))
  })

  await test("expired but ours: carries on to its r (with cc) as /init would, not the fallback; no exchange, no deposit", async () => {
    for (const [dest, extra, kind] of [[DEST_OS, OS_TOKEN_PARAMS, "os"], [DEST_OTHER, "", "other"], [DEST_POLARIS, POLARIS_TOKEN_PARAMS, "polaris"]]) {
      const r = await callbackScenario({ term: "top-level", dest, stPatch: { iat: nowSec() - 301 } })
      assertEqual(r.returnBody, continuation(dest, extra), kind + ": carries on to r")
      assertEqual(fetchLog.length, 0, kind + ": no exchange, no swap, no deposit")
      const o = outcomeOf(outcomeLines()[0])
      assertEqual([o.result, o.reason, o.dest].join(","), "degraded,expired-state," + kind, kind + ": outcome names the real destination")
      assertIncludes(findCookie(r.headersOut["Set-Cookie"], STATE_PREFIX + H1 + "="), "=deleted", kind + ": state cleared")
    }
    // The interactive leg (900s) too, and an AD error on an expired flow isn't acted on (no picker).
    let r = await callbackScenario({ term: "top-level", dest: DEST_OS, env: { ENTRA_INTERACTIVE_ENABLED: "true" }, adError: "login_required", adErrorDescription: "AADSTS50058: x", stPatch: { iat: nowSec() - 301, lh: 1 } })
    assertEqual(r.returnBody, continuation(DEST_OS, OS_TOKEN_PARAMS), "expired + AD error: carries on, no retry")
    assertEqual(findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX), undefined, "and no hint change")
    r = await callbackScenario({ term: "top-level", dest: DEST_OS, stPatch: { n: 1, iat: nowSec() - 1000 } })
    assertEqual(r.returnBody, continuation(DEST_OS, OS_TOKEN_PARAMS), "expired picker leg: carries on")
    r = await callbackScenario({ term: "iframe", stPatch: { iat: nowSec() - 301 } })
    assertEqual(r.returnBody, ENTRA_TERMINAL(entra), "iframe: the terminal page")
  })

  await test("unrecoverable states still land on the fallback: future-dated, from another host", async () => {
    let r = await callbackScenario({ term: "top-level", dest: DEST_OS, stPatch: { iat: nowSec() + 120 } })
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "future-dated")
    r = await callbackScenario({ term: "top-level", dest: DEST_OS, stPatch: { iat: nowSec() - 301, h: "other.example" } })
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "expired + other host")
    assertEqual(fetchLog.length, 0, "no exchange")
  })

  await test("replay: a state cookie from the future (beyond 60s skew) is rejected; within skew accepted", async () => {
    let r = await callbackScenario({ term: "top-level", stPatch: { iat: nowSec() + 120 } })
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "rejected")
    r = await callbackScenario({ term: "top-level", stPatch: { iat: nowSec() + 30 } })
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "accepted within skew")
  })

  await test("replay: a state cookie minted for another host is rejected", async () => {
    const r = await callbackScenario({ term: "top-level", stPatch: { h: "other.example" } })
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "fallback landing")
    assertEqual(fetchLog.length, 0, "no exchange")
    assert(logLines.some((l) => l.indexOf("reason=wrong-host") !== -1), logLines.join(" | "))
  })

  await test("replay: a state cookie with no iat is rejected", async () => {
    const r = await callbackScenario({ term: "top-level", stPatch: { iat: undefined } })
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "rejected")
  })

  await test("iframe flow with its state lost -> the terminal page (from the i. flag), never the Polaris UI", async () => {
    const entra2 = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
    for (const args of [{ state: "i." + H1, code: "y" }, { state: "i." + H1, error: "login_required" }]) {
      const r = createMockRequest({ args, headersIn: { Host: "proxy.example" } })
      await entra2.handleInitEntraCallback(r)
      assertEqual(r.returnCode, 200, "200 for " + JSON.stringify(args))
      assertEqual(r.returnBody, entra.__test.constants.TERMINAL_HTML, "terminal page")
    }
  })

  await test("iframe success/degrade never needs r", async () => {
    let r = await callbackScenario({ term: "iframe" })
    assertEqual(r.returnCode, 200, "success -> terminal")
    r = await callbackScenario({ term: "iframe", mdsSwap: "fail" })
    assertEqual(r.returnCode, 200, "degrade -> terminal")
    assertEqual(outcomeOf(outcomeLines()[0]).dest, "polaris", "dest kind from the state")
  })

  await test("no MDS scope: top-level skips Entra (nothing to deposit) and carries on; logged skipped/no-mds-scope", async () => {
    fetchLog = []
    logLines = []
    const restore = applyEnv({ ...on, ENTRA_MDS_SCOPE: "" })
    const r = initRequest({ r: DEST_OS })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, continuation(DEST_OS), "carry on")
    assertEqual(fetchLog.length, 0, "no CMS lookup either")
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason].join(","), "skipped,no-mds-scope", "logged")
  })

  await test("no MDS scope: the iframe still runs when there is a watchdog scope (presence)", async () => {
    fetchImpl = establishRouter()
    const restore = applyEnv({ ...on, ENTRA_MDS_SCOPE: "" })
    const r = initRequest({ terminal: "iframe", q: "x" })
    await authHandover.appAuthRedirect(r)
    restore()
    assert(r.returnBody.indexOf("https://login.microsoftonline.com/") === 0, "-> Entra")
  })

  await test("no MDS scope and no watchdog scope: the iframe skips -> /init/terminal", async () => {
    const restore = applyEnv({ ...on, ENTRA_MDS_SCOPE: "", ENTRA_WATCHDOG_SCOPE: "" })
    const r = initRequest({ terminal: "iframe", q: "x" })
    await authHandover.appAuthRedirect(r)
    restore()
    assertEqual(r.returnBody, "https://proxy.example/init/terminal", "terminal")
  })
}

// D7: the login hint, its endpoint, and the one interactive retry.
const HINT_PREFIX = "cps-entra-login-hint="
const CLAIMS = { login_hint: "O.opaque-hint-1", preferred_username: "user@cps.gov.uk", oid: "oid-1", tid: "tid-1" }

async function d7Tests(authHandover, entra) {
  console.log("\nD7 — login hint (cookie + endpoint) and the interactive retry:")
  const on = { ENTRA_STORE_ENABLED: "true" }
  const interactive = { ENTRA_INTERACTIVE_ENABLED: "true" }
  const hintValue = (r) => cookieValue(findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX))

  await test("a successful sign-in sets the hint: signed, host-wide, HttpOnly, SameSite=None, 30 days", async () => {
    const r = await callbackScenario({ term: "top-level", idClaims: CLAIMS })
    const sc = findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX)
    assert(!!sc, "hint cookie set")
    for (const a of ["; Path=/;", "HttpOnly", "Secure", "SameSite=None", "Max-Age=2592000"]) assertIncludes(sc, a, a)
    const h = entra.__test.unpackState(cookieValue(sc))
    assertEqual([h.k, h.lh, h.u, h.oid, h.tid].join(","), "hint,O.opaque-hint-1,user@cps.gov.uk,oid-1,tid-1", "payload")
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "still carries on as before")
  })

  await test("no login_hint claim: preferred_username is the hint; neither: no hint cookie", async () => {
    let r = await callbackScenario({ term: "top-level", idClaims: { preferred_username: "u2@cps.gov.uk", oid: "o" } })
    assertEqual(entra.__test.unpackState(hintValue(r)).lh, "u2@cps.gov.uk", "UPN fallback")
    r = await callbackScenario({ term: "top-level", idClaims: { oid: "o" } })
    assertEqual(findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX), undefined, "no hint")
    r = await callbackScenario({ term: "top-level" })
    assertEqual(findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX), undefined, "garbage id_token: no hint, no crash")
  })

  await test("the hint is recorded even when the deposit later fails (the account choice still stands)", async () => {
    const r = await callbackScenario({ term: "top-level", idClaims: CLAIMS, mdsSwap: "fail" })
    assert(!!findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX), "hint set on degrade after sign-in")
    const r2 = await callbackScenario({ term: "iframe", idClaims: CLAIMS })
    assert(!!findCookie(r2.headersOut["Set-Cookie"], HINT_PREFIX), "and from the iframe")
    assertEqual(presenceCookies(r2).length, 2, "presence cookies kept alongside")
  })

  await test("/init sends the hint as login_hint (still prompt=none); outcome lines say hint=1", async () => {
    const hint = entra.__test.hintCookie(jwt(CLAIMS)).split(";")[0]
    fetchImpl = establishRouter()
    const restore = applyEnv(on)
    const r = initRequest({ r: DEST_OS }, { Cookie: hint })
    await authHandover.appAuthRedirect(r)
    restore()
    const q = form(r.returnBody.split("?")[1])
    assertEqual([q.prompt, q.login_hint].join(","), "none,O.opaque-hint-1", "silent, hinted")
    const st = entra.__test.unpackState(cookieValue(findCookie([].concat(r.headersOut["Set-Cookie"]), STATE_PREFIX)))
    assertEqual(st.lh, 1, "state records that a hint was sent")
    await callbackScenario({ term: "top-level", stPatch: { lh: 1 } })
    assertEqual(outcomeOf(outcomeLines()[0]).hint, "1", "hint=1")
  })

  await test("a forged / unsigned / state-cookie-as-hint is ignored (no login_hint sent)", async () => {
    const st = entra.__test.packState({ s: H1, iat: nowSec() })
    for (const bad of ["cps-entra-login-hint=xyz", "cps-entra-login-hint=" + entra.__test.b64urlEncode('{"k":"hint","lh":"evil"}') + ".bad", "cps-entra-login-hint=" + st]) {
      fetchImpl = establishRouter()
      const restore = applyEnv(on)
      const r = initRequest({ r: DEST_OS }, { Cookie: bad })
      await authHandover.appAuthRedirect(r)
      restore()
      assertEqual(form(r.returnBody.split("?")[1]).login_hint, undefined, "ignored: " + bad.slice(0, 40))
    }
  })

  await test("interactive ON, top-level, several accounts (16000): ONE retry with the account picker, same flow", async () => {
    const r = await callbackScenario({ term: "top-level", env: interactive, adError: "interaction_required", adErrorDescription: "AADSTS16000: pick", stPatch: { iat: nowSec() - 200 } })
    assertEqual(r.returnCode, 302, "302")
    assert(r.returnBody.indexOf("https://login.microsoftonline.com/") === 0, "back to AD: " + r.returnBody)
    const q = form(r.returnBody.split("?")[1])
    assertEqual([q.prompt, q.login_hint, q.state].join(","), "select_account,," + "t." + H1, "picker, no hint, same handle")
    const sc = findCookie(r.headersOut["Set-Cookie"], STATE_PREFIX + H1 + "=")
    assertIncludes(sc, "Max-Age=900", "longer state for the interactive leg")
    const st = entra.__test.unpackState(cookieValue(sc))
    assertEqual([st.n, st.ra, st.r, st.cc].join("|"), ["1", "16000", DEST_POLARIS, CC].join("|"), "re-signed with n=1 + why (ra), same destination + cookies")
    assert(Math.abs(st.iat - nowSec()) <= 2, "fresh iat for the interactive leg (the 900s runs from the retry): " + st.iat)
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.ad, o.aadsts, o.int].join(","), "retried,ad-error,interaction_required,16000,0", "retried line")
  })

  await test("interactive ON: login_required (no session) and MFA retry too; a CA device block (53003) does not", async () => {
    for (const [e, d, retried] of [["login_required", "AADSTS50058: x", true], ["interaction_required", "AADSTS50076: x", true], ["interaction_required", "AADSTS53003: x", false], ["consent_required", "AADSTS65001: x", false], ["access_denied", "", false]]) {
      const r = await callbackScenario({ term: "top-level", env: interactive, adError: e, adErrorDescription: d })
      assertEqual(form((r.returnBody || "").split("?")[1] || "").prompt === "select_account", retried, e + " " + d)
    }
  })

  await test("the interactive leg is never retried again: a second AD error degrades (carry on), logged int=1", async () => {
    const r = await callbackScenario({ term: "top-level", env: interactive, adError: "interaction_required", adErrorDescription: "AADSTS16000: x", stPatch: { n: 1 } })
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "carry on")
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.int].join(","), "degraded,1", "int=1")
  })

  await test("the interactive leg's success sets the hint and logs ok int=1", async () => {
    const r = await callbackScenario({ term: "top-level", env: interactive, idClaims: CLAIMS, stPatch: { n: 1 } })
    assert(!!findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX), "hint")
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.int].join(","), "ok,1", "ok int=1")
  })

  const hintVia = (r) => entra.__test.unpackState(hintValue(r)).via
  const prevHint = (via, oid) =>
    "cps-entra-login-hint=" + entra.__test.packState({ k: "hint", lh: "O.prev", u: "prev@cps.gov.uk", oid: oid, tid: "tid-1", at: nowSec() - 60, via: via })

  await test("via: silent without the picker; after it, why it was needed (16000 / 50058 / 50076 / 50079 / other)", async () => {
    let r = await callbackScenario({ term: "top-level", idClaims: CLAIMS })
    assertEqual(hintVia(r), "silent", "silent leg")
    for (const [ra, via] of [["16000", "several-accounts"], ["50058", "no-session"], ["50076", "mfa"], ["50079", "mfa"], ["50000", "other"], ["", "other"], [undefined, "other"]]) {
      r = await callbackScenario({ term: "top-level", env: interactive, idClaims: CLAIMS, stPatch: { n: 1, ra: ra } })
      assertEqual(hintVia(r), via, "picker after " + ra)
    }
  })

  await test("/init re-checks a several-accounts hint: signs in WITHOUT it (probe, p=1); any other hint is sent as before", async () => {
    for (const [via, sent, p] of [["several-accounts", undefined, 1], ["silent", "O.prev", 0], ["no-session", "O.prev", 0]]) {
      fetchImpl = establishRouter()
      const restore = applyEnv(on)
      const r = initRequest({ r: DEST_OS }, { Cookie: prevHint(via, "oid-1") })
      await authHandover.appAuthRedirect(r)
      restore()
      const q = form(r.returnBody.split("?")[1])
      assertEqual([q.prompt, q.login_hint].join(","), "none," + (sent || ""), via + ": silent, hint " + (sent ? "sent" : "withheld"))
      const st = entra.__test.unpackState(cookieValue(findCookie([].concat(r.headersOut["Set-Cookie"]), STATE_PREFIX)))
      assertEqual([st.p, st.lh].join(","), [p, 1 - p].join(","), via + ": p / lh")
    }
  })

  await test("probe meets several accounts (16000): the same flow again, silently WITH the hint; that leg ends via=several-accounts", async () => {
    let r = await callbackScenario({ term: "top-level", adError: "interaction_required", adErrorDescription: "AADSTS16000: pick", stPatch: { p: 1, iat: nowSec() - 200 }, extraCookie: prevHint("several-accounts", "oid-1") })
    const q = form(r.returnBody.split("?")[1])
    assertEqual([q.prompt, q.login_hint, q.state].join(","), "none,O.prev,t." + H1, "silent, hinted, same handle (interactive off: no picker needed)")
    const sc = findCookie(r.headersOut["Set-Cookie"], STATE_PREFIX + H1 + "=")
    assertIncludes(sc, "Max-Age=300", "a silent leg's lifetime")
    const st = entra.__test.unpackState(cookieValue(sc))
    assertEqual([st.p, st.m, st.lh, st.n, st.r].join("|"), ["0", "1", "1", "", DEST_POLARIS].join("|"), "p=0 (no loop), m=1, lh=1, not interactive")
    assert(Math.abs(st.iat - nowSec()) <= 2, "fresh iat")
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason, o.aadsts, o.hint].join(","), "retried,several-accounts,16000,0", "retried line")
    r = await callbackScenario({ term: "top-level", idClaims: CLAIMS, stPatch: { m: 1, lh: 1 } })
    assertEqual(hintVia(r), "several-accounts", "the hinted leg records it")
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "and carries on")
  })

  await test("probe signs in as the hint's account: only one account left -> via=silent, no extra leg", async () => {
    const r = await callbackScenario({ term: "top-level", idClaims: CLAIMS, stPatch: { p: 1 }, extraCookie: prevHint("several-accounts", "oid-1") })
    assertEqual(hintVia(r), "silent", "silent now")
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "carries on")
    assertEqual(outcomeOf(outcomeLines()[0]).result, "ok", "ok")
  })

  await test("probe signs in as ANOTHER account (an ambient one won): hinted leg, the user's choice kept, nothing deposited", async () => {
    const r = await callbackScenario({ term: "top-level", idClaims: CLAIMS, stPatch: { p: 1 }, extraCookie: prevHint("several-accounts", "oid-2") })
    const q = form(r.returnBody.split("?")[1])
    assertEqual([q.prompt, q.login_hint].join(","), "none,O.prev", "hinted silent leg")
    assertEqual(findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX), undefined, "hint not overwritten")
    assertEqual(callOrder(), "authorization_code", "no swap, no deposit")
    const st = entra.__test.unpackState(cookieValue(findCookie(r.headersOut["Set-Cookie"], STATE_PREFIX + H1 + "=")))
    assertEqual([st.p, st.m].join(","), "0,1", "p=0, m=1")
    assertEqual(outcomeOf(outcomeLines()[0]).reason, "other-account", "retried other-account")
  })

  await test("no probe loop / normal paths: the hinted leg's 16000 takes the picker; a probe's 50058 too; no hint cookie: normal", async () => {
    let r = await callbackScenario({ term: "top-level", env: interactive, adError: "interaction_required", adErrorDescription: "AADSTS16000: x", stPatch: { m: 1, lh: 1 }, extraCookie: prevHint("several-accounts", "oid-1") })
    assertEqual(form(r.returnBody.split("?")[1]).prompt, "select_account", "hinted leg -> picker")
    r = await callbackScenario({ term: "top-level", env: interactive, adError: "login_required", adErrorDescription: "AADSTS50058: x", stPatch: { p: 1 }, extraCookie: prevHint("several-accounts", "oid-1") })
    assertEqual(form(r.returnBody.split("?")[1]).prompt, "select_account", "probe, no session -> picker")
    r = await callbackScenario({ term: "top-level", adError: "interaction_required", adErrorDescription: "AADSTS16000: x", stPatch: { p: 1 } })
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "hint gone mid-flow, interactive off: carry on")
    r = await callbackScenario({ term: "top-level", idClaims: CLAIMS, stPatch: { p: 1 } })
    assertEqual(hintVia(r), "silent", "hint gone mid-flow, probe signs in: silent")
  })

  await test("probe -> picker: the user's pick stands (not probed again): new hint, deposit, logged hint=0", async () => {
    // Hint is account A (oid-2, several accounts); A's session has gone, so the probe gets 50058.
    let r = await callbackScenario({ term: "top-level", env: interactive, adError: "login_required", adErrorDescription: "AADSTS50058: x", stPatch: { p: 1, lh: 0 }, extraCookie: prevHint("several-accounts", "oid-2") })
    const st = entra.__test.unpackState(cookieValue(findCookie(r.headersOut["Set-Cookie"], STATE_PREFIX + H1 + "=")))
    assertEqual([st.n, st.p, st.lh].join(","), "1,0,0", "picker leg: n=1, no longer the probe, no hint sent")
    // The user picks B (oid-1) in the picker.
    r = await callbackScenario({ term: "top-level", env: interactive, idClaims: CLAIMS, stPatch: st, extraCookie: prevHint("several-accounts", "oid-2") })
    const h = entra.__test.unpackState(hintValue(r))
    assertEqual([h.oid, h.via].join(","), "oid-1,no-session", "B is the new hint")
    assertEqual(callOrder(), "authorization_code > refresh_token:mds > mds", "swapped + deposited, no hinted leg back to A")
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "carries on")
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.int, o.hint].join(","), "ok,1,0", "ok int=1 hint=0")
  })

  await test("stale hint: a hinted leg's 50058 (hinted account gone) with no retry forgets the hint, carries on, logs hint-cleared=1", async () => {
    for (const term of ["top-level", "iframe"]) {
      const r = await callbackScenario({ term, adError: "login_required", adErrorDescription: "AADSTS50058: no match for hint", stPatch: { lh: 1 }, extraCookie: prevHint("silent", "oid-2") })
      const sc = findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX)
      assert(!!sc, term + ": hint cookie cleared")
      for (const a of ["=deleted", "; Path=/;", "Secure", "SameSite=None", "Max-Age=0"]) assertIncludes(sc, a, term + " " + a)
      assertEqual(r.returnBody, term === "iframe" ? ENTRA_TERMINAL(entra) : continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), term + ": carries on")
      const o = outcomeOf(outcomeLines()[0])
      assertEqual([o.result, o.aadsts, o["hint-cleared"], o.hint].join(","), "degraded,50058,1,1", term + ": outcome")
    }
  })

  await test("stale hint: kept when unhinted (lh=0), on other errors (MFA, CA block), or when the picker retries instead", async () => {
    const kept = (r, why) => assertEqual(findCookie(r.headersOut["Set-Cookie"] || [], HINT_PREFIX), undefined, why)
    kept(await callbackScenario({ term: "top-level", adError: "login_required", adErrorDescription: "AADSTS50058: x", stPatch: { lh: 0 } }), "unhinted 50058")
    kept(await callbackScenario({ term: "top-level", adError: "interaction_required", adErrorDescription: "AADSTS50076: x", stPatch: { lh: 1 } }), "hinted MFA")
    kept(await callbackScenario({ term: "top-level", adError: "interaction_required", adErrorDescription: "AADSTS53003: x", stPatch: { lh: 1 } }), "hinted CA block")
    const r = await callbackScenario({ term: "top-level", env: interactive, adError: "login_required", adErrorDescription: "AADSTS50058: x", stPatch: { lh: 1 } })
    assertEqual(form(r.returnBody.split("?")[1]).prompt, "select_account", "interactive on: picker (its pick overwrites the hint)")
    kept(r, "not cleared on the picker hand-over")
  })

  await test("a repeated query arg (?state=a&state=b) reads as missing: bad-state-param, never a throw", async () => {
    const r = await callbackScenario({ term: "top-level", stateParam: ["t." + H1, "t." + H2] })
    assertEqual(outcomeOf(outcomeLines()[0]).reason, "bad-state-param", "bad-state-param, not callback-error")
    assertEqual(fetchLog.length, 0, "no exchange")
    assertEqual(r.returnBody, "https://proxy.example/polaris-ui/", "unrecoverable: fallback landing")
  })

  await test("iframe, watchdog scope but no MDS scope: presence-only is ok (not degraded swap-failed)", async () => {
    const r = await callbackScenario({ term: "iframe", env: { ENTRA_MDS_SCOPE: "" } })
    assertEqual(presenceCookies(r).length, 2, "presence cookies set")
    assertEqual(callOrder(), "authorization_code > refresh_token:watchdog", "no MDS swap or deposit")
    const o = outcomeOf(outcomeLines()[0])
    assertEqual([o.result, o.reason].join(","), "ok,presence-only", "ok presence-only")
    assertEqual(r.returnBody, ENTRA_TERMINAL(entra), "terminal page")
  })

  await test("presence token too large for a cookie: not set, logged with its size; the deposit still runs", async () => {
    const r = await callbackScenario({ term: "iframe", watchdogToken: "x".repeat(3901) })
    assertEqual(presenceCookies(r).length, 0, "no presence cookie a browser would drop")
    assert(logLines.some((l) => l.indexOf("presence token too large — bytes=3901") !== -1), logLines.join(" | "))
    assertEqual(outcomeOf(outcomeLines()[0]).result, "ok", "deposit ok")
    const ok = await callbackScenario({ term: "iframe", watchdogToken: "x".repeat(3900) })
    assertEqual(presenceCookies(ok).length, 2, "at the limit: set")
  })

  await test("PKCE: S256 challenge = base64url(SHA-256(verifier)), checked against Node's crypto", () => {
    const v = "dBjftJeZ4CVP-mJ92K1ifu-5j1RMn2O3QNlmNKyIqTk"
    assertEqual(entra.__test.pkceChallenge(v), require("crypto").createHash("sha256").update(v).digest("base64url"), "S256")
  })

  await test("PKCE: /init keeps a fresh verifier in the signed state and sends only its S256 challenge", async () => {
    const vs = []
    for (let i = 0; i < 2; i++) {
      fetchImpl = establishRouter()
      const restore = applyEnv(on)
      const r = initRequest({ r: DEST_OS })
      await authHandover.appAuthRedirect(r)
      restore()
      const st = entra.__test.unpackState(cookieValue(findCookie([].concat(r.headersOut["Set-Cookie"]), STATE_PREFIX)))
      assert(/^[0-9a-f]{64}$/.test(st.v || ""), "verifier: 64 hex chars: " + st.v)
      const q = form(r.returnBody.split("?")[1])
      assertEqual([q.code_challenge, q.code_challenge_method].join(","), entra.__test.pkceChallenge(st.v) + ",S256", "challenge of the verifier")
      assert(r.returnBody.indexOf(st.v) === -1, "the verifier itself never goes to /authorize")
      vs.push(st.v)
    }
    assert(vs[0] !== vs[1], "fresh per flow")
  })

  await test("PKCE: the exchange sends the state's verifier; a pre-PKCE state (no v) exchanges without one", async () => {
    const V = "c".repeat(64)
    await callbackScenario({ term: "top-level", stPatch: { v: V } })
    assertEqual(form(fetchLog[0].init.body).code_verifier, V, "code_verifier")
    await callbackScenario({ term: "top-level" })
    assertEqual(form(fetchLog[0].init.body).code_verifier, undefined, "none without v")
    assertEqual(outcomeOf(outcomeLines()[0]).result, "ok", "and still works")
  })

  await test("PKCE: the picker and the hinted legs reuse the flow's verifier (same challenge)", async () => {
    const V = "d".repeat(64)
    const want = entra.__test.pkceChallenge(V)
    let r = await callbackScenario({ term: "top-level", env: interactive, adError: "login_required", adErrorDescription: "AADSTS50058: x", stPatch: { v: V } })
    assertEqual(form(r.returnBody.split("?")[1]).code_challenge, want, "picker leg")
    r = await callbackScenario({ term: "top-level", adError: "interaction_required", adErrorDescription: "AADSTS16000: x", stPatch: { p: 1, v: V }, extraCookie: prevHint("several-accounts", "oid-1") })
    assertEqual(form(r.returnBody.split("?")[1]).code_challenge, want, "hinted leg")
  })

  await test("iframe: the probe and the hinted leg work there too (all silent)", async () => {
    const r = await callbackScenario({ term: "iframe", adError: "interaction_required", adErrorDescription: "AADSTS16000: x", stPatch: { p: 1 }, extraCookie: prevHint("several-accounts", "oid-1") })
    const q = form(r.returnBody.split("?")[1])
    assertEqual([q.prompt, q.login_hint, q.state].join(","), "none,O.prev,i." + H1, "hinted, iframe state")
  })

  await test("interactive state lives 900s; the silent leg's 300s still applies without n", async () => {
    let r = await callbackScenario({ term: "top-level", stPatch: { n: 1, iat: nowSec() - 600 } })
    assertEqual(outcomeOf(outcomeLines()[0]).result, "ok", "interactive leg at 600s ok")
    r = await callbackScenario({ term: "top-level", stPatch: { n: 1, iat: nowSec() - 1000 } })
    assertEqual(outcomeOf(outcomeLines()[0]).reason, "expired-state", "interactive leg at 1000s expired")
    r = await callbackScenario({ term: "top-level", stPatch: { iat: nowSec() - 600 } })
    assertEqual(outcomeOf(outcomeLines()[0]).reason, "expired-state", "silent leg at 600s expired")
  })

  await test("interactive OFF (prod), or the iframe: no retry, degrade as before", async () => {
    let r = await callbackScenario({ term: "top-level", adError: "interaction_required", adErrorDescription: "AADSTS16000: x" })
    assertEqual(r.returnBody, continuation(DEST_POLARIS, POLARIS_TOKEN_PARAMS), "off: carry on")
    r = await callbackScenario({ term: "iframe", env: interactive, adError: "interaction_required", adErrorDescription: "AADSTS16000: x" })
    assertEqual(r.returnBody, ENTRA_TERMINAL(entra), "iframe: terminal page, never a prompt")
  })

  await test("hintCorsOrigin: global-components' $cors_origin, https only", async () => {
    for (const [o, want] of [["https://app.cps.gov.uk", "https://app.cps.gov.uk"], ["http://app.cps.gov.uk", ""], ["http://localhost:3000", ""], ["", ""]]) {
      const r = createMockRequest({ variables: { cors_origin: o } })
      assertEqual(entra.hintCorsOrigin(r), want, o || "(none)")
    }
  })

  await test("endpoint GET: the hint as JSON (no-store); no / forged cookie -> null", async () => {
    const hint = entra.__test.hintCookie(jwt(CLAIMS)).split(";")[0]
    let r = createMockRequest({ method: "GET", headersIn: { Cookie: "a=1; " + hint } })
    entra.loginHint(r)
    assertEqual(r.returnCode, 200, "200")
    assertEqual(r.headersOut["Cache-Control"], "no-store", "no-store")
    const j = JSON.parse(r.returnBody)
    assertEqual([j.loginHint, j.username, j.oid, j.tid, j.via].join(","), "O.opaque-hint-1,user@cps.gov.uk,oid-1,tid-1,silent", "fields")
    assert(Math.abs(j.setAt - nowSec()) <= 2, "setAt")
    const old = "cps-entra-login-hint=" + entra.__test.packState({ k: "hint", lh: "O.old", at: nowSec() })
    r = createMockRequest({ method: "GET", headersIn: { Cookie: old } })
    entra.loginHint(r)
    assertEqual(JSON.parse(r.returnBody).via, "unknown", "a hint from before via: unknown")
    for (const c of ["", "cps-entra-login-hint=xyz"]) {
      r = createMockRequest({ method: "GET", headersIn: { Cookie: c } })
      entra.loginHint(r)
      assertEqual(r.returnBody, "null", "null for: " + c)
    }
  })

  await test("endpoint DELETE clears the hint (same name/path/attributes, Max-Age=0); other methods 405", async () => {
    let r = createMockRequest({ method: "DELETE" })
    entra.loginHint(r)
    const sc = findCookie(r.headersOut["Set-Cookie"], HINT_PREFIX)
    for (const a of ["; Path=/;", "SameSite=None", "Max-Age=0"]) assertIncludes(sc, a, a)
    assertEqual(JSON.parse(r.returnBody).cleared, true, "cleared")
    r = createMockRequest({ method: "PUT" })
    entra.loginHint(r)
    assertEqual(r.returnCode, 405, "405")
  })
}

const ENTRA_TERMINAL = (entra) => entra.__test.constants.TERMINAL_HTML

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
  await iframeTerminalTests(authHandover, entra)
  await reviewFixTests(authHandover, entra)
  await d7Tests(authHandover, entra)

  restore()
  process.exit(summarise("auth-handover.drop2.entra (unit)"))
}

main()
