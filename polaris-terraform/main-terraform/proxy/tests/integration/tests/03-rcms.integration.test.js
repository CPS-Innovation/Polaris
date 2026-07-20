#!/usr/bin/env node
/**
 * Feature 3 🧭 — rCMS / navigate-cms.  CHARACTERISATION.
 *
 * Locations covered (docs/PROXY.md §4):
 *   nginx.conf:127  location = /api/navigate-cms    (js_set cmsenv.getDomainFromCookie)
 *   nginx.conf:143  location = /navigate-cms-close
 *
 * Both branch on $ieaction, which nginx.conf:72-85 derives from TWO request
 * inputs — so every branch is drivable from a test:
 *   $http_x_internetexplorermodeconfigurable = 1  -> "configurable+"  else "nonconfigurable+"
 *   $http_user_agent =~ Trident                   -> "ie+"            else "nonie+"
 * giving: ie+configurable+ | nonie+configurable+ | ie+nonconfigurable+ | nonie+nonconfigurable+
 *
 * §4 notes both are "candidate to move to global components".
 */
const { test, assert, assertEqual, assertIncludes, get, summarise } = require("../test-utils")

const IE_UA = "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko"
const EDGE_UA = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 Chrome/120 Safari/537.36"

const asIe = (configurable) => ({
  "User-Agent": IE_UA,
  ...(configurable ? { "X-InternetExplorerModeConfigurable": "1" } : {}),
})
const asEdge = (configurable) => ({
  "User-Agent": EDGE_UA,
  ...(configurable ? { "X-InternetExplorerModeConfigurable": "1" } : {}),
})

// getDomainFromCookie matches /([a-z0-9]+)\.cps\.gov\.uk/ against the Cookie header.
const CMS_COOKIE = "BIGipServer~x~cin2.cps.gov.uk_POOL=1"

async function navigateCms() {
  console.log("\nlocation = /api/navigate-cms:")

  await test("Edge + configurable -> 302 into IE mode, with domain from cookie", async () => {
    const res = await get("/api/navigate-cms?caseId=1", {
      headers: { ...asEdge(true), Cookie: CMS_COOKIE },
    })
    assertEqual(res.status, 302, "Should 302 to re-enter in IE mode")
    assertEqual(res.headers.get("x-internetexplorermode"), "1", "Should request IE mode")
    assertIncludes(res.headers.get("location"), "&domain=", "Should append the domain from the cookie")
    assertIncludes(
      res.headers.get("location"),
      "domain=cin2.cps.gov.uk",
      "domain should come from cmsenv.getDomainFromCookie"
    )
  })

  await test("IE mode -> 200 HTML with the two notification iframes", async () => {
    const res = await get("/api/navigate-cms?caseId=1&domain=cin2.cps.gov.uk", {
      headers: { ...asIe(true), Cookie: CMS_COOKIE },
    })
    assertEqual(res.status, 200, "Should render the iframe page")
    const body = await res.text()
    assertIncludes(body, "<iframe", "Should render iframes")
    assertIncludes(body, "/CMSModern/Navigation/Notification.html", "Should target the CMS notification page")
    assertIncludes(body, "navigate-cms-close", "Should redirect to the close helper once loaded")
    assertIncludes(body, "https://cin2.cps.gov.uk/CMSModern", "Second iframe targets $arg_domain")
  })

  await test("is text/html in IE mode", async () => {
    const res = await get("/api/navigate-cms?caseId=1", {
      headers: { ...asIe(true), Cookie: CMS_COOKIE },
    })
    assertIncludes(res.headers.get("content-type"), "text/html", "Content-Type")
  })

  await test("exact match — does not fall through to the /api/ gateway proxy", async () => {
    const res = await get("/api/navigate-cms", {
      headers: { ...asIe(true), Cookie: CMS_COOKIE },
    })
    assertEqual(res.headers.get("x-mock-echo"), null, "Should NOT proxy to the gateway")
  })
}

async function navigateCmsClose() {
  console.log("\nlocation = /navigate-cms-close:")

  await test("IE + configurable -> 302 back to Edge mode", async () => {
    const res = await get("/navigate-cms-close", { headers: asIe(true) })
    assertEqual(res.status, 302, "Should 302 to leave IE mode")
    assertEqual(res.headers.get("x-internetexplorermode"), "0", "Should turn IE mode off")
  })

  await test("otherwise -> 200 window.close() page", async () => {
    const res = await get("/navigate-cms-close", { headers: asEdge(false) })
    assertEqual(res.status, 200, "Should render the close page")
    assertIncludes(await res.text(), "window.close()", "Should close the tab")
  })
}

async function main() {
  await navigateCms()
  await navigateCmsClose()
  process.exit(summarise("rCMS / navigate-cms (feature 3)"))
}

main()
