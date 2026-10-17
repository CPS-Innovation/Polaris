#!/usr/bin/env node
/**
 * Unit tests for features/common/cmsenv.js — the CMS-upstream getters nginx.conf
 * imports as `cmsenv`. They are a PUBLISHED CONTRACT: global-components' configs
 * js_set the same variables to these same `cmsenv.<name>` functions, so the export
 * names are pinned here as well as the values (see cmsenv.js / docs/PROXY.md).
 *
 * Loaded from the REAL config/features/common/cmsenv.js — see njs-harness.js.
 */
const { test, assertEqual, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("../../../tests/unit/njs-harness")

async function contract(cmsenv) {
  console.log("\ncmsenv — exported names are the contract:")

  await test("exports exactly the five contract getters", () => {
    assertEqual(
      Object.keys(cmsenv).sort().join(","),
      [
        "proxyDestinationCorsham",
        "proxyDestinationModernCorsham",
        "upstreamCmsDomainName",
        "upstreamCmsModernDomainName",
        "upstreamCmsServicesDomainName",
      ].join(","),
      "contract names (renaming any breaks global-components' configs)"
    )
  })
}

async function getters(cmsenv) {
  console.log("\nCMS-upstream getters (the cmsenv contract — Corsham dests + domain getters):")

  const req = (cookie) => createMockRequest({ headersIn: cookie ? { Cookie: cookie } : {} })

  const table = [
    ["cin2", { domain: "cin2.cps.gov.uk", modern: "cmsmodcin2.cps.gov.uk", services: "not-used-in-cin2.cps.gov.uk", ip: "10.0.2.1", modernIp: "10.0.2.2" }],
    ["cin4", { domain: "cin4.cps.gov.uk", modern: "cmsmodstage.cps.gov.uk", services: "not-used-in-cin4.cps.gov.uk", ip: "10.0.4.1", modernIp: "10.0.4.2" }],
    ["cin5", { domain: "cin5.cps.gov.uk", modern: "cmsmodcin5.cps.gov.uk", services: "not-used-in-cin5.cps.gov.uk", ip: "10.0.5.1", modernIp: "10.0.5.2" }],
    ["cin3", { domain: "cms.cps.gov.uk", modern: "cmsmodern.cps.gov.uk", services: "cms-services.cps.gov.uk", ip: "10.0.0.1", modernIp: "10.0.0.2" }],
  ]

  for (const [env, e] of table) {
    const r = () => req(`__CMSENV=${env}`)
    await test(`${env}: domain / modern / services`, () => {
      assertEqual(cmsenv.upstreamCmsDomainName(r()), e.domain, "classic domain")
      assertEqual(cmsenv.upstreamCmsModernDomainName(r()), e.modern, "modern domain")
      assertEqual(cmsenv.upstreamCmsServicesDomainName(r()), e.services, "services domain")
    })

    await test(`${env}: proxyDestinationCorsham / ModernCorsham build <protocol>://<ip>`, () => {
      assertEqual(cmsenv.proxyDestinationCorsham(r()), `http://${e.ip}`, "classic Corsham")
      assertEqual(cmsenv.proxyDestinationModernCorsham(r()), `http://${e.modernIp}`, "modern Corsham")
    })
  }

  await test("proxyDestination* uses ENDPOINT_HTTP_PROTOCOL from process.env", () => {
    const restore = applyEnv({ ENDPOINT_HTTP_PROTOCOL: "https" })
    try {
      assertEqual(cmsenv.proxyDestinationCorsham(req("__CMSENV=cin2")), "https://10.0.2.1", "protocol from process.env")
    } finally {
      restore()
    }
  })
}

async function main() {
  const cmsenv = await loadNjs("features/common/cmsenv.js")
  const restoreEnv = applyEnv(cmsEnvObject())
  await contract(cmsenv)
  await getters(cmsenv)
  restoreEnv()
  process.exit(summarise("cmsenv.js (unit)"))
}

main()
