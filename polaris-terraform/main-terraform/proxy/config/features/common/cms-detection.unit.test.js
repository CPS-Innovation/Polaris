#!/usr/bin/env node
/**
 * Unit tests for features/common/cms-detection.js.  CHARACTERISATION.
 *
 * cms-detection.js is the shared CMS-environment PRIMITIVE — a pure ESM
 * helper the feature modules import. It has just two exports; this file pins that
 * surface: the detection rule (detect) and the process.env reader (setting). The
 * getter factories that build on setting are feature-local, covered in each
 * feature's own suite. (Which header to detect from is also a feature concern —
 * e.g. auth-handover's dev-login detects from the outgoing Set-Cookie.)
 *
 * Loaded from the REAL config/features/common/cms-detection.js — see njs-harness.js.
 */
const {
  test,
  assertEqual,
  summarise,
} = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("../../../tests/unit/njs-harness")

const req = (cookie) =>
  createMockRequest({ headersIn: cookie ? { Cookie: cookie } : {} })

async function detection(common) {
  // detect(cookieString) is the shared detection rule — assert it directly.
  console.log("\ndetect — cookie text -> environment token:")

  const cases = [
    ["__CMSENV=cin2", "cin2", "cin2"],
    ["__CMSENV=cin4", "cin4", "cin4"],
    ["__CMSENV=cin5", "cin5", "cin5"],
    ["__CMSENV=cin3", "default", "cin3 IS the default environment"],
    ["__CMSENV=default", "default", "explicit default"],
    ["", "default", "empty -> default"],
    [undefined, "default", "missing -> default"],
    ["__CMSENV=nonsense", "default", "unknown -> default"],
  ]
  for (const [cookie, expected, label] of cases) {
    await test(`${cookie || "(none)"} -> ${expected}  (${label})`, () => {
      assertEqual(common.detect(cookie), expected, label)
    })
  }

  await test("matching is case-insensitive", () => {
    assertEqual(common.detect("__CMSENV=CIN2"), "cin2", "toLowerCase()")
  })

  // QUIRK: loose SUBSTRING match over the whole cookie text, not a parse of the
  // __CMSENV value — any cookie containing "cin2" anywhere selects cin2.
  await test("QUIRK: any cookie containing 'cin2' selects cin2, even unrelated ones", () => {
    assertEqual(common.detect("BIGipServer~x~cin2.cps.gov.uk_POOL=1"), "cin2", "substring match")
    assertEqual(common.detect("somethingcin2something=1"), "cin2", "not anchored to __CMSENV")
  })

  // QUIRK: checks run cin3, cin2, cin4, cin5 and return on the FIRST hit.
  await test("QUIRK: cin3 wins over cin2 when both appear (fixed check order)", () => {
    assertEqual(common.detect("__CMSENV=cin2; other=cin3"), "default", "cin3 checked first -> default")
  })

  await test("QUIRK: cin2 wins over cin4/cin5 when several appear", () => {
    assertEqual(common.detect("__CMSENV=cin5; stale=cin2"), "cin2", "cin2 checked before cin4/cin5")
  })
}

async function primitives(common) {
  // The getter factories (upstream/dest) that used to live here are now
  // feature-local; each feature's suite covers them via its getters.
  console.log("\nsetting — read a per-environment app setting:")

  await test("setting(r, name) reads <ENV>_<name> from process.env", () => {
    assertEqual(common.setting(req("__CMSENV=cin2"), "UPSTREAM_CMS_DOMAIN_NAME"), "cin2.cps.gov.uk", "CIN2_UPSTREAM_CMS_DOMAIN_NAME")
    assertEqual(common.setting(req("__CMSENV=cin4"), "UPSTREAM_CMS_MODERN_DOMAIN_NAME"), "cmsmodstage.cps.gov.uk", "CIN4_UPSTREAM_CMS_MODERN_DOMAIN_NAME")
    assertEqual(common.setting(req(""), "UPSTREAM_CMS_DOMAIN_NAME"), "cms.cps.gov.uk", "no cookie -> DEFAULT_ prefix")
  })
}

async function main() {
  const common = await loadNjs("features/common/cms-detection.js")
  const restoreEnv = applyEnv(cmsEnvObject())
  await detection(common)
  await primitives(common)
  restoreEnv()
  process.exit(summarise("cms-detection.js (unit)"))
}

main()
