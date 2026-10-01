#!/usr/bin/env node
/**
 * Unit tests for global-components.js — only the bits this repo owns.
 *
 * global-components' full behaviour is covered by the sibling repo
 * (global-components/infra/proxy). Here we unit-test getDomainFromCookie, which
 * was moved in from cmsenv.js and drives the legacy /api/navigate-cms block, and
 * the FCT2-22132 additions (full-host case-review redirects — an open-redirect
 * guard — and the cpslon-tst CORS origin). The other handlers are exercised via
 * the integration smoke test.
 */
const { test, assertEqual, assertIncludes, assertThrows, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest } = require("../../../tests/unit/njs-harness")

const req = (cookie) => createMockRequest({ headersIn: cookie ? { Cookie: cookie } : {} })

async function domainFromCookie(gloco) {
  console.log("\ngetDomainFromCookie (legacy navigate-cms):")

  await test("returns the first *.cps.gov.uk domain in the Cookie header", () => {
    assertEqual(
      gloco.getDomainFromCookie(req("BIGipServer~x~cin2.cps.gov.uk_POOL=1")),
      "cin2.cps.gov.uk",
      "first match"
    )
  })

  await test("finds the domain by its own regex (not the __CMSENV env)", () => {
    assertEqual(gloco.getDomainFromCookie(req("a=cin4.cps.gov.uk_POOL")), "cin4.cps.gov.uk", "own regex")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation): no match -> domainMatch[0] on null -> TypeError.
  // In nginx this surfaces as an njs exception on the js_set, not a clean value.
  // See QUIRKS.md C4. Preserved verbatim through the move from cmsenv.js.
  // ---------------------------------------------------------------------------
  await test("QUIRK: throws when the cookie has no *.cps.gov.uk domain", () => {
    const err = assertThrows(
      () => gloco.getDomainFromCookie(req("unrelated=1")),
      "Should have thrown on a non-matching cookie"
    )
    assertIncludes(String(err), "null", "Reads [0] of a null match")
  })

  await test("QUIRK: throws when there is no Cookie header at all", () => {
    assertThrows(() => gloco.getDomainFromCookie(req("")), "Should have thrown")
  })
}

// FCT2-22132: /case-review-redirect/{osHost}/{envFolder} — osHost is an
// outsystemsenterprise.com subdomain (original form) or a full host in our estate.
async function caseReviewRedirect(gloco) {
  console.log("\nhandleCaseReviewRedirect — OS host forms (FCT2-22132):")

  const run = (uri) => {
    const r = createMockRequest({
      uri,
      args: { CMSCaseId: "2", URN: "1" },
      headersIn: { Host: "polaris.cps.gov.uk", "X-Forwarded-Proto": "https" },
    })
    gloco.handleCaseReviewRedirect(r)
    return r
  }
  const landing = (r) => decodeURIComponent(decodeURIComponent(r.returnBody))

  await test("subdomain form still expands to <sub>.outsystemsenterprise.com", () => {
    const r = run("/case-review-redirect/cps-tst/test")
    assertEqual(r.returnCode, 302, "302")
    assertIncludes(landing(r), "https://cps-tst.outsystemsenterprise.com/CaseReview/LandingPage?CMSCaseId=2&URN=1", "OS landing")
  })

  await test("full cps.gov.uk host is used as-is (lower-cased)", () => {
    const r = run("/case-review-redirect/OApps-QA-NotProd.int.cps.gov.uk/test")
    assertEqual(r.returnCode, 302, "302")
    assertIncludes(landing(r), "https://oapps-qa-notprod.int.cps.gov.uk/CaseReview/LandingPage", "full host, lower-cased")
  })

  await test("full outsystemsenterprise.com host is accepted", () => {
    const r = run("/case-review-redirect/cpslon-tst.outsystemsenterprise.com/test")
    assertEqual(r.returnCode, 302, "302")
    assertIncludes(landing(r), "https://cpslon-tst.outsystemsenterprise.com/CaseReview/LandingPage", "full OS host")
  })

  await test("a full host outside our estate is refused (no open redirect)", () => {
    for (const bad of ["evil.com", "cps.gov.uk.evil.com", "evilcps.gov.uk"]) {
      const r = run(`/case-review-redirect/${bad}/test`)
      assertEqual(r.returnCode, 400, `${bad} -> 400`)
      assertIncludes(r.returnBody, "unexpected OS host", `${bad} explained`)
    }
  })

  await test("missing envFolder -> 400 naming the {osHost} form", () => {
    const r = run("/case-review-redirect/cps-tst")
    assertEqual(r.returnCode, 400, "400")
    assertIncludes(r.returnBody, "{osHost}/{envFolder}", "usage message")
  })
}

async function corsOrigins(gloco) {
  console.log("\nreadCorsOrigin — allow-list additions:")

  await test("cpslon-tst OutSystems origin is allowed (FCT2-22132)", () => {
    const origin = "https://cpslon-tst.outsystemsenterprise.com"
    assertEqual(gloco.readCorsOrigin(createMockRequest({ headersIn: { Origin: origin } })), origin, "echoed back")
  })
}

async function main() {
  const gloco = await loadNjs("features/global-components/global-components.js")
  await domainFromCookie(gloco)
  await caseReviewRedirect(gloco)
  await corsOrigins(gloco)
  process.exit(summarise("global-components.js (unit)"))
}

main()
