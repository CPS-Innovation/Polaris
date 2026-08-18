#!/usr/bin/env node
/**
 * Feature 5 🔀 — CMS proxy.  CHARACTERISATION.
 *
 * The refactor's blast radius: every location here routes through cmsenv.js,
 * which docs/PROXY.md §6.2 wants to condense into common.js. These tests are the
 * safety net for that.
 *
 * Locations covered (docs/PROXY.md §4) — regexes written without their slashes
 * here, because a literal "* /" sequence would close this comment block:
 *   nginx.conf:157  /polaris-script.js                        (served asset)
 *   nginx.conf:164  ~ ^/CMS\.  …Includes/uainGeneratedScript.aspx  (c-button rewrite)
 *   nginx.conf:203  ~ ^/CMS\.  …Noexpiry/Toolbar/uainMenuBar.js    (button injection)
 *   nginx.conf:279  ~ ^/CMS\.  …Case/uacdCDTabs.aspx               (script injection)
 *   nginx.conf:309  ~ ^/CMSModern/Files                            (CMS Services)
 *   nginx.conf:354  ~ ^/CMS                                        (classic fallback)
 *   nginx.conf:400  /cin3    nginx.conf:430  /cin2                 (env switch)
 *   nginx.conf:460  /cin4    nginx.conf:490  /cin5                 (env switch)
 *   nginx.conf:520  /                                              (CMS Modern root)
 *   nginx.conf:556  /ajax/viewer/                                  (ajax viewer)
 *
 * NOTE the regexes require a DOT: the path must look like /CMS.<something>/...
 * (e.g. /CMS.Live/Home), not /CMS/Home.
 */
const {
  test,
  assert,
  assertEqual,
  assertIncludes,
  assertNotIncludes,
  get,
  summarise,
  isNext,
} = require("../../../tests/integration/test-utils")

const IE_UA = "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko"
const EDGE_UA = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 Chrome/120 Safari/537.36"
const ie = { "User-Agent": IE_UA }

/** cmsenv.__getCmsEnvInternal lowercases the Cookie header and substring-matches. */
const envCookie = (v) => ({ Cookie: `__CMSENV=${v}` })

/**
 * The CMS blocks gate on $ieaction: without a Trident user-agent they answer
 * 402 "requires Internet Explorer mode" (see the non-IE test below and
 * zz-cross-cutting). So every request INTO the CMS proxy carries an IE UA.
 */
const cget = (path, headers = {}) => get(path, { headers: { "User-Agent": IE_UA, ...headers } })

async function servedAsset() {
  console.log("\nlocation /polaris-script.js — the injected client script:")

  await test("serves polaris-script.js as a local static asset", async () => {
    const res = await get("/polaris-script.js")
    assertEqual(res.status, 200, "Should serve the file")
    assertIncludes(res.headers.get("content-type"), "javascript", "Should be javascript")
  })

  await test("is the real file, not the upstream", async () => {
    const res = await get("/polaris-script.js")
    assertEqual(res.headers.get("x-mock-echo"), null, "Should be served locally, not proxied")
    assert((await res.text()).length > 0, "Should have a body")
  })
}

async function cmsEnvSelection() {
  console.log("\ncmsenv.js environment selection (via the __CMSENV cookie):")

  // The mock echoes the Host it received in X-Mock-Host even on fixture routes.
  // Host is set from $upstreamCmsDomainName, so it reveals which environment
  // cmsenv.js picked — the single most important behaviour to pin before
  // condensing cmsenv.js into common.js.
  const cases = [
    ["cin2", "cin2.cps.gov.uk"],
    ["cin4", "cin4.cps.gov.uk"],
    ["cin5", "cin5.cps.gov.uk"],
    ["cin3", "cms.cps.gov.uk"], // cin3 maps to DEFAULT
  ]
  for (const [cookie, expectedDomain] of cases) {
    await test(`__CMSENV=${cookie} -> upstream Host ${expectedDomain}`, async () => {
      const res = await cget("/CMS.Live/Home", envCookie(cookie))
      assertEqual(res.status, 200, "Should proxy")
      assertIncludes(res.headers.get("x-mock-host"), expectedDomain, "Selected CMS environment")
    })
  }

  await test("no __CMSENV cookie -> DEFAULT environment", async () => {
    const res = await cget("/CMS.Live/Home")
    assertIncludes(res.headers.get("x-mock-host"), "cms.cps.gov.uk", "Should default")
  })

  await test("unknown __CMSENV value -> DEFAULT environment", async () => {
    const res = await cget("/CMS.Live/Home", envCookie("nonsense"))
    assertIncludes(res.headers.get("x-mock-host"), "cms.cps.gov.uk", "Should default")
  })
}

async function cmsClassic() {
  console.log("\nlocation ~ ^/CMS.* — CMS classic fallback:")

  await test("proxies a generic CMS path", async () => {
    const res = await cget("/CMS.Live/Home")
    assertEqual(res.status, 200, "Should proxy")
    assertEqual(res.headers.get("x-mock-fixture"), "cms-page.html", "Should hit the CMS fixture")
  })

  await test("rewrites the CMS domain to $host in the body", async () => {
    const res = await cget("/CMS.Live/Home")
    const body = await res.text()
    assertIncludes(body, "http://localhost/CMS/Home", "CMS domain rewritten to $host (which carries NO port)")
  })

  await test("rewrites https:// to ${WEBSITE_SCHEME}:// in the body", async () => {
    // WEBSITE_SCHEME is http in this harness, so https:// links become http://.
    const res = await cget("/CMS.Live/Home")
    const body = await res.text()
    assertNotIncludes(body, "https://localhost", "https:// should have been rewritten to the scheme")
  })
}

async function cmsModernFiles() {
  console.log("\nlocation ~ ^/CMSModern/Files.* — CMS Services:")

  await test("proxies /CMSModern/Files", async () => {
    const res = await cget("/CMSModern/Files/doc/1")
    assertEqual(res.status, 200, "Should proxy")
    assert(res.headers.get("x-mock-fixture"), "Should hit a fixture")
  })

  await test("precedence: matches before the ^/CMS.* fallback", async () => {
    // /CMSModern/... also matches ^/CMS.* — first-matching-regex wins, so the
    // Files block MUST stay above the fallback in nginx.conf (docs/PROXY.md §5).
    const res = await cget("/CMSModern/Files/doc/1")
    assertIncludes(res.headers.get("x-mock-host"), "cps.gov.uk", "Should have selected a CMS host")
  })
}

async function cButtonRewrite() {
  console.log("\nlocation ~ .../Includes/uainGeneratedScript.aspx — c-button rewrite:")

  await test("rewrites CASEWORK_TOOLS_URL /launch/cms -> /launch/cms-proxy", async () => {
    const res = await cget("/CMS.Live/Includes/uainGeneratedScript.aspx")
    assertEqual(res.status, 200, "Should proxy")
    const body = await res.text()
    assertIncludes(body, "/launch/cms-proxy", "c-button should point at the proxied launch route")
    assertNotIncludes(body, "/launch/cms'", "The un-proxied /launch/cms should be gone")
  })

  await test("rewrites CMS domains to $host", async () => {
    const res = await cget("/CMS.Live/Includes/uainGeneratedScript.aspx")
    assertIncludes(await res.text(), "http://localhost/CMS/Home", "Domains rewritten to $host (no port)")
  })
}

async function menuBarInjection() {
  console.log("\nlocation ~ .../Noexpiry/Toolbar/uainMenuBar.js — button injection (cmsenv.cmsMenuBarFilters):")

  await test("replaces POLARIS_URL with /polaris", async () => {
    const res = await cget("/CMS.Live/Noexpiry/Toolbar/uainMenuBar.js")
    assertEqual(res.status, 200, "Should proxy")
    const body = await res.text()
    assertIncludes(body, '"/polaris"', "POLARIS_URL should become /polaris")
    assertNotIncludes(body, "frameServerJS.POLARIS_URL", "The original token should be gone")
  })

  await test("injects the Polaris logo", async () => {
    const res = await cget("/CMS.Live/Noexpiry/Toolbar/uainMenuBar.js")
    const body = await res.text()
    assertIncludes(body, "data:image/png;base64,", "Logo should be inlined")
    assertNotIncludes(body, "MENU_BAR_POLARIS_LOGO", "The logo token should be gone")
  })

  await test("injects the Materials button before sMenuBarRight", async () => {
    const res = await cget("/CMS.Live/Noexpiry/Toolbar/uainMenuBar.js")
    const body = await res.text()
    assertIncludes(body, "Launch Materials", "Materials button should be injected")
    assertIncludes(body, "openMaterials()", "Materials button should wire up openMaterials()")
  })
}

async function scriptInjection() {
  console.log("\nlocation ~ .../Case/uacdCDTabs.aspx — polaris-script.js injection:")

  await test("injects the polaris-script.js tag before </html>", async () => {
    const res = await cget("/CMS.Live/Case/uacdCDTabs.aspx")
    assertEqual(res.status, 200, "Should proxy")
    assertIncludes(
      await res.text(),
      '<script src="/polaris-script.js"></script></html>',
      "Script tag should be injected"
    )
  })
}

async function envSwitch() {
  console.log("\nlocation /cin2 /cin3 /cin4 /cin5 /cpt — environment switch:")

  const cases = [
    ["/cin2", "cin2"],
    ["/cin3", "default"], // cin3 IS the default environment
    ["/cin4", "cin4"],
    ["/cin5", "cin5"],
    ["/cpt", "cpt"], // brought across from the live config (FCT2-18732)
  ]
  for (const [path, expectedEnv] of cases) {
    await test(`${path} (IE mode) sets __CMSENV=${expectedEnv} and 302s to /CMS`, async () => {
      const res = await get(path, { headers: ie })
      assertEqual(res.status, 302, "Should 302")
      assertIncludes(res.headers.get("location"), "/CMS", "Should land on /CMS")
      const setCookie = res.headers.getSetCookie().join("; ")
      assertIncludes(setCookie, `__CMSENV=${expectedEnv}`, "Should set the environment cookie")
    })
  }

  await test("/cin2 clears the BIG-IP and LB session cookies of other envs", async () => {
    const res = await get("/cin2", { headers: ie })
    const setCookie = res.headers.getSetCookie().join("; ")
    assertIncludes(setCookie, "expires=Thu, 01 Jan 1970", "Should expire stale cookies")
    assertIncludes(setCookie, "LBsessioncookie=deleted", "Should clear LB session cookies")
  })

  await test("/cin2 clears the OTHER envs' EXACT pool/LB cookies, not its own", async () => {
    const res = await get("/cin2", { headers: ie })
    const setCookie = res.headers.getSetCookie().join("\n")
    assertIncludes(setCookie, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "exact cin3 pool cookie")
    assertIncludes(setCookie, "F-CIN5-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "exact cin5 LB cookie")
    if (setCookie.indexOf("CIN2-cin2") !== -1) throw new Error("must NOT clear its own env (cin2)")
  })

  await test("every switch also clears the MOD + CPT LB session cookies (FCT2-18732)", async () => {
    const res = await get("/cin2", { headers: ie })
    const setCookie = res.headers.getSetCookie().join("\n")
    assertIncludes(setCookie, "C-MOD-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "exact MOD LB cookie")
    assertIncludes(setCookie, "F-CPT-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "exact CPT LB cookie")
  })

  // NEXT-ONLY: our next-gen cinSwitch fixes the /cpt cookie-clearing bug by construction.
  // The LIVE monolith carries the original (buggy) FCT2-18732 /cpt block on this branch — the
  // fix ships to live via a SEPARATE maintainer PR (FCT2-21518), so we don't assert the fixed
  // behaviour against the live config here. Gated on isNext (PROXY_CONFIG_KIND from run-tests.sh).
  if (isNext) {
    await test("/cpt clears CIN3 and does NOT clear its own CPT cookie (next-gen fix; live via FCT2-21518)", async () => {
      const res = await get("/cpt", { headers: ie })
      const setCookie = res.headers.getSetCookie().join("\n")
      // The live /cpt block regressed on both of these (missed CIN3, cleared its own CPT).
      assertIncludes(setCookie, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "must clear cin3 pool")
      assertIncludes(setCookie, "F-CIN3-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "must clear cin3 LB")
      assertIncludes(setCookie, "C-MOD-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "must clear the other LB-only env (mod)")
      if (setCookie.indexOf("CPT-LBsessioncookie") !== -1) throw new Error("must NOT clear its own env (cpt)")
    })
  }

  await test("non-IE + non-configurable -> 402 'requires Internet Explorer mode'", async () => {
    const res = await get("/cin2", { headers: { "User-Agent": EDGE_UA } })
    assertEqual(res.status, 402, "Should 402")
    assertIncludes(await res.text(), "requires Internet Explorer mode", "Should explain why")
  })

  await test("non-IE + configurable -> 302 asking the client to switch to IE mode", async () => {
    const res = await get("/cin2", {
      headers: { "User-Agent": EDGE_UA, "X-InternetExplorerModeConfigurable": "1" },
    })
    assertEqual(res.status, 302, "Should 302 to re-enter in IE mode")
    assertEqual(res.headers.get("x-internetexplorermode"), "1", "Should request IE mode")
  })
}

async function modernRoot() {
  console.log("\nlocation / — CMS Modern root proxy:")

  await test("an unmatched path proxies to CMS Modern", async () => {
    // §4: "Has always been a bit dodgy just being on the root".
    const res = await cget("/some-unmatched-path")
    assertEqual(res.status, 200, "Should proxy")
    assertIncludes(res.headers.get("x-mock-host"), "cps.gov.uk", "Should select a CMS host")
  })

  await test("does not swallow the exact-match health check", async () => {
    const res = await get("/")
    assertIncludes(await res.text(), "Polaris Proxy is online", "= / must win over the / prefix")
  })
}

async function ajaxViewer() {
  console.log("\nlocation /ajax/viewer/ — CMS Modern ajax viewer:")

  await test("proxies to CMS Modern", async () => {
    const res = await cget("/ajax/viewer/doc/1")
    assertEqual(res.status, 200, "Should proxy")
    assertIncludes(res.headers.get("x-mock-host"), "cps.gov.uk", "Should select a CMS host")
  })
}

async function main() {
  await servedAsset()
  await cmsEnvSelection()
  await cmsClassic()
  await cmsModernFiles()
  await cButtonRewrite()
  await menuBarInjection()
  await scriptInjection()
  await envSwitch()
  await modernRoot()
  await ajaxViewer()
  process.exit(summarise("CMS proxy (feature 5)"))
}

main()
