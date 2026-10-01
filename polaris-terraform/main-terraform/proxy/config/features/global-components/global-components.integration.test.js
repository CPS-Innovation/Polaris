#!/usr/bin/env node
/**
 * Global-components — SMOKE.  Runs under BOTH configs (live monolith and the
 * sliced `--next`), so passing on both proves the move of global-components.conf
 * / .js into features/ preserved behaviour.
 *
 * SMOKE ONLY. global-components' full behaviour is owned by the sibling repo
 * (global-components/infra/proxy); here we just confirm every route family still
 * routes correctly through this proxy — the njs module loads, the CORS/OPTIONS
 * handling works, and the proxied routes reach their (mocked) upstreams.
 * One exception: /case-review-redirect's host handling (FCT2-22132) is pinned in
 * full, because it is an open-redirect guard and live + next each carry a copy.
 *
 * Routes (features/global-components.conf):
 *   = /global-components/cms-session-hint        gloco.handleSessionHint
 *   ~ ^/global-components/api/(.*)                -> MDS (WM_MDS_*)
 *   ~ ^/api/global-components/(.*)                -> rewrite to the above
 *   ~ ^/global-components/(dev|test|prod)/(.*)    -> blob storage
 *   ~ ^/global-components/state/(.*)              gloco.handleState
 *   ~ ^/global-components/analytics/(.*)          -> App Insights
 *   /global-components/navigate-cms               gloco.handleNavigateCms
 *   /case-review-redirect/                        gloco.handleCaseReviewRedirect
 *   @gloco_preflight                              OPTIONS -> 204
 */
const {
  test,
  assert,
  assertEqual,
  assertIncludes,
  get,
  summarise,
} = require("../../../tests/integration/test-utils")

const notServerError = (res, label) =>
  assert(res.status < 500, `${label} should not 5xx, got ${res.status}`)

async function njsHandlers() {
  console.log("\nnjs-content handlers (gloco.*) — module loaded & responding:")

  await test("cms-session-hint responds (handleSessionHint)", async () => {
    const res = await get("/global-components/cms-session-hint")
    notServerError(res, "cms-session-hint")
  })

  await test("state responds (handleState)", async () => {
    const res = await get("/global-components/state/preview")
    notServerError(res, "state")
  })

  await test("navigate-cms responds (handleNavigateCms)", async () => {
    const res = await get("/global-components/navigate-cms")
    notServerError(res, "navigate-cms")
  })

  await test("case-review-redirect responds (handleCaseReviewRedirect)", async () => {
    const res = await get("/case-review-redirect/cps-tst/test?URN=1&CMSCaseId=2")
    notServerError(res, "case-review-redirect")
  })
}

// FCT2-22132: /case-review-redirect/{osHost}/{envFolder} — osHost is an
// outsystemsenterprise.com subdomain (original form) or a full host in our estate.
// Runs against BOTH configs, so it also pins live and next to the same behaviour.
async function caseReviewRedirect() {
  console.log("\n/case-review-redirect — OS host forms (FCT2-22132):")

  // The OS landing URL is double-encoded inside the 302 (auth-refresh-outbound?r=<handover>?r=<landing>).
  const landingOf = (res) => decodeURIComponent(decodeURIComponent(res.headers.get("location") || ""))

  await test("subdomain form -> <sub>.outsystemsenterprise.com, via auth-refresh-outbound", async () => {
    const res = await get("/case-review-redirect/cps-tst/test?URN=1&CMSCaseId=2")
    assertEqual(res.status, 302, "302")
    assertIncludes(res.headers.get("location"), "/auth-refresh-outbound?r=", "routed through auth-refresh-outbound")
    assertIncludes(landingOf(res), "https://cps-tst.outsystemsenterprise.com/CaseReview/LandingPage?CMSCaseId=2&URN=1", "OS landing")
  })

  await test("full cps.gov.uk host is used as-is (lower-cased)", async () => {
    const res = await get("/case-review-redirect/OApps-QA-NotProd.int.cps.gov.uk/test?URN=1&CMSCaseId=2")
    assertEqual(res.status, 302, "302")
    assertIncludes(landingOf(res), "https://oapps-qa-notprod.int.cps.gov.uk/CaseReview/LandingPage", "full host, lower-cased")
  })

  await test("a full host outside our estate is refused (no open redirect)", async () => {
    const res = await get("/case-review-redirect/evil.com/test?URN=1&CMSCaseId=2")
    assertEqual(res.status, 400, "400")
    assertIncludes(await res.text(), "unexpected OS host", "explained")
  })
}

async function mdsProxy() {
  console.log("\nMDS API proxy (WM_MDS_*):")

  await test("/global-components/api/* proxies to the MDS mock", async () => {
    const res = await get("/global-components/api/cases")
    assertEqual(res.status, 200, "Should proxy")
    const echo = await res.json()
    assertIncludes(echo.url, "/api/cases", "Should hit the MDS base URL + path")
  })

  await test("forwards the x-functions-key and strips Authorization", async () => {
    const res = await get("/global-components/api/cases", {
      headers: { Authorization: "Bearer should-be-stripped" },
    })
    const echo = await res.json()
    assertEqual(echo.headers["x-functions-key"], "test-mds-key-12345", "function key forwarded")
    assert(
      !echo.headers.authorization || echo.headers.authorization === "",
      `Authorization should be stripped, got ${JSON.stringify(echo.headers.authorization)}`
    )
  })

  await test("legacy /api/global-components/* rewrites to the MDS route", async () => {
    const res = await get("/api/global-components/cases")
    assertEqual(res.status, 200, "Should proxy after rewrite")
    const echo = await res.json()
    assertIncludes(echo.url, "/api/cases", "Rewritten to /global-components/api/* -> MDS /api/cases")
  })
}

async function blobAndAnalytics() {
  console.log("\nStatic blob + analytics proxies:")

  await test("/global-components/{env}/* proxies to blob storage (https)", async () => {
    const res = await get("/global-components/test/global-components.js")
    assertEqual(res.status, 200, "Should proxy to the blob mock")
    const echo = await res.json()
    assertIncludes(echo.url, "/test/global-components.js", "Path preserved to blob")
  })

  // gzip on the blob route (ported from global-components #1056). The echo body
  // must clear gzip_min_length (1024) and blob forwards no request headers, so a
  // long path is the only way to pad it.
  const BIG_BLOB = `/global-components/test/${"x".repeat(1100)}.js`

  await test("/global-components/{env}/* gzips when the client accepts it", async () => {
    const res = await get(BIG_BLOB, { headers: { "Accept-Encoding": "gzip" } })
    assertEqual(res.status, 200, "Should proxy to the blob mock")
    assertEqual(res.headers.get("content-encoding"), "gzip", "Compressed")
    assertIncludes(res.headers.get("vary") || "", "Accept-Encoding", "gzip_vary on")
  })

  await test("/global-components/{env}/* stays identity when the client does not accept gzip", async () => {
    const res = await get(BIG_BLOB, { headers: { "Accept-Encoding": "identity" } })
    assertEqual(res.status, 200, "Should proxy to the blob mock")
    assertEqual(res.headers.get("content-encoding"), null, "Not compressed")
  })

  await test("/global-components/analytics/* proxies to App Insights", async () => {
    const res = await get("/global-components/analytics/v2/track")
    assertEqual(res.status, 200, "Should proxy to the (mocked) App Insights host")
    const echo = await res.json()
    assertIncludes(echo.headers.host, "applicationinsights", "Host is the App Insights hostname")
  })
}

async function cors() {
  console.log("\nCORS / OPTIONS preflight (@gloco_preflight):")

  await test("OPTIONS on a gc route -> 204 with CORS methods", async () => {
    const res = await get("/global-components/api/cases", { method: "OPTIONS" })
    assertEqual(res.status, 204, "Preflight should be 204")
    assertIncludes(
      res.headers.get("access-control-allow-methods") || "",
      "OPTIONS",
      "Should advertise the allowed methods"
    )
  })

  await test("GET on a gc route carries the CORS credentials header", async () => {
    const res = await get("/global-components/api/cases")
    assertEqual(
      res.headers.get("access-control-allow-credentials"),
      "true",
      "Should set Allow-Credentials"
    )
  })
}

async function main() {
  await njsHandlers()
  await caseReviewRedirect()
  await mdsProxy()
  await blobAndAnalytics()
  await cors()
  process.exit(summarise("Global-components (smoke)"))
}

main()
