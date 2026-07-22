#!/usr/bin/env node
/**
 * Unit tests for features/app-launch.js.  CHARACTERISATION / GOLDEN MASTER.
 *
 * app-launch.js collapses the ten static /launch/* 302 blocks into one handler.
 * This file pins its output BYTE-FOR-BYTE against the original config: PROD_R and
 * TEST_R below are the exact `?r=` values copied verbatim from the pre-refactor
 * app-launch.conf, so if the handler's URL construction ever drifts from what the
 * static blocks emitted, these fail.
 *
 * Loaded from the REAL config/features/app-launch.js — see njs-harness.js.
 */
const { test, assertEqual, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("../../../tests/unit/njs-harness")

// The two OutSystems hop `?r=` values, verbatim from the old static blocks.
const PROD_R = "https%3A%2F%2Fcps.outsystemsenterprise.com%2FCasework_Patterns%2Fauth-handover.html%3Fsrc%3Dhttps%3A%2F%2Fpolaris.cps.gov.uk%2Fglobal-components%2Fprod%2Fauth-handover.js%26stage%3Dos-cookie-return%26r%3Dhttps%253A%252F%252Fcps.outsystemsenterprise.com%252Fcasework_blocks%252Fhome%253FIsFromCMS%253DTrue"
const TEST_R = "https%3A%2F%2Fcps-tst.outsystemsenterprise.com%2FCasework_Patterns%2Fauth-handover.html%3Fsrc%3Dhttps%3A%2F%2Fpolaris-qa-notprod.cps.gov.uk%2Fglobal-components%2Ftest%2Fauth-handover.js%26stage%3Dos-cookie-return%26r%3Dhttps%253A%252F%252Fcps-tst.outsystemsenterprise.com%252Fcasework%252Fhome%253FIsFromCMS%253DTrue"

const expect = (host, rParam) => `https://${host}/polaris?r=${rParam}`

// [path, expected Location] — the exact strings the ten static blocks returned.
const CASES = [
  ["/launch/cms", expect("cms.cps.gov.uk", PROD_R)], // host from DEFAULT_UPSTREAM_CMS_DOMAIN_NAME
  ["/launch/cin2", expect("cin2.cps.gov.uk", TEST_R)],
  ["/launch/cin3", expect("cin3.cps.gov.uk", TEST_R)],
  ["/launch/cin4", expect("cin4.cps.gov.uk", TEST_R)],
  ["/launch/cin5", expect("cin5.cps.gov.uk", TEST_R)],
  ["/launch/cms-proxy", expect("polaris.cps.gov.uk", PROD_R)],
  ["/launch/cin2-proxy", expect("polaris-qa-notprod.cps.gov.uk", TEST_R)],
  ["/launch/cin3-proxy", expect("polaris-qa-notprod.cps.gov.uk", TEST_R)],
  ["/launch/cin4-proxy", expect("polaris-qa-notprod.cps.gov.uk", TEST_R)],
  ["/launch/cin5-proxy", expect("polaris-qa-notprod.cps.gov.uk", TEST_R)],
]

async function launches(appLaunch) {
  console.log("\n/launch/* — 302 Location built byte-identically to the old static blocks:")

  for (const [uri, expected] of CASES) {
    await test(`${uri} -> exact 302 Location`, () => {
      const r = createMockRequest({ uri })
      appLaunch.launch(r)
      assertEqual(r.returnCode, 302, "302")
      assertEqual(r.returnBody, expected, "byte-exact Location")
    })
  }

  await test("unknown /launch/<key> -> 404", () => {
    const r = createMockRequest({ uri: "/launch/nope" })
    appLaunch.launch(r)
    assertEqual(r.returnCode, 404, "unknown target 404s")
  })
}

async function main() {
  // app-launch.js reads DEFAULT_UPSTREAM_CMS_DOMAIN_NAME at module load, so apply
  // the env BEFORE loadNjs (unlike the per-request readers, which don't care).
  const restoreEnv = applyEnv(cmsEnvObject())
  const appLaunch = await loadNjs("features/app-launch/app-launch.js")
  await launches(appLaunch)
  restoreEnv()
  process.exit(summarise("app-launch.js (unit)"))
}

main()
