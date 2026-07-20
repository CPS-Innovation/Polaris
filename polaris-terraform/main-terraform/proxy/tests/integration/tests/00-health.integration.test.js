#!/usr/bin/env node
/**
 * Feature 0 💓 — health check.  CHARACTERISATION.
 *
 * Locations covered (docs/PROXY.md §4):
 *   nginx.conf:122  location = /
 *   nginx.conf:152  location = /robots933456.txt
 *
 * The cross-cutting security headers live in zz-cross-cutting.integration.test.js.
 */
const { test, assert, assertEqual, assertIncludes, get, summarise } = require("../test-utils")

async function main() {
  console.log("\nlocation = / (health):")

  await test("returns 200 with the online string", async () => {
    const res = await get("/")
    assertEqual(res.status, 200, "Should 200")
    assertIncludes(await res.text(), "Polaris Proxy is online", "health string")
  })

  await test("is text/plain", async () => {
    const res = await get("/")
    assertIncludes(res.headers.get("content-type"), "text/plain", "Content-Type")
  })

  await test("exact match — / does not fall through to the CMS Modern root proxy", async () => {
    // location = / (exact) must beat location / (prefix, feature 5). If this
    // regresses, / would proxy to CMS instead of answering the health probe.
    const res = await get("/")
    assertEqual(res.headers.get("x-mock-echo"), null, "Should NOT have hit the upstream")
  })

  console.log("\nlocation = /robots933456.txt (Azure platform probe):")

  await test("returns 200 'here'", async () => {
    const res = await get("/robots933456.txt")
    assertEqual(res.status, 200, "Should 200")
    assertIncludes(await res.text(), "here", "probe body")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation, not an endorsement).
  //
  // The server-level security headers (X-Frame-Options, HSTS, Cache-Control:
  // no-store, ...) are NOT present on these two endpoints.
  //
  // That is nginx's add_header inheritance rule: a location inherits add_header
  // from its parent ONLY IF it declares no add_header of its own. Both health
  // locations declare `add_header Content-Type ...`, which drops the whole
  // inherited set. nginx.conf:62 acknowledges exactly this ("inherited by
  // location blocks that do not define their own add_header directives").
  //
  // Low impact here (a health string and a probe), but the same rule silently
  // strips the security headers from EVERY location that sets any header of its
  // own — see zz-cross-cutting for the fuller picture. Recorded so the refactor
  // cannot change it unnoticed.
  // ---------------------------------------------------------------------------
  await test("QUIRK: = / does not get the inherited security headers", async () => {
    const res = await get("/")
    assertEqual(res.headers.get("x-frame-options"), null, "add_header in this block drops inherited ones")
    assertEqual(res.headers.get("strict-transport-security"), null, "HSTS also dropped")
  })

  await test("QUIRK: = /robots933456.txt does not get them either", async () => {
    const res = await get("/robots933456.txt")
    assertEqual(res.headers.get("x-frame-options"), null, "same add_header inheritance rule")
  })

  process.exit(summarise("Health (feature 0)"))
}

main()
