#!/usr/bin/env node
/**
 * Feature 4 📁 — CWA / materials (+ the gateway API).  CHARACTERISATION.
 *
 * Locations covered (docs/PROXY.md §4):
 *   nginx.conf:679  /${APP_SUBFOLDER_PATH}                       -> Polaris SPA
 *   nginx.conf:687  /materials                                    -> handover 302
 *   nginx.conf:697  = /materials-ui                               -> 302 /materials-ui/
 *   nginx.conf:701  ~ ^/materials-ui/[^/]+/[^/]+/materials$       -> Materials deep-link
 *   nginx.conf:709  /materials-ui/                                -> Materials UI
 *   nginx.conf:718  /api/                                         -> gateway API
 *   nginx.conf:940  /v2/                                          -> App Insights ingestion
 */
const {
  test,
  assert,
  assertEqual,
  assertIncludes,
  get,
  summarise,
} = require("../test-utils")

async function spa() {
  console.log("\nlocation /polaris-ui (APP_SUBFOLDER_PATH) — Polaris SPA:")

  await test("proxies to the SPA upstream, preserving the full URI", async () => {
    const res = await get("/polaris-ui/case/1")
    assertEqual(res.status, 200, "Should proxy")
    const echo = await res.json()
    assertEqual(echo.url, "/polaris-ui/case/1", "Full URI passed to the SPA (proxy_pass has no URI part)")
  })
}

async function materials() {
  console.log("\nlocation /materials — handover redirect:")

  await test("generic (no caseUrn) -> 302 handover to /materials-ui", async () => {
    const res = await get("/materials")
    assertEqual(res.status, 302, "Should 302")
    const loc = res.headers.get("location")
    assertIncludes(loc, "/polaris?r=", "Should hop via /polaris")
    assertIncludes(loc, "auth-refresh-inbound", "Should route through auth-refresh-inbound")
    assertIncludes(loc, "polaris-ui-url=/materials-ui", "Should land on the generic Materials page")
  })

  await test("case-specific -> 302 handover to the case deep-link", async () => {
    const res = await get("/materials?caseUrn=URN1&caseId=99")
    assertEqual(res.status, 302, "Should 302")
    assertIncludes(
      res.headers.get("location"),
      "polaris-ui-url=/materials-ui/URN1/99/materials",
      "Should build the {caseUrn}/{caseId}/materials deep-link (feeds nginx.conf:701)"
    )
  })

  console.log("\nlocation = /materials-ui — trailing-slash normaliser:")

  await test("302s to /materials-ui/ (as an absolute URL)", async () => {
    const res = await get("/materials-ui")
    assertEqual(res.status, 302, "Should 302")
    // The config says `return 302 /materials-ui/;` but nginx's absolute_redirect
    // (on by default) rewrites the relative target to an absolute URL built from
    // $host + the listening port. In this harness that yields
    // http://localhost/materials-ui/ — the :8080 is absent because the container
    // listens on 80 and we publish 8080. So assert the path, not the whole URL.
    assertIncludes(res.headers.get("location"), "/materials-ui/", "Should add the trailing slash")
  })

  console.log("\nMaterials UI proxying:")

  await test("/materials-ui/ prefix proxies to the Materials app", async () => {
    const res = await get("/materials-ui/static/app.js")
    assertEqual(res.status, 200, "Should proxy")
    const echo = await res.json()
    assertEqual(echo.url, "/materials-ui/static/app.js", "Path preserved")
  })

  await test("deep-link regex (nginx.conf:701) wins over the /materials-ui/ prefix", async () => {
    // A regex location beats a plain prefix regardless of file order. Both end up
    // at the same upstream path, which is why §4 flags 701 as a drop candidate —
    // this test pins today's behaviour so that deletion can be proven safe.
    const res = await get("/materials-ui/URN1/99/materials")
    assertEqual(res.status, 200, "Should proxy")
    const echo = await res.json()
    assertEqual(echo.url, "/materials-ui/URN1/99/materials", "Same upstream path as the prefix block")
  })

  await test("deep-link regex only matches exactly two segments + /materials", async () => {
    // Three segments -> falls through to the /materials-ui/ prefix block instead.
    const res = await get("/materials-ui/a/b/c/materials")
    assertEqual(res.status, 200, "Should still proxy (via the prefix block)")
    const echo = await res.json()
    assertEqual(echo.url, "/materials-ui/a/b/c/materials", "Path preserved")
  })
}

async function api() {
  console.log("\nlocation /api/ — gateway API:")

  await test("proxies to the gateway upstream", async () => {
    const res = await get("/api/cases/1")
    assertEqual(res.status, 200, "Should proxy")
    const echo = await res.json()
    assertEqual(echo.url, "/api/cases/1", "Path preserved")
  })

  await test("sets the gateway Host header (with the :443 suffix as configured)", async () => {
    const res = await get("/api/cases/1")
    const echo = await res.json()
    // proxy_set_header Host ${API_ENDPOINT_DOMAIN_NAME}:443 — recorded verbatim.
    assertIncludes(echo.headers.host, ":443", "Host carries the configured :443 suffix")
  })

  await test("forwards X-Forwarded-For", async () => {
    const res = await get("/api/cases/1")
    const echo = await res.json()
    assert(echo.headers["x-forwarded-for"], "Should set X-Forwarded-For from $remote_addr")
  })

  await test("longest-prefix: /api/dev-login-full-cookie/ is NOT served by /api/", async () => {
    const res = await get("/api/dev-login-full-cookie/")
    const echo = await res.json()
    assertIncludes(echo.url, "/api/login-full-cookie", "Should hit the DDEI dev-login block instead")
  })
}

async function telemetry() {
  console.log("\nlocation /v2/ — App Insights ingestion:")

  await test("proxies to the App Insights host", async () => {
    // nginx.conf hardcodes https://uksouth-1.in.applicationinsights.azure.com/v2/.
    // The mock owns that hostname via a docker network alias (see docker-compose).
    const res = await get("/v2/track")
    assertEqual(res.status, 200, "Should proxy to the (mocked) App Insights host")
    const echo = await res.json()
    assertIncludes(echo.url, "/v2/", "Should hit the /v2/ ingestion path")
    assertIncludes(echo.headers.host, "applicationinsights", "Host should be the App Insights hostname")
  })
}

async function main() {
  await spa()
  await materials()
  await api()
  await telemetry()
  process.exit(summarise("CWA / materials (feature 4)"))
}

main()
