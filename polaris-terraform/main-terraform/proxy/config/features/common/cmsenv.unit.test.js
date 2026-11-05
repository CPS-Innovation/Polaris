#!/usr/bin/env node
/**
 * Unit tests for features/common/cmsenv.js.  CHARACTERISATION.
 *
 * Two things live there: the CMS-upstream getters nginx.conf imports as `cmsenv` — a
 * PUBLISHED CONTRACT: global-components' configs js_set the same variables to these same
 * `cmsenv.<name>` functions, so the export names are pinned here as well as the values
 * (see cmsenv.js / docs/PROXY.md) — and the two primitives our feature modules import:
 * the detection rule (detect) and the process.env reader (setting).
 *
 * Loaded from the REAL config/features/common/cmsenv.js — see njs-harness.js.
 */
const { test, assertEqual, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("../../../tests/unit/njs-harness")

const req = (cookie) =>
  createMockRequest({ headersIn: cookie ? { Cookie: cookie } : {} })

async function contract(cmsenv) {
  console.log("\ncmsenv — exported names are the contract:")

  await test("exports exactly the five contract getters + the two primitives", () => {
    assertEqual(
      Object.keys(cmsenv).sort().join(","),
      [
        "detect",
        "proxyDestinationCorsham",
        "proxyDestinationModernCorsham",
        "setting",
        "upstreamCmsDomainName",
        "upstreamCmsModernDomainName",
        "upstreamCmsServicesDomainName",
      ].join(","),
      "contract names (renaming any breaks global-components' configs)"
    )
  })
}

async function detection(cmsenv) {
  // detect(cookieString) is the shared detection rule — assert it directly.
  console.log("\ndetect — cookie text -> environment token:")

  const cases = [
    ["__CMSENV=cin2", "cin2", "cin2"],
    ["__CMSENV=cin4", "cin4", "cin4"],
    ["__CMSENV=cin5", "cin5", "cin5"],
    ["__CMSENV=cin3", "default", "cin3 IS the default environment"],
    ["C-CPT-LBsessioncookie=x", "cpt", "the CPT LB cookie -> cpt (#2181 keys on '-cpt-lb')"],
    // QUIRK (#2181): detection keys on the LB cookie, so the /cpt switch's __CMSENV=cpt
    // marker ALONE no longer selects cpt — until the LB cookie arrives it routes default.
    ["__CMSENV=cpt", "default", "QUIRK: the __CMSENV=cpt marker alone -> default (#2181)"],
    ["C-CMO-LBsessioncookie=x", "default", "the CMO LB cookie -> default (RMAT-242: cmo has no upstream)"],
    ["__CMSENV=default", "default", "explicit default"],
    ["", "default", "empty -> default"],
    [undefined, "default", "missing -> default"],
    ["__CMSENV=nonsense", "default", "unknown -> default"],
  ]
  for (const [cookie, expected, label] of cases) {
    await test(`${cookie || "(none)"} -> ${expected}  (${label})`, () => {
      assertEqual(cmsenv.detect(cookie), expected, label)
    })
  }

  await test("matching is case-insensitive", () => {
    assertEqual(cmsenv.detect("__CMSENV=CIN2"), "cin2", "toLowerCase()")
  })

  // A browser's jar can hold cookies that mention environments and must choose nothing.
  const SESSION_HINT =
    "Cms-Session-Hint=%7B%22cmsDomains%22%3A%5B%22cin3.cps.gov.uk%22%5D%2C%22isProxySession%22%3Atrue%7D"

  await test("unrelated cookies never choose, whatever they contain", () => {
    assertEqual(cmsenv.detect("__CMSENV=cin5; " + SESSION_HINT), "cin5", "the hint's cin3 domain, with a marker")
    assertEqual(cmsenv.detect(SESSION_HINT.replace("cin3", "cin5")), "default", "the hint alone, without one")
    assertEqual(cmsenv.detect("somethingcin2something=1"), "default", "in a name")
    assertEqual(cmsenv.detect("user=jo.CIN2"), "default", "in a value")
    assertEqual(cmsenv.detect("x=__CMSENV=cin2"), "default", "__CMSENV inside another cookie's value")
    assertEqual(cmsenv.detect("x=C-CIN2-LBsessioncookie"), "default", "an LB name inside a value")
    assertEqual(cmsenv.detect("C-CIN5-LBsessioncookie=cin2"), "cin5", "an LB cookie's value")
  })

  await test("__CMSENV beats the load-balancer cookies, in both directions", () => {
    assertEqual(cmsenv.detect("C-CIN3-LBsessioncookie=x; __CMSENV=cin5"), "cin5", "stale cin3 LB cookie")
    assertEqual(cmsenv.detect("__CMSENV=default; C-CIN5-LBsessioncookie=x"), "default", "the /cin3 switch's marker")
    assertEqual(cmsenv.detect("__CMSENV=cin3; F-CIN2-LBsessioncookie=x"), "default", "cin3 names the default")
  })

  await test("__CMSENV is parsed: spacing and case don't matter, and the first one counts", () => {
    assertEqual(cmsenv.detect("a=b;__cmsenv = CIN4 ;c=d"), "cin4", "trimmed, any case")
    assertEqual(cmsenv.detect("__CMSENV=cin2; __CMSENV=cin5"), "cin2", "first wins")
  })

  // DDEI's CmsRequestFactory sends DEVLOGIN=<env>, and nothing else, on its dev-login GET /CMS.
  await test("DEVLOGIN chooses like __CMSENV (DDEI's and our dev logins)", () => {
    assertEqual(cmsenv.detect("DEVLOGIN=cin5;"), "cin5", "DDEI's exact form")
    assertEqual(cmsenv.detect("DEVLOGIN=cin3"), "default", "cin3 names the default")
    assertEqual(cmsenv.detect("DEVLOGIN="), "default", "blank = default")
    assertEqual(cmsenv.detect("DEVLOGIN=; C-CIN2-LBsessioncookie=x"), "cin2", "blank falls through")
    assertEqual(cmsenv.detect("C-CIN3-LBsessioncookie=x; DEVLOGIN=cin4"), "cin4", "beats an LB cookie")
  })

  await test("a __CMSENV that names no environment falls through to the LB cookies", () => {
    assertEqual(cmsenv.detect("__CMSENV=; C-CIN5-LBsessioncookie=x"), "cin5", "blank")
    assertEqual(cmsenv.detect("__CMSENV=deleted; C-CIN5-LBsessioncookie=x"), "cin5", "deleted")
    assertEqual(cmsenv.detect("__CMSENV=constructor"), "default", "not an env, nor a lookup key")
    assertEqual(cmsenv.detect("__CMSENV=cpt; C-CPT-LBsessioncookie=x"), "cpt", "cpt by its LB cookie (#2181)")
  })

  await test("without __CMSENV, the load-balancer cookies' names choose", () => {
    assertEqual(cmsenv.detect("F-CIN5-LBsessioncookie=x"), "cin5", "the F- form")
    assertEqual(cmsenv.detect("C-CIN3-LBsessioncookie=x"), "default", "cin3 IS the default")
    assertEqual(cmsenv.detect("C-CMO-LBsessioncookie=x; C-MOD-LBsessioncookie=y"), "default", "no upstream")
    assertEqual(cmsenv.detect("BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN2-cin2.cps.gov.uk_POOL=1"), "cin2", "BIG-IP")
    assertEqual(cmsenv.detect("BIGipServer~x~cin4.cps.gov.uk_POOL=1"), "cin4", "BIG-IP, short form")
  })

  // Several env's LB cookies only linger when nothing has cleared them: the old fixed order stands.
  await test("several environments' LB cookies resolve in fixed order: cin3, cin2, cin4, cin5", () => {
    assertEqual(cmsenv.detect("C-CIN2-LBsessioncookie=x; C-CIN3-LBsessioncookie=y"), "default", "cin3 first")
    assertEqual(cmsenv.detect("C-CIN5-LBsessioncookie=x; C-CIN2-LBsessioncookie=y"), "cin2", "cin2 before cin5")
  })

  await test("a Set-Cookie line (dev-login, DDEI path): the attributes choose nothing", () => {
    assertEqual(cmsenv.detect("C-CIN4-LBsessioncookie=x; path=/; domain=cin2.cps.gov.uk"), "cin4", "name, not domain")
    assertEqual(cmsenv.detect(".CMSAUTH=x; path=/; domain=cin2.cps.gov.uk"), "default", "no env in the name")
  })

  // RMAT-242: cmo has no configured upstream, so "cmo" cookies route to default
  // (the UAT default upstream is cmo.cps.gov.uk) rather than an undefined destination.
  await test("RMAT-242: 'cmo' cookies route to default (cmo has no upstream)", () => {
    assertEqual(cmsenv.detect("__CMSENV=cmo"), "default", "no 'cmo' substring -> default")
    assertEqual(cmsenv.detect("x=modern"), "default", "'modern' contains 'mod' -> default")
  })
}

async function primitives(cmsenv) {
  console.log("\nsetting — read a per-environment app setting:")

  await test("setting(r, name) reads <ENV>_<name> from process.env", () => {
    assertEqual(cmsenv.setting(req("__CMSENV=cin2"), "UPSTREAM_CMS_DOMAIN_NAME"), "cin2.cps.gov.uk", "CIN2_UPSTREAM_CMS_DOMAIN_NAME")
    assertEqual(cmsenv.setting(req("__CMSENV=cin4"), "UPSTREAM_CMS_MODERN_DOMAIN_NAME"), "cmsmodstage.cps.gov.uk", "CIN4_UPSTREAM_CMS_MODERN_DOMAIN_NAME")
    assertEqual(cmsenv.setting(req(""), "UPSTREAM_CMS_DOMAIN_NAME"), "cms.cps.gov.uk", "no cookie -> DEFAULT_ prefix")
  })
}

async function getters(cmsenv) {
  console.log("\nCMS-upstream getters (the cmsenv contract — Corsham dests + domain getters):")

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
  await detection(cmsenv)
  await primitives(cmsenv)
  await getters(cmsenv)
  restoreEnv()
  process.exit(summarise("cmsenv.js (unit)"))
}

main()
