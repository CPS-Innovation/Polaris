#!/usr/bin/env node
/**
 * Feature 2 🤝 — auth handover.  CHARACTERISATION TESTS: these record what the
 * config does TODAY, so the §6 refactor can prove it still does the same.
 *
 * Locations covered (docs/PROXY.md §4):
 *   nginx.conf:596  /auth-refresh-outbound          nginx.js handleAuthRefreshOutbound
 *   nginx.conf:604  /polaris                        nginx.js polarisAuthRedirect
 *   nginx.conf:648  /init                           nginx.js appAuthRedirect
 *   nginx.conf:662  /auth-refresh-inbound           -> DDEI /api/init/
 *   nginx.conf:666  /auth-refresh-termination       -> DDEI /api/auth-refresh-termination/
 *   nginx.conf:674  /auth-refresh-cms-modern-token  -> DDEI /api/cms-modern-token/
 *   nginx.conf:741  /dev-login/                     -> DDEI /api/login
 *   nginx.conf:772  /api/dev-login-full-cookie/     -> DDEI /api/login-full-cookie
 *
 * NOTE /polaris is tagged "5 - CMS proxy" in §4 (with an "and auth 2" note), but
 * its handler lives in nginx.js alongside the rest of the handover flow, so it is
 * tested here. Its file ownership is a decision for the §6 slicing.
 */
const {
  test,
  assert,
  assertEqual,
  assertIncludes,
  get,
  summarise,
} = require("../test-utils")

// nginx.js builds absolute redirects from X-Forwarded-Proto + Host.
const H = { "X-Forwarded-Proto": "https" }

function sessionHint(setCookie) {
  const m = /Cms-Session-Hint=([^;]+)/.exec(setCookie || "")
  if (!m) throw new Error(`No Cms-Session-Hint in: ${setCookie}`)
  return JSON.parse(decodeURIComponent(m[1]))
}

async function initTests() {
  console.log("\n/init — appAuthRedirect (whitelist + session hint):")

  await test("whitelisted r -> 302 with the cms cookie appended as cc", async () => {
    const res = await get("/init?r=/auth-refresh-inbound&cookie=session%3Dabc123", { headers: H })
    assertEqual(res.status, 302, "Should 302")
    const loc = res.headers.get("location")
    assertIncludes(loc, "/auth-refresh-inbound", "Should redirect to the r target")
    assertIncludes(loc, "cc=session%3Dabc123", "Should append the cookie as cc")
  })

  await test("non-whitelisted r -> 403", async () => {
    const res = await get("/init?r=http://evil.example.com/x&cookie=a%3Db", { headers: H })
    assertEqual(res.status, 403, "Should 403")
    assertIncludes(await res.text(), "403", "Body should explain the whitelist")
  })

  await test("relative redirect is made absolute using X-Forwarded-Proto + Host", async () => {
    const res = await get("/init?r=/auth-refresh-inbound&cookie=a%3Db", { headers: H })
    assertIncludes(res.headers.get("location"), "https://", "Should be an absolute https URL")
  })

  await test("sets Cms-Session-Hint with Path/Secure/SameSite/Expires", async () => {
    const res = await get("/init?r=/auth-refresh-inbound&cookie=a%3Db", { headers: H })
    const sc = res.headers.get("set-cookie")
    assertIncludes(sc, "Cms-Session-Hint=", "Should set the hint cookie")
    assertIncludes(sc, "Path=/", "Path=/")
    assertIncludes(sc, "Secure", "Secure")
    assertIncludes(sc, "SameSite=None", "SameSite=None")
    assertIncludes(sc, "Expires=", "Expires")
  })

  await test("session hint derives cmsDomains from a legacy _POOL cookie", async () => {
    // Realistic shape: an UPPERCASE load-balancer prefix, then the lowercase
    // domain, then _POOL — the case nginx.js's regex is written for (see its
    // comment citing "CPSACP-LTM-CM-WAN-CIN3-").
    const cookie = encodeURIComponent("CPSACP-LTM-CM-WAN-CIN2-cin2.cps.gov.uk_POOL=1234")
    const res = await get(`/init?r=/auth-refresh-inbound&cookie=${cookie}`, { headers: H })
    const hint = sessionHint(res.headers.get("set-cookie"))
    assert(
      hint.cmsDomains.includes("cin2.cps.gov.uk"),
      `Should extract cin2.cps.gov.uk, got ${JSON.stringify(hint.cmsDomains)}`
    )
    assertIncludes(hint.handoverEndpoint, "https://cin2.cps.gov.uk/polaris", "handoverEndpoint")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation, not an endorsement).
  //
  // The _POOL fallback regex is /[a-z][a-z0-9]*(?:\.[a-z][a-z0-9]*)*\.cps\.gov\.uk(?=_POOL)/
  // — it anchors on a LOWERCASE run to skip uppercase LB prefixes. If lowercase
  // letters run straight into the domain with no separator (e.g. the classic
  // F5 "BIGipServer<pool>" name), it starts mid-word and OVER-CAPTURES:
  //     BIGipServercin2.cps.gov.uk_POOL  ->  "ervercin2.cps.gov.uk"
  // which yields a bogus handoverEndpoint (https://ervercin2.cps.gov.uk/polaris).
  //
  // Recorded so the refactor cannot change it unnoticed. If this behaviour is
  // wrong in production, fix nginx.js and update this test deliberately.
  // ---------------------------------------------------------------------------
  await test("QUIRK: separator-less BIGipServer<pool> cookie over-captures the domain", async () => {
    const cookie = encodeURIComponent("BIGipServercin2.cps.gov.uk_POOL=1234")
    const res = await get(`/init?r=/auth-refresh-inbound&cookie=${cookie}`, { headers: H })
    const hint = sessionHint(res.headers.get("set-cookie"))
    assertEqual(
      JSON.stringify(hint.cmsDomains),
      JSON.stringify(["ervercin2.cps.gov.uk"]),
      "Records today's over-capture (regex starts after the 'S' of 'Server')"
    )
  })

  await test("session hint derives cmsDomains from a [CF]-TOKEN-LBsessioncookie", async () => {
    const cookie = encodeURIComponent("C-CIN3-LBsessioncookie=abc")
    const res = await get(`/init?r=/auth-refresh-inbound&cookie=${cookie}`, { headers: H })
    const hint = sessionHint(res.headers.get("set-cookie"))
    assert(
      hint.cmsDomains.includes("cin3.cps.gov.uk"),
      `Should derive cin3.cps.gov.uk from the LB cookie, got ${JSON.stringify(hint.cmsDomains)}`
    )
  })

  await test("is-proxy-session=true -> handoverEndpoint points at this proxy", async () => {
    const res = await get(
      "/init?r=/auth-refresh-inbound&cookie=a%3Db&is-proxy-session=true",
      { headers: H }
    )
    const hint = sessionHint(res.headers.get("set-cookie"))
    assertEqual(hint.isProxySession, true, "isProxySession should be true")
    assertIncludes(hint.handoverEndpoint, "/polaris", "handoverEndpoint should be this host's /polaris")
  })

  await test("legacy handover (no r param) is coerced to /auth-refresh-inbound", async () => {
    const res = await get("/init?q=SOMEQ&referer=http%3A%2F%2Fx&cookie=a%3Db", { headers: H })
    assertEqual(res.status, 302, "Should 302 (shimmed r is whitelisted)")
    const loc = res.headers.get("location")
    assertIncludes(loc, "/auth-refresh-inbound", "Should synthesise r=/auth-refresh-inbound")
    assertIncludes(loc, "q=SOMEQ", "Should carry the legacy q param through")
    assert(!/[?&]cookie=/.test(loc.split("cc=")[0]), "Should not serialise cookie into the r param")
  })
}

async function polarisTests() {
  console.log("\n/polaris — polarisAuthRedirect (simulated CMS handover):")

  await test("302s to /init carrying cookie + is-proxy-session=true", async () => {
    const res = await get("/polaris?r=/auth-refresh-inbound", {
      headers: { ...H, Cookie: "UID=abc; BIGipServercin2.cps.gov.uk_POOL=1" },
    })
    assertEqual(res.status, 302, "Should 302")
    const loc = res.headers.get("location")
    assertIncludes(loc, "/init?", "Should hand off to /init")
    assertIncludes(loc, "is-proxy-session=true", "Should mark it a proxy session")
    assertIncludes(loc, "cookie=", "Should pass the request cookies through")
  })
}

async function outboundTests() {
  console.log("\n/auth-refresh-outbound — handleAuthRefreshOutbound:")

  await test("uses handoverEndpoint from the Cms-Session-Hint cookie", async () => {
    const hint = encodeURIComponent(
      JSON.stringify({ handoverEndpoint: "https://cin4.cps.gov.uk/polaris" })
    )
    const res = await get("/auth-refresh-outbound?foo=bar", {
      headers: { ...H, Cookie: `Cms-Session-Hint=${hint}` },
    })
    assertEqual(res.status, 302, "Should 302")
    assertIncludes(res.headers.get("location"), "https://cin4.cps.gov.uk/polaris", "hint wins")
    assertIncludes(res.headers.get("location"), "foo=bar", "Should carry the query string")
  })

  await test("falls back to DEFAULT_UPSTREAM_CMS_DOMAIN_NAME with no hint cookie", async () => {
    const res = await get("/auth-refresh-outbound", { headers: H })
    assertEqual(res.status, 302, "Should 302")
    assertIncludes(
      res.headers.get("location"),
      "https://cms.cps.gov.uk/polaris",
      "Should fall back to the DEFAULT CMS domain"
    )
  })

  await test("sets X-InternetExplorerMode: 1", async () => {
    const res = await get("/auth-refresh-outbound", { headers: H })
    assertEqual(res.headers.get("x-internetexplorermode"), "1", "Should force IE mode")
  })
}

async function ddeiTests() {
  console.log("\nDDEI-backed handover endpoints (proxied):")

  const cases = [
    ["/auth-refresh-inbound", "/api/init/"],
    ["/auth-refresh-termination", "/api/auth-refresh-termination/"],
    ["/auth-refresh-cms-modern-token", "/api/cms-modern-token/"],
  ]
  for (const [path, upstreamPath] of cases) {
    await test(`${path} -> DDEI ${upstreamPath}`, async () => {
      const res = await get(path, { headers: H })
      assertEqual(res.status, 200, "Should proxy to the mock DDEI")
      const echo = await res.json()
      assertIncludes(echo.url, upstreamPath, `Should hit DDEI ${upstreamPath}`)
    })
  }

  await test("/dev-login/ -> DDEI /api/login with the function key", async () => {
    const res = await get("/dev-login/", { headers: H })
    assertEqual(res.status, 200, "Should proxy")
    const echo = await res.json()
    assertIncludes(echo.url, "/api/login", "Should hit DDEI /api/login")
    assertIncludes(echo.url, "code=test-ddei-key-12345", "Should append the function key")
  })

  await test("/dev-login/ GET clears the env + BIG-IP/LB cookies (authHandover.devLogin)", async () => {
    // Pins the effect of the shared method-branching js_header_filter.
    const cookies = (await get("/dev-login/", { headers: H })).headers.getSetCookie().join("\n")
    assertIncludes(cookies, "__CMSENV=deleted", "clears the env cookie")
    assertIncludes(cookies, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted", "clears a BIG-IP pool cookie")
    assertIncludes(cookies, "F-CIN5-LBsessioncookie=deleted", "clears an LB session cookie")
  })

  await test("/api/dev-login-full-cookie/ -> DDEI /api/login-full-cookie", async () => {
    const res = await get("/api/dev-login-full-cookie/", { headers: H })
    assertEqual(res.status, 200, "Should proxy")
    const echo = await res.json()
    assertIncludes(echo.url, "/api/login-full-cookie", "Should hit DDEI /api/login-full-cookie")
    assertIncludes(echo.url, "code=test-ddei-key-12345", "Should append the function key")
  })
}

async function main() {
  await initTests()
  await polarisTests()
  await outboundTests()
  await ddeiTests()
  process.exit(summarise("Auth handover (feature 2)"))
}

main()
