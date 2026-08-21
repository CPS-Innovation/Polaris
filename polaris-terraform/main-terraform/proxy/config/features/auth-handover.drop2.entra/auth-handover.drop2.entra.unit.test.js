#!/usr/bin/env node
/**
 * Unit tests for the auth-handover.drop2.entra feature (store.js + the two handlers).
 * ISOLATED / NEW-GEN: this path is dormant unless ENTRA_STORE_ENABLED=true, so it is
 * outside the golden master. These tests pin the new Entra flow's own behaviour.
 *
 * FIRST njs unit test to drive `ngx.fetch` — so it also establishes the mock pattern:
 * a global `ngx` with a swappable `fetch` router (there is no such fixture in the
 * harness; the modules read the free `ngx` global, which in node resolves to `global`).
 *
 * Loaded from the REAL config sources via njs-harness (which stages the whole config/
 * tree, so drop2's imports of drop1 and ./store.js resolve exactly as in production).
 */
const nodeCrypto = require("crypto")
const { test, assertEqual, assert, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, applyEnv } = require("../../../tests/unit/njs-harness")

// --- ngx.fetch mock -------------------------------------------------------
// The modules call the free global `ngx`. Point fetch at a per-scenario router.
let fetchImpl = async () => {
  throw new Error("no ngx.fetch stub set for this test")
}
global.ngx = {
  fetch: (...args) => fetchImpl(...args),
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

// base64url a JSON object (for crafting id_tokens + state).
function b64url(obj) {
  return Buffer.from(JSON.stringify(obj))
    .toString("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "")
}
function makeIdToken(claims) {
  return b64url({ alg: "RS256", typ: "JWT" }) + "." + b64url(claims) + ".sig"
}
function findCookie(arr, prefix) {
  return (arr || []).find((c) => c.indexOf(prefix) === 0)
}

// A router that establishes a CMS session (drop1's mint + verify loopbacks).
function establishRouter() {
  return async (url) => {
    if (/\/CMS$/.test(url)) return res({ headers: { Location: "/CMS.24.0.01/x" } })
    if (url.indexOf("uainGeneratedScript") !== -1)
      // Must be a real GUID: drop1's _mintModernToken now pins the 8-4-4-4-12 shape (it feeds
      // the GraphQL `$guid: UUID!` verify). A non-GUID here would correctly fail extraction.
      return res({ body: "var SESS_MODERN_USER_SESSION_ID = '3f2504e0-4f89-41d3-9a0c-0305e82c3301';" })
    if (url.indexOf("/graphql/") !== -1)
      return res({ body: JSON.stringify({ data: { user: { partyId: 1 } } }) })
    throw new Error("establishRouter: unexpected url " + url)
  }
}

// ---------------------------------------------------------------------------

async function storeTests(store) {
  console.log("\nstore.js — SharedKeyLite signing + Table Storage deposit:")

  await test("sharedKeyLite matches an independent HMAC of the documented string-to-sign", () => {
    const account = "acct"
    const key = "c2VjcmV0" // base64("secret")
    const date = "Fri, 01 Jan 2021 00:00:00 GMT"
    const resource = "cmsauth(PartitionKey='oid1',RowKey='cmsAuth')"
    const stringToSign = date + "\n/" + account + "/" + resource
    const expected =
      "SharedKeyLite " +
      account +
      ":" +
      nodeCrypto.createHmac("sha256", Buffer.from(key, "base64")).update(stringToSign).digest("base64")
    assertEqual(store.__test.sharedKeyLite(account, key, date, resource), expected)
  })

  await test("deposit() success -> {ok:true} and PUTs oid-keyed JSON with SharedKeyLite auth", async () => {
    let captured = null
    fetchImpl = async (url, opts) => {
      captured = { url, opts }
      return res({ status: 204 })
    }
    const r = await store.deposit(
      "OID-1",
      { cookies: "c=1", modernToken: "3f2504e0-4f89-41d3-9a0c-0305e82c3301", correlationId: "corr", email: "a@b.gov.uk" },
      { idToken: "id", accessToken: "at" },
    )
    assertEqual(r.ok, true, "ok")
    assert(captured.url.indexOf("PartitionKey='OID-1'") !== -1, "url keys by OID")
    assertEqual(captured.opts.method, "PUT", "PUT")
    assert(captured.opts.headers.Authorization.indexOf("SharedKeyLite ") === 0, "SharedKeyLite auth")
    const sent = JSON.parse(captured.opts.body)
    assertEqual(sent.PartitionKey, "OID-1", "PartitionKey")
    assertEqual(sent.RowKey, "cmsAuth", "RowKey")
    assertEqual(JSON.parse(sent.Value).modernToken, "3f2504e0-4f89-41d3-9a0c-0305e82c3301", "Value carries the modern token")
  })

  await test("deposit() propagates a store failure -> {ok:false}", async () => {
    fetchImpl = async () => res({ status: 403, body: "denied" })
    const r = await store.deposit("OID-1", { email: "" }, {})
    assertEqual(r.ok, false, "not ok")
    assert(r.diag.indexOf("403") !== -1, "diag carries the status")
  })

  await test("deposit === tableStorageDeposit (the swap-out seam binding)", () => {
    assertEqual(store.deposit, store.tableStorageDeposit)
  })
}

async function helperTests(entra) {
  console.log("\ndrop2 helpers — state, claims, encoding:")
  const T = entra.__test

  await test("unpackState rejects a tampered payload (HMAC integrity)", () => {
    const packed = T.packState({ s: "abc", cc: "x" })
    const dot = packed.lastIndexOf(".")
    const payload = packed.slice(0, dot)
    const tampered = (payload[0] === "A" ? "B" : "A") + payload.slice(1) + packed.slice(dot)
    let threw = false
    try { T.unpackState(tampered) } catch (e) { threw = true }
    assert(threw, "a forged/tampered state cookie must be rejected")
  })

  await test("unpackState rejects a payload with no MAC", () => {
    let threw = false
    try { T.unpackState(T.packState({ s: "a" }).split(".")[0]) } catch (e) { threw = true }
    assert(threw, "no MAC -> rejected")
  })

  await test("packState/unpackState round-trips (incl. non-Latin1)", () => {
    const st = { s: "abc", n: "def", cc: "n�a=mé; b=2", tok: "T", term: "top-level" }
    assertEqual(JSON.stringify(T.unpackState(T.packState(st))), JSON.stringify(st))
  })

  await test("b64url round-trips utf-8", () => {
    assertEqual(T.b64urlDecode(T.b64urlEncode("héllo—world")), "héllo—world")
  })

  await test("decodeJwtPayload extracts claims; rejects malformed", () => {
    const tok = makeIdToken({ oid: "X", nonce: "N" })
    assertEqual(T.decodeJwtPayload(tok).oid, "X")
    assertEqual(T.decodeJwtPayload("not.a"), null, "wrong segment count -> null")
  })

  await test("rand(n) -> 2n hex chars", () => {
    assertEqual(T.rand(16).length, 32)
    assert(/^[0-9a-f]+$/.test(T.rand(8)), "hex only")
  })

  await test("validateClaims passes a good token, names each failure", () => {
    const tid = T.constants.TENANT_ID
    const good = {
      nonce: "N",
      tid: tid,
      iss: "https://login.microsoftonline.com/" + tid + "/v2.0",
      exp: Math.floor(Date.now() / 1000) + 3600,
    }
    assertEqual(T.validateClaims(good, "N"), "", "valid")
    assertEqual(T.validateClaims({ ...good, nonce: "other" }, "N"), "nonce")
    assertEqual(T.validateClaims({ ...good, tid: "wrong" }, "N"), "tid")
    assertEqual(T.validateClaims({ ...good, iss: "https://evil/" }, "N"), "iss")
    assertEqual(T.validateClaims({ ...good, exp: 1 }, "N"), "exp")
  })

  await test("validateClaims accepts the sts.windows.net issuer too", () => {
    const tid = T.constants.TENANT_ID
    assertEqual(
      T.validateClaims(
        {
          nonce: "N",
          tid: tid,
          iss: "https://sts.windows.net/" + tid + "/",
          exp: Math.floor(Date.now() / 1000) + 3600,
        },
        "N",
      ),
      "",
    )
  })
}

async function beginTests(entra) {
  console.log("\nhandleInitEntra — establish (drop1) then 302 to Entra /authorize:")
  const restore = applyEnv({ WEBSITE_SCHEME: "https", ENDPOINT_HTTP_PROTOCOL: "https" })

  await test("silent AD redirect: 302 authorize (prompt=none) + state cookie carrying the session", async () => {
    fetchImpl = establishRouter()
    const r = createMockRequest({
      args: { cc: "ASP.NET_SessionId=x; .CMSAUTHa=y", "polaris-ui-url": "/polaris-ui/case/1" },
      headersIn: { Host: "proxy.example", "X-Forwarded-Proto": "https" },
    })
    await entra.handleInitEntra(r)
    assertEqual(r.returnCode, 302, "302")
    const tid = entra.__test.constants.TENANT_ID
    assert(
      r.returnBody.indexOf("https://login.microsoftonline.com/" + tid + "/oauth2/v2.0/authorize?") === 0,
      "-> AD authorize",
    )
    assert(r.returnBody.indexOf("prompt=none") !== -1, "prompt=none (silent)")
    assert(r.returnBody.indexOf("response_type=code") !== -1, "code flow")
    assert(/[?&]state=[0-9a-f]+/.test(r.returnBody), "random state handle")
    const sc = findCookie(r.headersOut["Set-Cookie"], "entra_auth_state=")
    assert(!!sc, "state cookie set")
    assert(sc.indexOf("HttpOnly") !== -1 && sc.indexOf("Secure") !== -1, "state cookie is HttpOnly+Secure")
    const packed = sc.slice("entra_auth_state=".length).split(";")[0]
    const st = entra.__test.unpackState(packed)
    assertEqual(st.tok, "3f2504e0-4f89-41d3-9a0c-0305e82c3301", "state carries the minted modern token")
    assertEqual(st.term, "top-level", "defaults to top-level")
    assert(st.cc.indexOf("WindowID=MASTER") !== -1, "state carries the whitelisted cookies")
  })

  await test("no cookies -> drop1 fail-redirect, no AD hop", async () => {
    fetchImpl = establishRouter()
    const r = createMockRequest({
      args: { "polaris-ui-url": "/polaris-ui/" },
      headersIn: { Host: "proxy.example" },
    })
    await entra.handleInitEntra(r)
    assertEqual(r.returnCode, 302, "302")
    assert(r.returnBody.indexOf("auth-fail-reason=no-cookies") !== -1, "fail-redirect")
    assert(r.returnBody.indexOf("login.microsoftonline.com") === -1, "no AD hop")
  })

  restore()
}

// Drive the callback with a valid state cookie + code, routing token + storage fetches.
async function callbackScenario({ term, storeStatus = 204, adError = null }) {
  const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")
  const T = entra.__test
  const tid = T.constants.TENANT_ID
  const st = {
    s: "STATE-HANDLE",
    n: "NONCE-1",
    cc: "ASP.NET_SessionId=x; WindowID=MASTER",
    tok: "3f2504e0-4f89-41d3-9a0c-0305e82c3301",
    ver: "CMS.24.0.01",
    ui: "/polaris-ui/case/1",
    q: "",
    term: term,
    corr: "corr-1",
  }
  const claims = {
    oid: "OID-1",
    email: "user@cps.gov.uk",
    nonce: st.n,
    tid: tid,
    iss: "https://login.microsoftonline.com/" + tid + "/v2.0",
    exp: Math.floor(Date.now() / 1000) + 3600,
  }
  fetchImpl = async (url) => {
    if (url.indexOf("/oauth2/v2.0/token") !== -1)
      return res({ body: JSON.stringify({ id_token: makeIdToken(claims), access_token: "AT" }) })
    if (url.indexOf("table.core.windows.net") !== -1)
      return res({ status: storeStatus, body: storeStatus >= 400 ? "err" : "" })
    throw new Error("callback: unexpected url " + url)
  }
  const args = adError ? { error: adError } : { state: st.s, code: "CODE-1" }
  const r = createMockRequest({
    args: args,
    headersIn: {
      Host: "proxy.example",
      "X-Forwarded-Proto": "https",
      Cookie: "entra_auth_state=" + T.packState(st),
    },
  })
  await entra.handleInitEntraCallback(r)
  return r
}

async function callbackTests() {
  console.log("\nhandleInitEntraCallback — deposit + finalize / degrade:")
  const entra = await loadNjs("features/auth-handover.drop2.entra/auth-handover.drop2.entra.js")

  await test("top-level success: 302 landing + Cms-Auth-Values (additive), state cleared", async () => {
    const r = await callbackScenario({ term: "top-level" })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "/polaris-ui/case/1", "-> landing")
    const sc = r.headersOut["Set-Cookie"]
    assert(!!findCookie(sc, "Cms-Auth-Values="), "Cms-Auth-Values still set (additive)")
    assert(findCookie(sc, "entra_auth_state=deleted") !== undefined, "state cookie cleared")
    // The presence id-token cookie was experimental (consumer removed) — drop2 no longer sets it.
    assertEqual(findCookie(sc, "cms-auth-id-token="), undefined, "no presence id-token cookie")
  })

  await test("iframe success: 200 static terminal, NO Cms-Auth-Values", async () => {
    const r = await callbackScenario({ term: "iframe" })
    assertEqual(r.returnCode, 200, "200")
    assert(r.returnBody.indexOf('data-cms-auth="done"') !== -1, "renders the terminal page")
    const sc = r.headersOut["Set-Cookie"]
    assertEqual(findCookie(sc, "Cms-Auth-Values="), undefined, "pure side-channel: no Cms-Auth-Values")
  })

  await test("store failure degrades (top-level): still lands + Cms-Auth-Values", async () => {
    const r = await callbackScenario({ term: "top-level", storeStatus: 403 })
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "/polaris-ui/case/1", "-> landing (login not blocked)")
    const sc = r.headersOut["Set-Cookie"]
    assert(!!findCookie(sc, "Cms-Auth-Values="), "Cms-Auth-Values set (degraded to drop1)")
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
  // store consts read env at load; set the account key so deposit() attempts the PUT.
  const restore = applyEnv({
    ENTRA_STORAGE_ACCOUNT: "acct",
    ENTRA_STORAGE_KEY: "c2VjcmV0",
    ENTRA_STORAGE_TABLE: "cmsauth",
    ENTRA_CLIENT_SECRET: "test-secret",
    ENTRA_STATE_HMAC_SECRET: "test-state-hmac-secret",
  })
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
