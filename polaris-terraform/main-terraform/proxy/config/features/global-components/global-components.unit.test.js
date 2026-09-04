#!/usr/bin/env node
/**
 * Unit tests for global-components.js — only the bits this repo owns.
 *
 * global-components' full behaviour is covered by the sibling repo
 * (global-components/infra/proxy). Here we unit-test getDomainFromCookie, which
 * was moved in from cmsenv.js and drives the legacy /api/navigate-cms block. The
 * other handlers are exercised via the integration smoke test.
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

async function main() {
  const gloco = await loadNjs("features/global-components/global-components.js")
  await domainFromCookie(gloco)
  process.exit(summarise("global-components.js (unit)"))
}

main()
