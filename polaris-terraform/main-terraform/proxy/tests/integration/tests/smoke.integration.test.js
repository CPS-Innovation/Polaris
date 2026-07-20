#!/usr/bin/env node
/**
 * SMOKE — proves the whole loop before any per-feature assertions exist:
 *   real nginx.conf + real njs  ->  rendered by envsubst from cmsproxy.mock.env
 *   ->  nginx boots  ->  njs modules load  ->  the mock upstream is reachable.
 *
 * If this is red, nothing else is worth reading.
 */
const { test, assert, assertEqual, assertIncludes, get, summarise } = require("../test-utils")

async function main() {
  console.log("\nSmoke — config renders and the proxy boots:")

  // Feature 0 💓 — health. Also proves the server block itself is live.
  await test("GET / returns the health string", async () => {
    const res = await get("/")
    assertEqual(res.status, 200, "Should return 200")
    const body = await res.text()
    assertIncludes(body, "Polaris Proxy is online", "Should return the health string")
  })

  // Feature 0 💓 — the Azure platform probe.
  await test("GET /robots933456.txt returns the probe body", async () => {
    const res = await get("/robots933456.txt")
    assertEqual(res.status, 200, "Should return 200")
    assertIncludes(await res.text(), "here", "Should return 'here'")
  })

  // If envsubst missed a variable the body/headers would still contain "${...}".
  await test("no unsubstituted ${VAR} placeholders leak into a response", async () => {
    const res = await get("/")
    const body = await res.text()
    assert(!body.includes("${"), `Body should have no un-rendered placeholders, got: ${body}`)
  })

  // Proves js_import of nginx.js worked: /init is js_content nginx.appAuthRedirect.
  // Non-whitelisted target -> 403 straight from njs.
  await test("njs is loaded (nginx.js): /init rejects a non-whitelisted target", async () => {
    const res = await get("/init?r=http://evil.example.com/x&cookie=a%3Db")
    assertEqual(res.status, 403, "Should return 403 from nginx.js appAuthRedirect")
  })

  // Proves js_import of cmsenv.js worked: this location does js_set cmsenv.getDomainFromCookie.
  await test("njs is loaded (cmsenv.js): /api/navigate-cms responds", async () => {
    const res = await get("/api/navigate-cms?caseId=1", {
      headers: { Cookie: "foo.cps.gov.uk_POOL=bar" },
    })
    assert([200, 302].includes(res.status), `Should respond 200/302, got ${res.status}`)
  })

  // Proves `include global-components*.conf;` picked up the rendered template.
  await test("global-components include is loaded", async () => {
    const res = await get("/global-components/cms-session-hint")
    assert(res.status < 500, `Should not 5xx (include loaded), got ${res.status}`)
  })

  // Proves the mock upstream is wired and reachable through the proxy.
  await test("proxy reaches the mock upstream (gateway API echo)", async () => {
    const res = await get("/api/some-endpoint")
    assertEqual(res.status, 200, "Should proxy to the mock")
    assertEqual(res.headers.get("x-mock-echo"), "1", "Should have hit the mock echo handler")
  })

  process.exit(summarise("Smoke"))
}

main()
