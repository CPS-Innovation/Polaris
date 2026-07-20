#!/usr/bin/env node
/**
 * Feature 6 🔌 — polaris-ddei internal routes.  CHARACTERISATION.
 *
 * Locations covered (docs/PROXY.md §4):
 *   nginx.conf:803  /internal-implementation/corsham/
 *   nginx.conf:838  /internal-implementation/corsham/modern/
 *   nginx.conf:872  /internal-implementation/farnborough/
 *   nginx.conf:907  /internal-implementation/farnborough/modern/
 *
 * These are "internal-only, used by DDEI" — but note that is enforced by NETWORK
 * ISOLATION in Azure, not by nginx (there is no allow/deny in the config), which
 * is why they are reachable and testable here.
 *
 * §4 notes all four "can be deleted once auth handover code is refactored", and
 * docs/PLAN.md Phase 3 reimplements polaris-ddei in njs — so this file is the
 * evidence that deleting them changes nothing else.
 *
 * Each block does `rewrite ^/internal-implementation/<dc>/(.*) /$1 break;` then
 * proxy_passes to the DC-specific upstream, so the prefix is stripped upstream.
 */
const { test, assertEqual, assertIncludes, get, summarise } = require("../test-utils")

const envCookie = (v) => ({ Cookie: `__CMSENV=${v}` })

async function main() {
  console.log("\n/internal-implementation/* — DC-pinned CMS routes:")

  // [path, upstream path, expected Host fragment]
  const cases = [
    ["/internal-implementation/corsham/Home", "/Home", "cms.cps.gov.uk"],
    ["/internal-implementation/corsham/modern/Home", "/Home", "cmsmodern.cps.gov.uk"],
    ["/internal-implementation/farnborough/Home", "/Home", "cms.cps.gov.uk"],
    ["/internal-implementation/farnborough/modern/Home", "/Home", "cmsmodern.cps.gov.uk"],
  ]

  for (const [path, upstreamPath, host] of cases) {
    await test(`${path} -> upstream ${upstreamPath} (Host ${host})`, async () => {
      const res = await get(path)
      assertEqual(res.status, 200, "Should proxy")
      const echo = await res.json()
      assertEqual(echo.url, upstreamPath, "The /internal-implementation/<dc>/ prefix is rewritten away")
      assertIncludes(echo.headers.host, host, "Should target the right CMS flavour")
    })
  }

  await test("longest-prefix: /corsham/modern/ is not served by /corsham/", async () => {
    const res = await get("/internal-implementation/corsham/modern/Ping")
    const echo = await res.json()
    assertIncludes(echo.headers.host, "cmsmodern", "Should hit the modern block, not classic")
  })

  console.log("\nEnvironment selection still applies on the internal routes:")

  await test("__CMSENV=cin2 pins the internal route to the cin2 CMS", async () => {
    const res = await get("/internal-implementation/corsham/Home", { headers: envCookie("cin2") })
    const echo = await res.json()
    assertIncludes(echo.headers.host, "cin2.cps.gov.uk", "cmsenv selection applies here too")
  })

  await test("__CMSENV=cin4 pins the modern internal route to the cin4 modern CMS", async () => {
    const res = await get("/internal-implementation/farnborough/modern/Home", {
      headers: envCookie("cin4"),
    })
    const echo = await res.json()
    assertIncludes(echo.headers.host, "cmsmodstage.cps.gov.uk", "cin4's modern domain")
  })

  await test("query strings are carried through", async () => {
    const res = await get("/internal-implementation/corsham/Home?a=1&b=2")
    const echo = await res.json()
    assertIncludes(echo.url, "a=1", "Should carry $is_args$args")
  })

  process.exit(summarise("polaris-ddei internal routes (feature 6)"))
}

main()
