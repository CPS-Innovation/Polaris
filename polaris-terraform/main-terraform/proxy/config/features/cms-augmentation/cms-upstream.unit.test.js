#!/usr/bin/env node
/**
 * Unit tests for features/cms-augmentation/cms-upstream.js — the per-CMS-env upstream getters
 * the uaglCMS route js_sets (as $aug*). Mirrors the cms-proxy / polaris-ddei getter tests;
 * also exercises the ../common/cms-detection wiring. The browser client (cms-augmentation.js)
 * and the relay (presence-relay.html) are static assets, not njs — nothing to unit-test there.
 */
const { test, assertEqual, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("../../../tests/unit/njs-harness")

const req = (cookie) =>
  createMockRequest({
    headersIn: cookie ? { Cookie: cookie } : {},
    variables: { host: "proxy.example.org" },
  })

async function getters(cmsAug) {
  console.log("\ncms-upstream getters, per environment (the four the uaglCMS route uses):")

  const table = [
    ["cin2", { domain: "cin2.cps.gov.uk", modern: "cmsmodcin2.cps.gov.uk", services: "not-used-in-cin2.cps.gov.uk", ip: "10.0.2.1" }],
    ["cin4", { domain: "cin4.cps.gov.uk", modern: "cmsmodstage.cps.gov.uk", services: "not-used-in-cin4.cps.gov.uk", ip: "10.0.4.1" }],
    ["cin5", { domain: "cin5.cps.gov.uk", modern: "cmsmodcin5.cps.gov.uk", services: "not-used-in-cin5.cps.gov.uk", ip: "10.0.5.1" }],
    ["cin3", { domain: "cms.cps.gov.uk", modern: "cmsmodern.cps.gov.uk", services: "cms-services.cps.gov.uk", ip: "10.0.0.1" }],
  ]

  for (const [env, e] of table) {
    const r = () => req(`__CMSENV=${env}`)
    await test(`${env}: domain / modern / services`, () => {
      assertEqual(cmsAug.upstreamCmsDomainName(r()), e.domain, "classic domain")
      assertEqual(cmsAug.upstreamCmsModernDomainName(r()), e.modern, "modern domain")
      assertEqual(cmsAug.upstreamCmsServicesDomainName(r()), e.services, "services domain")
    })
    await test(`${env}: proxyDestinationCorsham builds <protocol>://<ip>`, () => {
      assertEqual(cmsAug.proxyDestinationCorsham(r()), `http://${e.ip}`, "Corsham dest")
    })
  }
}

async function main() {
  const restore = applyEnv(cmsEnvObject())
  const cmsAug = await loadNjs("features/cms-augmentation/cms-upstream.js")
  await getters(cmsAug)
  restore()
  process.exit(summarise("cms-augmentation/cms-upstream.js (unit)"))
}

main()
