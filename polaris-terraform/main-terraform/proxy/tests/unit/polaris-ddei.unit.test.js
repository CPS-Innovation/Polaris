#!/usr/bin/env node
/**
 * Unit tests for features/polaris-ddei.js.  CHARACTERISATION.
 *
 * This feature is self-contained (slated for deletion), so polaris-ddei.js owns
 * EVERY getter its conf js_sets — both the ones unique to it (Farnborough dests +
 * all four DC IPs) and its own copies of the ones cms-proxy.js also has (Corsham
 * dests + domain getters). All are built from its own factories over
 * common.setting (`import common from "./common/cms-detection.js"`), so these
 * tests also exercise that cross-directory wiring.
 *
 * Loaded from the REAL config/features/polaris-ddei.js — see njs-harness.js.
 */
const {
  test,
  assertEqual,
  summarise,
} = require("./test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("./njs-harness")

const req = (cookie) =>
  createMockRequest({
    headersIn: cookie ? { Cookie: cookie } : {},
    variables: { host: "proxy.example.org" },
  })

async function getters(polarisDdei) {
  console.log("\nAll getters this feature owns, every environment:")

  const table = [
    ["cin2", { domain: "cin2.cps.gov.uk", modern: "cmsmodcin2.cps.gov.uk", services: "not-used-in-cin2.cps.gov.uk", ip: "10.0.2.1", modernIp: "10.0.2.2" }],
    ["cin4", { domain: "cin4.cps.gov.uk", modern: "cmsmodstage.cps.gov.uk", services: "not-used-in-cin4.cps.gov.uk", ip: "10.0.4.1", modernIp: "10.0.4.2" }],
    ["cin5", { domain: "cin5.cps.gov.uk", modern: "cmsmodcin5.cps.gov.uk", services: "not-used-in-cin5.cps.gov.uk", ip: "10.0.5.1", modernIp: "10.0.5.2" }],
    ["cin3", { domain: "cms.cps.gov.uk", modern: "cmsmodern.cps.gov.uk", services: "cms-services.cps.gov.uk", ip: "10.0.0.1", modernIp: "10.0.0.2" }],
  ]

  for (const [env, e] of table) {
    const r = () => req(`__CMSENV=${env}`)

    // Own copies of the getters also in cms-proxy.js (same values).
    await test(`${env}: domain / modern / services`, () => {
      assertEqual(polarisDdei.upstreamCmsDomainName(r()), e.domain, "classic domain")
      assertEqual(polarisDdei.upstreamCmsModernDomainName(r()), e.modern, "modern domain")
      assertEqual(polarisDdei.upstreamCmsServicesDomainName(r()), e.services, "services domain")
    })

    await test(`${env}: proxyDestinationCorsham / ModernCorsham build <protocol>://<ip>`, () => {
      assertEqual(polarisDdei.proxyDestinationCorsham(r()), `http://${e.ip}`, "classic Corsham")
      assertEqual(polarisDdei.proxyDestinationModernCorsham(r()), `http://${e.modernIp}`, "modern Corsham")
    })

    // Getters unique to this feature.
    await test(`${env}: Corsham / Farnborough IPs`, () => {
      assertEqual(polarisDdei.upstreamCmsIpCorsham(r()), e.ip, "classic Corsham IP")
      assertEqual(polarisDdei.upstreamCmsModernIpCorsham(r()), e.modernIp, "modern Corsham IP")
      assertEqual(polarisDdei.upstreamCmsIpFarnborough(r()), `${e.ip}-fb`, "classic Farnborough IP")
      assertEqual(polarisDdei.upstreamCmsModernIpFarnborough(r()), `${e.modernIp}-fb`, "modern Farnborough IP")
    })

    await test(`${env}: proxyDestinationFarnborough* build <protocol>://<ip>`, () => {
      assertEqual(polarisDdei.proxyDestinationFarnborough(r()), `http://${e.ip}-fb`, "classic Farnborough")
      assertEqual(polarisDdei.proxyDestinationModernFarnborough(r()), `http://${e.modernIp}-fb`, "modern Farnborough")
    })
  }

  await test("proxyDestinationFarnborough uses ENDPOINT_HTTP_PROTOCOL from process.env", () => {
    const restore = applyEnv({ ENDPOINT_HTTP_PROTOCOL: "https" })
    try {
      assertEqual(polarisDdei.proxyDestinationFarnborough(req("__CMSENV=cin2")), "https://10.0.2.1-fb", "protocol from process.env")
    } finally {
      restore()
    }
  })
}

async function main() {
  const polarisDdei = await loadNjs("features/polaris-ddei.js")
  const restoreEnv = applyEnv(cmsEnvObject())
  await getters(polarisDdei)
  restoreEnv()
  process.exit(summarise("polaris-ddei.js (unit)"))
}

main()
