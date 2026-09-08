#!/usr/bin/env node
/**
 * Feature 1 🚀 — app launch from CMS.  CHARACTERISATION.
 *
 * Locations covered (docs/PROXY.md §4) — 10 static 302s:
 *   nginx.conf:616  /launch/cms          nginx.conf:631  /launch/cms-proxy
 *   nginx.conf:619  /launch/cin2         nginx.conf:634  /launch/cin2-proxy
 *   nginx.conf:622  /launch/cin3         nginx.conf:637  /launch/cin3-proxy
 *   nginx.conf:625  /launch/cin4         nginx.conf:640  /launch/cin4-proxy
 *   nginx.conf:628  /launch/cin5         nginx.conf:643  /launch/cin5-proxy
 *
 * These are pure `return 302` — no upstream is dialled, so no network is needed.
 * §4 notes these "can be collapsed into one (maybe needs njs)"; this file is the
 * safety net for exactly that change: it pins each Location verbatim.
 *
 * Only /launch/cms is env-driven (${DEFAULT_UPSTREAM_CMS_DOMAIN_NAME}); the other
 * nine hardcode their domains.
 */
const { test, assertEqual, assertIncludes, get, summarise } = require("../../../tests/integration/test-utils")

// The OutSystems auth-handover hop every launch redirect routes through.
const OS_PROD = "cps.outsystemsenterprise.com%2FCasework_Patterns%2Fauth-handover.html"
const OS_TEST = "cps-tst.outsystemsenterprise.com%2FCasework_Patterns%2Fauth-handover.html"
const OS_TEST1 = "cps-tst1.outsystemsenterprise.com%2FCasework_Patterns%2Fauth-handover.html" // cpt/cmo (FCT2-18732)

async function launches() {
  // [path, expected redirect host prefix, expected OutSystems hop]
  const cases = [
    // Direct-to-CMS launches
    ["/launch/cms", "https://cms.cps.gov.uk/polaris?r=", OS_PROD], // <- from env
    ["/launch/cin2", "https://cin2.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cin3", "https://cin3.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cin4", "https://cin4.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cin5", "https://cin5.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cpt", "https://cmscpt.cps.gov.uk/polaris?r=", OS_TEST1], // FCT2-18732
    ["/launch/cmo", "https://cmo.cps.gov.uk/polaris?r=", OS_TEST1], // FCT2-18732
    // Proxied-session launches — hop via the polaris proxy's own /polaris
    ["/launch/cms-proxy", "https://polaris.cps.gov.uk/polaris?r=", OS_PROD],
    ["/launch/cin2-proxy", "https://polaris-qa-notprod.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cin3-proxy", "https://polaris-qa-notprod.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cin4-proxy", "https://polaris-qa-notprod.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cin5-proxy", "https://polaris-qa-notprod.cps.gov.uk/polaris?r=", OS_TEST],
    ["/launch/cpt-proxy", "https://polaris-uat-notprod.cps.gov.uk/polaris?r=", OS_TEST1], // FCT2-18732
    ["/launch/cmo-proxy", "https://polaris-uat-notprod.cps.gov.uk/polaris?r=", OS_TEST1], // FCT2-18732
  ]

  console.log("\n/launch/* — static handover redirects:")
  for (const [path, expectedPrefix, osHop] of cases) {
    await test(`${path} -> 302 ${expectedPrefix.replace("/polaris?r=", "")}`, async () => {
      const res = await get(path)
      assertEqual(res.status, 302, "Should 302")
      const loc = res.headers.get("location")
      assertIncludes(loc, expectedPrefix, "Redirect target")
      assertIncludes(loc, osHop, "Should route via the OutSystems auth-handover hop")
      assertIncludes(loc, "stage%3Dos-cookie-return", "Should carry the os-cookie-return stage")
    })
  }

  await test("/launch/cms takes its domain from DEFAULT_UPSTREAM_CMS_DOMAIN_NAME", async () => {
    // The only launch route wired to an app setting — proves the env plumbing.
    const res = await get("/launch/cms")
    assertIncludes(res.headers.get("location"), "https://cms.cps.gov.uk/polaris", "env-driven domain")
  })

  await test("-proxy variants target the proxy host, not the CMS host", async () => {
    const direct = await get("/launch/cin2")
    const proxied = await get("/launch/cin2-proxy")
    assertIncludes(direct.headers.get("location"), "https://cin2.cps.gov.uk/", "direct -> CMS")
    assertIncludes(proxied.headers.get("location"), "https://polaris-qa-notprod.cps.gov.uk/", "proxy -> polaris")
  })

  await test("longest-prefix wins: /launch/cin2-proxy does not match /launch/cin2", async () => {
    const res = await get("/launch/cin2-proxy")
    assertIncludes(
      res.headers.get("location"),
      "https://polaris-qa-notprod.cps.gov.uk/",
      "Should hit the -proxy block, not /launch/cin2"
    )
  })
}

async function main() {
  await launches()
  process.exit(summarise("App launch from CMS (feature 1)"))
}

main()
