#!/usr/bin/env node
/**
 * Unit tests for the auth-handover.drop2.entra feature (store.js + the two handlers).
 * ISOLATED / NEW-GEN: this path is dormant unless drop2 is armed (ENTRA_STORE_ENABLED=true or a
 * per-user enrolment), so it is outside the golden master. These tests pin the Entra flow's own
 * behaviour: ONE neutral sign-in (our app's scope), an on-behalf-of swap for the store backend
 * (MDS), the MDS deposit, and — on the iframe terminal — the presence token cookies.
 *
 * Drives `ngx.fetch` through a global `ngx` with a swappable router (the modules read the free
 * `ngx` global, which in node resolves to `global`).
 *
 * Loaded from the REAL config sources via njs-harness (which stages the whole config/ tree, so
 * drop2's imports of drop1 and ./store.js resolve exactly as in production).
 */
const { test, assertEqual, assert, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, applyEnv } = require("../../../tests/unit/njs-harness")

// --- ngx.fetch mock -------------------------------------------------------
let fetchImpl = async () => {
  throw new Error("no ngx.fetch stub set for this test")
}
let fetchLog = []
global.ngx = {
  fetch: (url, init) => {
    fetchLog.push({ url, init: init || {} })
    return fetchImpl(url, init || {})
  },
  log: () => {},
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
  ENTRA_APP_SCOPE: "api://00000000-0000-0000-0000-0000000000cc/api.presence.user.readwrite",
  ENTRA_MDS_SCOPE: "api://fa-wm-app-test/full_scope",
  WM_MDS_BASE_URL: "https://fa-wm-app-test.azurewebsites.net/api/",
  WM_MDS_ACCESS_KEY: "test-mds-key",
  WEBSITE_SCHEME: "https",
  ENDPOINT_HTTP_PROTOCOL: "https",
}

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
    assertEqual(body.expiryTime, "2000-01-01T00:00:00Z", "the agreed fixed expiryTime")
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
  console.log("\nhelpers — state packing, on-behalf-of, presence cookies:")

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

  await test("obo: jwt-bearer grant, on_behalf_of, the user's token as assertion, the target scope", async () => {
    fetchLog = []
    fetchImpl = async () => res({ body: JSON.stringify({ access_token: "MDS-TOKEN", expires_in: 3599 }) })
    const out = await entra.__test.obo("NEUTRAL-TOKEN", "api://fa-wm-app-test/full_scope")
    assertEqual(out.accessToken, "MDS-TOKEN", "swapped token")
    const f = form(fetchLog[0].init.body)
    assertEqual(f.grant_type, "urn:ietf:params:oauth:grant-type:jwt-bearer", "grant")
    assertEqual(f.requested_token_use, "on_behalf_of", "OBO")
    assertEqual(f.assertion, "NEUTRAL-TOKEN", "assertion")
    assertEqual(f.scope, "api://fa-wm-app-test/full_scope", "scope")
    assertEqual(f.client_id, ENV.ENTRA_CLIENT_ID, "client id")
    assert(fetchLog[0].url.indexOf("/" + ENV.ENTRA_TENANT_ID + "/oauth2/v2.0/token") !== -1, "tenant token endpoint")
  })

  await test("obo surfaces AD's error code (e.g. consent missing) but never a token", async () => {
    fetchImpl = async () =>
      res({ status: 400, body: JSON.stringify({ error: "invalid_grant", error_codes: [65001] }) })
    const out = await entra.__test.obo("NEUTRAL-TOKEN", "api://x/full_scope")
    assertEqual(out.accessToken, "", "no token")
    assert(out.diag.indexOf("invalid_grant") !== -1 && out.diag.indexOf("AADSTS65001") !== -1, "diag: " + out.diag)
  })

  await test("presenceCookies: one HttpOnly cookie per presence path, Max-Age = token lifetime", () => {
    const cs = entra.__test.presenceCookies("NEUTRAL-TOKEN", 3599)
    assertEqual(cs.length, 2, "two paths")
    assert(cs[0].indexOf("cms-auth-presence-token=NEUTRAL-TOKEN; Path=/global-components/presence-jsonp;") === 0, cs[0])
    assert(cs[1].indexOf("; Path=/global-components/case-locking;") !== -1, cs[1])
    cs.forEach((c) => assert(/HttpOnly; Secure; SameSite=Lax; Max-Age=3599$/.test(c), c))
  })
}

async function beginTests(entra) {
  console.log("\nhandleInitEntra — establish (drop1) then 302 to Entra /authorize for the NEUTRAL scope:")

  await test("silent AD redirect: neutral scope, prompt=none, no nonce; state cookie carries the session", async () => {
    fetchImpl = establishRouter()
    const r = createMockRequest({
      args: { cc: "ASP.NET_SessionId=x; .CMSAUTHa=y", "polaris-ui-url": "/polaris-ui/case/1" },
      headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" },
    })
    await entra.handleInitEntra(r)
    assertEqual(r.returnCode, 302, "302")
    assert(r.returnBody.indexOf("https://login.microsoftonline.com/" + ENV.ENTRA_TENANT_ID + "/oauth2/v2.0/authorize?") === 0, "-> AD authorize")
    const q = form(r.returnBody.split("?")[1])
    assertEqual(q.scope, ENV.ENTRA_APP_SCOPE, "neutral scope only (no openid/profile/email)")
    assertEqual(q.prompt, "none", "silent")
    assertEqual(q.response_type, "code", "code flow")
    assertEqual(q.redirect_uri, "https://proxy.example/init-entra/callback", "callback on this host")
    assertEqual(q.nonce, undefined, "no nonce (no id_token in this model)")
    assert(/^[0-9a-f]{32}$/.test(q.state), "random state handle")
    const sc = findCookie(r.headersOut["Set-Cookie"], "entra_auth_state=")
    assert(!!sc && sc.indexOf("HttpOnly") !== -1 && sc.indexOf("Secure") !== -1, "HttpOnly+Secure state cookie")
    const st = entra.__test.unpackState(sc.slice("entra_auth_state=".length).split(";")[0])
    assertEqual(st.tok, "00000000-0000-0000-0000-000000000001", "state carries the minted modern token")
    assertEqual(st.term, "top-level", "defaults to top-level")
    assertEqual(st.n, undefined, "no nonce in state")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra")
  })

  await test("terminal=iframe is carried into the state", async () => {
    fetchImpl = establishRouter()
    const r = createMockRequest({
      args: { cc: "ASP.NET_SessionId=x; .CMSAUTHa=y", terminal: "iframe" },
      headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" },
    })
    await entra.handleInitEntra(r)
    const sc = findCookie(r.headersOut["Set-Cookie"], "entra_auth_state=")
    assertEqual(entra.__test.unpackState(sc.slice("entra_auth_state=".length).split(";")[0]).term, "iframe", "iframe")
  })

  await test("no cookies -> drop1 fail-redirect, no AD hop", async () => {
    fetchImpl = establishRouter()
    const r = createMockRequest({ args: { "polaris-ui-url": "/polaris-ui/" }, headersIn: { Host: "proxy.example" } })
    await entra.handleInitEntra(r)
    assertEqual(r.returnCode, 302, "302")
    assert(r.returnBody.indexOf("auth-fail-reason=no-cookies") !== -1, "fail-redirect")
    assert(r.returnBody.indexOf("login.microsoftonline.com") === -1, "no AD hop")
  })
}

// Drive the callback with a valid state cookie + code, routing exchange / OBO / MDS.
async function callbackScenario({ term, exchange = "ok", obo = "ok", mdsStatus = 204, adError = null, sendNeutral = false }) {
  const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
  entra.__test.setMdsSendNeutralToken(sendNeutral)
  const st = {
    s: "STATE-HANDLE",
    cc: "ASP.NET_SessionId=x; WindowID=MASTER",
    tok: "00000000-0000-0000-0000-000000000001",
    ver: "CMS.24.0.01",
    ui: "/polaris-ui/case/1",
    q: "",
    term: term,
    corr: "corr-1",
  }
  fetchLog = []
  fetchImpl = async (url, init) => {
    if (url.indexOf("/oauth2/v2.0/token") !== -1) {
      const f = form(init.body)
      if (f.grant_type === "authorization_code") {
        return exchange === "ok"
          ? res({ body: JSON.stringify({ access_token: "NEUTRAL-TOKEN", expires_in: 3599 }) })
          : res({ status: 400, body: JSON.stringify({ error: "invalid_grant" }) })
      }
      return obo === "ok"
        ? res({ body: JSON.stringify({ access_token: "MDS-TOKEN", expires_in: 3599 }) })
        : res({ status: 400, body: JSON.stringify({ error: "invalid_grant", error_codes: [65001] }) })
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

const PRESENCE_PREFIX = "cms-auth-presence-token="
const presenceCookies = (r) => (r.headersOut["Set-Cookie"] || []).filter((c) => c.indexOf(PRESENCE_PREFIX) === 0)

async function callbackTests() {
  console.log("\nhandleInitEntraCallback — interim switch MDS_SEND_NEUTRAL_TOKEN:")

  await test("shipped default: MDS_SEND_NEUTRAL_TOKEN is ON (QA interim — turn off once admin consent lands)", async () => {
    const fresh = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
    assertEqual(fresh.__test.getMdsSendNeutralToken(), true, "shipped value")
  })

  await test("switch ON: no OBO; MDS gets the NEUTRAL token; works with no consent", async () => {
    const r = await callbackScenario({ term: "top-level", sendNeutral: true, obo: "fail" })
    const calls = fetchLog.map((c) => (c.url.indexOf("cms-auth-store") !== -1 ? "mds" : form(c.init.body).grant_type))
    assertEqual(calls.join(" > "), "authorization_code > mds", "no swap attempted")
    assertEqual(fetchLog[1].init.headers.Authorization, "Bearer NEUTRAL-TOKEN", "neutral token to MDS")
    assertEqual(r.returnBody, "/polaris-ui/case/1", "-> landing")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra (deposit succeeded)")
  })

  await test("switch ON, iframe: presence cookies + MDS deposit with the neutral token", async () => {
    const r = await callbackScenario({ term: "iframe", sendNeutral: true })
    assertEqual(presenceCookies(r).length, 2, "presence cookies")
    assertEqual(fetchLog[fetchLog.length - 1].init.headers.Authorization, "Bearer NEUTRAL-TOKEN", "neutral to MDS")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker")
  })

  await test("switch ON: an MDS failure still degrades (lands + Cms-Auth-Values)", async () => {
    const r = await callbackScenario({ term: "top-level", sendNeutral: true, mdsStatus: 500 })
    assertEqual(r.returnBody, "/polaris-ui/case/1", "-> landing")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker")
  })

  console.log("\nhandleInitEntraCallback — switch OFF (proper): neutral token -> OBO -> MDS deposit -> finalize / degrade:")

  await test("top-level success: exchange(neutral) -> OBO(MDS) -> PUT; 302 landing + Cms-Auth-Values; no presence cookie", async () => {
    const r = await callbackScenario({ term: "top-level" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "/polaris-ui/case/1", "-> landing")
    const sc = r.headersOut["Set-Cookie"]
    assert(!!findCookie(sc, "Cms-Auth-Values="), "Cms-Auth-Values still set (additive)")
    assert(findCookie(sc, "entra_auth_state=deleted") !== undefined, "state cookie cleared")
    assertEqual(presenceCookies(r).length, 0, "no presence cookie on top-level")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra")
    const calls = fetchLog.map((c) => (c.url.indexOf("cms-auth-store") !== -1 ? "mds" : form(c.init.body).grant_type))
    assertEqual(calls.join(" > "), "authorization_code > urn:ietf:params:oauth:grant-type:jwt-bearer > mds", "call order")
    assertEqual(form(fetchLog[0].init.body).scope, ENV.ENTRA_APP_SCOPE, "exchange asks for the SAME neutral scope")
    assertEqual(form(fetchLog[1].init.body).assertion, "NEUTRAL-TOKEN", "OBO swaps the neutral token")
    assertEqual(form(fetchLog[1].init.body).scope, ENV.ENTRA_MDS_SCOPE, "for the store's scope")
    assertEqual(fetchLog[2].init.headers.Authorization, "Bearer MDS-TOKEN", "MDS gets the swapped token")
  })

  await test("iframe success: 200 terminal, presence cookies (neutral token, token lifetime), NO Cms-Auth-Values", async () => {
    const r = await callbackScenario({ term: "iframe" })
    assertEqual(r.returnCode, 200, "200")
    assert(r.returnBody.indexOf('data-cms-auth="done"') !== -1, "renders the terminal page")
    assertEqual(findCookie(r.headersOut["Set-Cookie"], "Cms-Auth-Values="), undefined, "no Cms-Auth-Values")
    const pc = presenceCookies(r)
    assertEqual(pc.length, 2, "both presence paths")
    pc.forEach((c) => assert(c.indexOf(PRESENCE_PREFIX + "NEUTRAL-TOKEN;") === 0 && /Max-Age=3599$/.test(c), c))
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra", "marker: entra")
  })

  await test("OBO failure (consent missing) degrades: top-level still lands + Cms-Auth-Values; no MDS call", async () => {
    const r = await callbackScenario({ term: "top-level", obo: "fail" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "/polaris-ui/case/1", "-> landing (login not blocked)")
    assert(!!findCookie(r.headersOut["Set-Cookie"], "Cms-Auth-Values="), "Cms-Auth-Values set (degraded to drop1)")
    assertEqual(fetchLog.filter((c) => c.url.indexOf("cms-auth-store") !== -1).length, 0, "no MDS call")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker: entra-degraded")
  })

  await test("OBO failure in the iframe still sets the presence cookies (presence is independent of the store)", async () => {
    const r = await callbackScenario({ term: "iframe", obo: "fail" })
    assertEqual(r.returnCode, 200, "terminal")
    assertEqual(presenceCookies(r).length, 2, "presence cookies kept")
    assertEqual(r.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker: entra-degraded")
  })

  await test("MDS failure degrades: top-level lands + Cms-Auth-Values; iframe keeps presence cookies", async () => {
    const top = await callbackScenario({ term: "top-level", mdsStatus: 401 })
    assertEqual(top.returnBody, "/polaris-ui/case/1", "-> landing")
    assert(!!findCookie(top.headersOut["Set-Cookie"], "Cms-Auth-Values="), "Cms-Auth-Values set")
    assertEqual(top.headersOut["X-Polaris-Auth-Init"], "entra-degraded", "marker")
    const ifr = await callbackScenario({ term: "iframe", mdsStatus: 500 })
    assertEqual(presenceCookies(ifr).length, 2, "presence cookies kept")
  })

  await test("code-exchange failure degrades with NO presence cookie and no OBO", async () => {
    const r = await callbackScenario({ term: "iframe", exchange: "fail" })
    assertEqual(r.returnCode, 200, "terminal")
    assertEqual(presenceCookies(r).length, 0, "no token -> no presence cookie")
    assertEqual(fetchLog.length, 1, "only the failed exchange")
  })

  await test("AD error (login_required) degrades: still lands via drop1", async () => {
    const r = await callbackScenario({ term: "top-level", adError: "login_required" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "/polaris-ui/case/1", "-> landing")
    assert(!!findCookie(r.headersOut["Set-Cookie"], "Cms-Auth-Values="), "Cms-Auth-Values set")
  })

  await test("missing state cookie: no recoverable session -> fallback landing", async () => {
    const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
    fetchImpl = async () => {
      throw new Error("should not fetch")
    }
    const r = createMockRequest({ args: { state: "x", code: "y" }, headersIn: { Host: "proxy.example" } })
    await entra.handleInitEntraCallback(r)
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "/polaris-ui/", "fallback landing")
  })
}

async function main() {
  // Module consts read env at load; apply before loading.
  const restore = applyEnv(ENV)
  const store = await loadNjs("features/auth-handover.drop2.entra/store.js")
  const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")

  await storeTests(store)
  await helperTests(entra)
  await beginTests(entra)
  await callbackTests()

  restore()
  process.exit(summarise("auth-handover.drop2.entra (unit)"))
}

main()
