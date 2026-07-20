#!/usr/bin/env node
/**
 * Unit tests for cmsenv.js.  CHARACTERISATION.
 *
 * cmsenv.js is the refactor's centre of gravity: docs/PROXY.md §6.2 wants its
 * CMS-environment resolution condensed into common.js. Integration can only
 * reach this logic indirectly (via a Host header); these tests pin it directly —
 * every environment, every getter, and the body filters.
 *
 * Loaded from the REAL ../../../cmsenv.js — see njs-harness.js.
 */
const {
  test,
  assert,
  assertEqual,
  assertDeepEqual,
  assertIncludes,
  assertNotIncludes,
  assertThrows,
  summarise,
} = require("./test-utils")
const { loadNjs, createMockRequest, cmsVariables } = require("./njs-harness")

const req = (cookie, extra = {}) =>
  createMockRequest({
    headersIn: cookie ? { Cookie: cookie } : {},
    variables: cmsVariables(),
    ...extra,
  })

async function envDetection(cmsenv) {
  console.log("\n__getCmsEnv — cookie -> environment:")

  // Probed through upstreamCmsDomainName, since __getCmsEnv is private.
  const cases = [
    ["__CMSENV=cin2", "cin2.cps.gov.uk", "cin2"],
    ["__CMSENV=cin4", "cin4.cps.gov.uk", "cin4"],
    ["__CMSENV=cin5", "cin5.cps.gov.uk", "cin5"],
    ["__CMSENV=cin3", "cms.cps.gov.uk", "cin3 IS the default environment"],
    ["__CMSENV=default", "cms.cps.gov.uk", "explicit default"],
    ["", "cms.cps.gov.uk", "no cookie -> default"],
    ["__CMSENV=nonsense", "cms.cps.gov.uk", "unknown -> default"],
  ]
  for (const [cookie, expected, label] of cases) {
    await test(`${cookie || "(no cookie)"} -> ${expected}  (${label})`, () => {
      assertEqual(cmsenv.upstreamCmsDomainName(req(cookie)), expected, label)
    })
  }

  await test("matching is case-insensitive", () => {
    assertEqual(cmsenv.upstreamCmsDomainName(req("__CMSENV=CIN2")), "cin2.cps.gov.uk", "toLowerCase()")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation): detection is a LOOSE SUBSTRING match over the whole
  // Cookie header — not a parse of the __CMSENV value. So ANY cookie containing
  // "cin2" anywhere selects cin2, even an unrelated one.
  // ---------------------------------------------------------------------------
  await test("QUIRK: any cookie containing 'cin2' selects cin2, even unrelated ones", () => {
    assertEqual(
      cmsenv.upstreamCmsDomainName(req("BIGipServer~x~cin2.cps.gov.uk_POOL=1")),
      "cin2.cps.gov.uk",
      "Substring match over the whole Cookie header"
    )
    assertEqual(
      cmsenv.upstreamCmsDomainName(req("somethingcin2something=1")),
      "cin2.cps.gov.uk",
      "Not anchored to the __CMSENV cookie at all"
    )
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation): the checks run in the order cin3, cin2, cin4, cin5
  // and return on the FIRST hit. A Cookie header mentioning several environments
  // resolves by that fixed order, not by which cookie is __CMSENV.
  // ---------------------------------------------------------------------------
  await test("QUIRK: cin3 wins over cin2 when both appear (fixed check order)", () => {
    assertEqual(
      cmsenv.upstreamCmsDomainName(req("__CMSENV=cin2; other=cin3")),
      "cms.cps.gov.uk",
      "cin3 is checked first, so this resolves to DEFAULT despite __CMSENV=cin2"
    )
  })

  await test("QUIRK: cin2 wins over cin4/cin5 when several appear", () => {
    assertEqual(
      cmsenv.upstreamCmsDomainName(req("__CMSENV=cin5; stale=cin2")),
      "cin2.cps.gov.uk",
      "cin2 is checked before cin4/cin5"
    )
  })
}

async function getters(cmsenv) {
  console.log("\nUpstream getters — every environment:")

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

    await test(`${env}: Corsham / Farnborough IPs`, () => {
      assertEqual(cmsenv.upstreamCmsIpCorsham(r()), e.ip, "classic Corsham IP")
      assertEqual(cmsenv.upstreamCmsModernIpCorsham(r()), e.modernIp, "modern Corsham IP")
      assertEqual(cmsenv.upstreamCmsIpFarnborough(r()), `${e.ip}-fb`, "classic Farnborough IP")
      assertEqual(cmsenv.upstreamCmsModernIpFarnborough(r()), `${e.modernIp}-fb`, "modern Farnborough IP")
    })

    await test(`${env}: proxyDestination* build <protocol>://<ip>`, () => {
      assertEqual(cmsenv.proxyDestinationCorsham(r()), `http://${e.ip}`, "classic Corsham")
      assertEqual(cmsenv.proxyDestinationModernCorsham(r()), `http://${e.modernIp}`, "modern Corsham")
      assertEqual(cmsenv.proxyDestinationFarnborough(r()), `http://${e.ip}-fb`, "classic Farnborough")
      assertEqual(cmsenv.proxyDestinationModernFarnborough(r()), `http://${e.modernIp}-fb`, "modern Farnborough")
    })
  }

  await test("proxyDestination uses endpointHttpProtocol", () => {
    const r = createMockRequest({
      headersIn: { Cookie: "__CMSENV=cin2" },
      variables: cmsVariables({ endpointHttpProtocol: "https" }),
    })
    assertEqual(cmsenv.proxyDestinationCorsham(r), "https://10.0.2.1", "protocol comes from the js_var")
  })

  console.log("\n*Internal variants (used by the polaris-ddei routes):")

  await test("the four *Internal fns are pure delegates to their public twins", () => {
    // docs/PROXY.md §6: these exist only to give the internal routes a distinct
    // js_set name — they add no behaviour, so they are free to collapse.
    const r = () => req("__CMSENV=cin4")
    assertEqual(cmsenv.proxyDestinationCorshamInternal(r()), cmsenv.proxyDestinationCorsham(r()), "corsham")
    assertEqual(cmsenv.proxyDestinationModernCorshamInternal(r()), cmsenv.proxyDestinationModernCorsham(r()), "corsham modern")
    assertEqual(cmsenv.proxyDestinationFarnboroughInternal(r()), cmsenv.proxyDestinationFarnborough(r()), "farnborough")
    assertEqual(cmsenv.proxyDestinationModernFarnboroughInternal(r()), cmsenv.proxyDestinationModernFarnborough(r()), "farnborough modern")
  })
}

async function domainFromCookie(cmsenv) {
  console.log("\ngetDomainFromCookie (feature 3 — navigate-cms):")

  await test("returns the first *.cps.gov.uk domain in the Cookie header", () => {
    assertEqual(
      cmsenv.getDomainFromCookie(req("BIGipServer~x~cin2.cps.gov.uk_POOL=1")),
      "cin2.cps.gov.uk",
      "first match"
    )
  })

  await test("is independent of __getCmsEnv (its own regex)", () => {
    // Note it returns the domain it FINDS, which need not agree with the
    // environment __getCmsEnv would select — they are separate mechanisms.
    assertEqual(cmsenv.getDomainFromCookie(req("a=cin4.cps.gov.uk_POOL")), "cin4.cps.gov.uk", "own regex")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation): no match -> `domainMatch[0]` on null -> TypeError.
  // In nginx this surfaces as an njs exception on the js_set, not a clean value.
  // ---------------------------------------------------------------------------
  await test("QUIRK: throws when the cookie has no *.cps.gov.uk domain", () => {
    const err = assertThrows(
      () => cmsenv.getDomainFromCookie(req("unrelated=1")),
      "Should have thrown on a non-matching cookie"
    )
    assertIncludes(String(err), "null", "Reads [0] of a null match")
  })

  await test("QUIRK: throws when there is no Cookie header at all", () => {
    assertThrows(() => cmsenv.getDomainFromCookie(req("")), "Should have thrown")
  })
}

async function bodyFilters(cmsenv) {
  console.log("\nreplaceCmsDomains — the njs body filter:")

  const run = (body, opts = {}) => {
    const r = createMockRequest({
      headersIn: { Cookie: "__CMSENV=cin2" },
      variables: cmsVariables(),
      ...opts,
    })
    cmsenv.replaceCmsDomains(r, body, true)
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

  await test("replaceCmsDomainsAjaxViewer targets websiteHostname instead of host", () => {
    const r = createMockRequest({
      headersIn: { Cookie: "__CMSENV=cin2" },
      variables: cmsVariables({ host: "req-host", websiteHostname: "site-host" }),
    })
    cmsenv.replaceCmsDomainsAjaxViewer(r, "prefix cin2cpsgovuk suffix", true)
    assertIncludes(r.sentBuffer, "site-host", "Uses $websiteHostname, not $host")
  })
}

async function menuBar(cmsenv) {
  console.log("\ncmsMenuBarFilters — P + Materials button injection:")

  const MENU_BAR = [
    "var polarisUrl = objMainWindow.top.frameData.objMasterWindow.top.frameServerJS.POLARIS_URL;",
    "var logo = MENU_BAR_POLARIS_LOGO;",
    "var sMenuBarRight = 'right';",
  ].join("\n")

  const run = (body) => {
    const r = createMockRequest({
      headersIn: { Cookie: "__CMSENV=cin2" },
      variables: cmsVariables(),
    })
    cmsenv.cmsMenuBarFilters(r, body, true)
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

async function devLogin(cmsenv) {
  console.log("\ndevLoginEnvCookie (feature 2 — dev login):")

  await test("appends __CMSENV derived from the OUTGOING Set-Cookie", () => {
    // Note it reads headersOut, not headersIn — it echoes the environment that
    // the dev-login response is establishing.
    const r = createMockRequest({
      variables: cmsVariables(),
      headersOut: { "Set-Cookie": ["BIGipServer~x~cin4.cps.gov.uk_POOL=1"] },
    })
    cmsenv.devLoginEnvCookie(r)
    const cookies = r.headersOut["Set-Cookie"]
    assert(cookies.some((c) => c === "__CMSENV=cin4; path=/"), `Expected __CMSENV=cin4, got ${JSON.stringify(cookies)}`)
  })

  await test("defaults to 'default' when the outgoing cookie names no environment", () => {
    const r = createMockRequest({
      variables: cmsVariables(),
      headersOut: { "Set-Cookie": ["session=abc"] },
    })
    cmsenv.devLoginEnvCookie(r)
    assert(
      r.headersOut["Set-Cookie"].some((c) => c === "__CMSENV=default; path=/"),
      "Should fall back to default"
    )
  })

  await test("preserves the existing Set-Cookie entries", () => {
    const r = createMockRequest({
      variables: cmsVariables(),
      headersOut: { "Set-Cookie": ["a=1", "b=2"] },
    })
    cmsenv.devLoginEnvCookie(r)
    assertEqual(r.headersOut["Set-Cookie"].length, 3, "Should append, not replace")
  })
}

async function main() {
  const cmsenv = await loadNjs("cmsenv.js")
  await envDetection(cmsenv)
  await getters(cmsenv)
  await domainFromCookie(cmsenv)
  await bodyFilters(cmsenv)
  await menuBar(cmsenv)
  await devLogin(cmsenv)
  process.exit(summarise("cmsenv.js (unit)"))
}

main()
