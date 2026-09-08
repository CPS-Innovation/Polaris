#!/usr/bin/env node
/**
 * Unit tests for features/cms-proxy.js.  CHARACTERISATION.
 *
 * cms-proxy.js holds the feature-exclusive response-body filters (the only njs
 * that rewrites the CMS body): domain replacement and the P + Materials menu-bar
 * button injection. Environment detection and app-setting reads are delegated to
 * the shared common.cms-detection module (imported via `import common from "../common/cms-detection.js"`), so these
 * tests also exercise that inter-module wiring.
 *
 * Loaded from the REAL config/features/cms-proxy.js — see njs-harness.js.
 */
const {
  test,
  assert,
  assertEqual,
  assertIncludes,
  assertNotIncludes,
  summarise,
} = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, cmsEnvObject, applyEnv } = require("../../../tests/unit/njs-harness")

async function getters(cmsProxy) {
  console.log("\nCMS-upstream getters this feature owns (Corsham dests + domain getters):")

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
      assertEqual(cmsProxy.upstreamCmsDomainName(r()), e.domain, "classic domain")
      assertEqual(cmsProxy.upstreamCmsModernDomainName(r()), e.modern, "modern domain")
      assertEqual(cmsProxy.upstreamCmsServicesDomainName(r()), e.services, "services domain")
    })

    await test(`${env}: proxyDestinationCorsham / ModernCorsham build <protocol>://<ip>`, () => {
      assertEqual(cmsProxy.proxyDestinationCorsham(r()), `http://${e.ip}`, "classic Corsham")
      assertEqual(cmsProxy.proxyDestinationModernCorsham(r()), `http://${e.modernIp}`, "modern Corsham")
    })
  }

  await test("proxyDestination* uses ENDPOINT_HTTP_PROTOCOL from process.env", () => {
    const restore = applyEnv({ ENDPOINT_HTTP_PROTOCOL: "https" })
    try {
      assertEqual(cmsProxy.proxyDestinationCorsham(req("__CMSENV=cin2")), "https://10.0.2.1", "protocol from process.env")
    } finally {
      restore()
    }
  })
}

async function bodyFilters(cmsProxy) {
  console.log("\nreplaceCmsDomains — the njs body filter:")

  const run = (body, opts = {}) => {
    const r = createMockRequest({
      headersIn: { Cookie: "__CMSENV=cin2" },
      variables: { host: "proxy.example.org" },
      ...opts,
    })
    cmsProxy.replaceCmsDomains(r, body, true)
    return r
  }

  await test("passes the buffer through on a 302 (no body processing)", () => {
    const r = run("cin2.cps.gov.uk", { status: 302 })
    assertEqual(r.sentBuffer, "cin2.cps.gov.uk", "Body forwarded untouched")
    assertEqual(r.sentFlags, true, "Flags forwarded")
  })

  await test("always calls sendBuffer (the filter never drops the body)", () => {
    const r = run("hello world")
    assertEqual(r.sentBuffer, "hello world", "Body forwarded")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation) — THE BIG ONE, and the reason this file exists.
  //
  // __replaceContent builds its search regex like this:
  //
  //     let reg = /[-=./]/gm;
  //     let repold = (rep.old).replace(reg, "");   // <-- STRIPS . - = /
  //     let regexp = new RegExp(repold, 'g');
  //
  // It REMOVES the regex-special characters instead of ESCAPING them. So the
  // domain "cin2.cps.gov.uk" becomes the pattern /cin2cpsgovuk/ — which never
  // matches a real domain, because real bodies contain the dots.
  //
  // Net effect: replaceCmsDomains is effectively a NO-OP on realistic content.
  // The domain rewriting users actually get is done by nginx's `sub_filter`
  // directives in the same location blocks, not by this filter.
  //
  // (Presumably the intent was to escape the dots, i.e. \. — a one-character
  // slip with a big consequence.)
  //
  // Recorded, not fixed: if the filter is repaired, these tests must be updated
  // deliberately — and note that doing so would START rewriting content that is
  // currently passing through untouched, which is a behaviour change to think
  // about carefully.
  // ---------------------------------------------------------------------------
  await test("QUIRK: does NOT rewrite a real CMS domain (regex strips the dots)", () => {
    const r = run('<a href="https://cin2.cps.gov.uk/CMS/Home">x</a>')
    assertIncludes(r.sentBuffer, "cin2.cps.gov.uk", "The domain is still there — no rewrite happened")
    assertNotIncludes(r.sentBuffer, "proxy.example.org", "$host was not substituted in")
  })

  await test("QUIRK: it DOES rewrite the dot-less form, proving the regex is mangled", () => {
    // "cin2cpsgovuk" is what the stripped pattern actually matches. No real page
    // contains this, which is why the filter is inert in practice.
    const r = run("prefix cin2cpsgovuk suffix")
    assertIncludes(r.sentBuffer, "prefix proxy.example.org suffix", "The stripped pattern matches")
  })

  await test("replaceCmsDomainsAjaxViewer targets WEBSITE_HOSTNAME instead of host", () => {
    const r = createMockRequest({
      headersIn: { Cookie: "__CMSENV=cin2" },
      variables: { host: "req-host" },
    })
    const restore = applyEnv({ WEBSITE_HOSTNAME: "site-host" })
    try {
      cmsProxy.replaceCmsDomainsAjaxViewer(r, "prefix cin2cpsgovuk suffix", true)
      assertIncludes(r.sentBuffer, "site-host", "Uses WEBSITE_HOSTNAME, not $host")
    } finally {
      restore()
    }
  })
}

async function menuBar(cmsProxy) {
  console.log("\ncmsMenuBarFilters — P + Materials button injection:")

  const MENU_BAR = [
    "var polarisUrl = objMainWindow.top.frameData.objMasterWindow.top.frameServerJS.POLARIS_URL;",
    "var logo = MENU_BAR_POLARIS_LOGO;",
    "var sMenuBarRight = 'right';",
  ].join("\n")

  const run = (body) => {
    const r = createMockRequest({
      headersIn: { Cookie: "__CMSENV=cin2" },
      variables: { host: "proxy.example.org" },
    })
    cmsProxy.cmsMenuBarFilters(r, body, true)
    return r.sentBuffer
  }

  await test("replaces the POLARIS_URL expression with \"/polaris\"", () => {
    const out = run(MENU_BAR)
    assertIncludes(out, 'var polarisUrl = "/polaris";', "POLARIS_URL -> /polaris")
    assertNotIncludes(out, "frameServerJS.POLARIS_URL", "Original expression gone")
  })

  await test("inlines the Polaris logo as base64", () => {
    const out = run(MENU_BAR)
    assertIncludes(out, "data:image/png;base64,", "Logo inlined")
    assertNotIncludes(out, "MENU_BAR_POLARIS_LOGO", "Token gone")
  })

  await test("injects the Materials button immediately before sMenuBarRight", () => {
    const out = run(MENU_BAR)
    assertIncludes(out, "Launch Materials", "Materials button injected")
    assertIncludes(out, "openMaterials()", "Wired to openMaterials()")
    assert(
      out.indexOf("Launch Materials") < out.indexOf("var sMenuBarRight"),
      "Button must be injected BEFORE sMenuBarRight"
    )
  })

  await test("leaves a body without the tokens alone", () => {
    assertEqual(run("nothing to see here"), "nothing to see here", "No tokens -> unchanged")
  })
}

async function cinSwitchTests(cmsProxy) {
  console.log("\ncinSwitch — /cin env switch (IE gate + cookies + redirect):")
  const restore = applyEnv({ WEBSITE_SCHEME: "https" })
  const IE_UA = "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko"
  const EDGE_UA = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 Chrome/120 Safari/537.36"

  // cinSwitch now gates via ieMode.coerce, which reads the UA + configurable HEADERS
  // (not $ieaction) — so drive it with headers, as production requests do.
  const call = (uri, ua, conf) => {
    const r = createMockRequest({
      uri,
      headersIn: {
        ...(ua !== undefined ? { "User-Agent": ua } : {}),
        ...(conf !== undefined ? { "X-InternetExplorerModeConfigurable": conf } : {}),
      },
      variables: { host: "polaris.example", request_uri: uri },
    })
    cmsProxy.cinSwitch(r)
    return r
  }
  const cookies = (uri) => call(uri, IE_UA).headersOut["Set-Cookie"] // IE UA -> proceeds

  // --- IE-desired gate (delegated to ieMode.coerce) ---
  await test("nonie+nonconfigurable -> 402", () => {
    assertEqual(call("/cin2", EDGE_UA).returnCode, 402)
  })
  await test("nonie+configurable -> 302 self + X-InternetExplorerMode: 1", () => {
    const r = call("/cin2", EDGE_UA, "1")
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.headersOut["X-InternetExplorerMode"], "1", "asks for IE mode")
    assertEqual(r.returnBody, "https://polaris.example/cin2", "self-redirect to request_uri")
  })

  // --- proceed (IE browser): redirect + env cookie + clearing ---
  await test("ie -> 302 to /CMS", () => {
    assertEqual(call("/cin2", IE_UA).returnBody, "https://polaris.example/CMS")
  })
  await test("__CMSENV set per env — cin3 IS 'default' (QUIRK D9); /cpt IS 'cpt'", () => {
    assertIncludes(cookies("/cin2").join("\n"), "__CMSENV=cin2", "cin2")
    assertIncludes(cookies("/cin3").join("\n"), "__CMSENV=default", "cin3 -> default")
    assertIncludes(cookies("/cin4").join("\n"), "__CMSENV=cin4", "cin4")
    assertIncludes(cookies("/cin5").join("\n"), "__CMSENV=cin5", "cin5")
    assertIncludes(cookies("/cpt").join("\n"), "__CMSENV=cpt", "cpt (FCT2-18732)")
  })
  await test("clears the OTHER envs' pool + LB cookies, leaves the current env's alone", () => {
    const joined = cookies("/cin2").join("\n")
    assertIncludes(joined, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "clears cin3 pool")
    assertIncludes(joined, "F-CIN5-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "clears cin5 LB")
    assertNotIncludes(joined, "CIN2-cin2", "does NOT clear the current env (cin2)")
  })
  await test("every switch also clears the MOD + CPT LB session cookies (FCT2-18732)", () => {
    const joined = cookies("/cin2").join("\n")
    assertIncludes(joined, "C-MOD-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "clears MOD LB")
    assertIncludes(joined, "F-CPT-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "clears CPT LB")
  })
  await test("cin switch = 17 Set-Cookie: __CMSENV + 3 cin x (2 pool + 2 lb) + mod/cpt x 2 lb", () => {
    assertEqual(cookies("/cin2").length, 1 + 3 * 4 + 2 * 2)
  })
  await test("/cpt clears CIN3 + MOD, NOT its own CPT (fixes the live /cpt bug)", () => {
    const joined = cookies("/cpt").join("\n")
    // Live /cpt regressed: it missed CIN3 and cleared its own CPT. Clear-all-except-target fixes both.
    assertIncludes(joined, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "clears cin3 pool")
    assertIncludes(joined, "F-CIN3-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "clears cin3 LB")
    assertIncludes(joined, "C-MOD-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "clears the other LB-only env (mod)")
    assertNotIncludes(joined, "CPT-LBsessioncookie", "does NOT clear its own env (cpt)")
  })
  await test("/cpt switch = 19 Set-Cookie: __CMSENV + 4 cin x (2 pool + 2 lb) + mod x 2 lb", () => {
    assertEqual(cookies("/cpt").length, 1 + 4 * 4 + 1 * 2)
  })

  restore()
}

async function main() {
  const cmsProxy = await loadNjs("features/cms-proxy/cms-proxy.js")
  const restoreEnv = applyEnv(cmsEnvObject())
  await getters(cmsProxy)
  await bodyFilters(cmsProxy)
  await menuBar(cmsProxy)
  await cinSwitchTests(cmsProxy)
  restoreEnv()
  process.exit(summarise("cms-proxy.js (unit)"))
}

main()
