#!/usr/bin/env node
/**
 * Feature X 🗑️ — locations marked redundant in docs/PROXY.md §4.
 *
 * CHARACTERISE BEFORE DELETING. §7.5 is explicit that the X paths get pinned
 * first, so that removing them is provably a no-op for everything else. When a
 * location here is actually deleted, delete its test in the SAME change.
 *
 * Locations covered:
 *   nginx.conf:735  /sas-url/    "Can be deleted" (§4)
 *
 * Note the SAS rewrite itself lives in the /api/ block (feature 4):
 *   sub_filter ${SAS_URL_DOMAIN_NAME}/ $host/sas-url/
 * i.e. the gateway's blob URLs are rewritten to point back here. Both halves
 * must go together — see 04-cwa-materials for the /api/ side.
 */
const { test, assertEqual, assertIncludes, get, summarise } = require("../test-utils")

async function main() {
  console.log("\nlocation /sas-url/ — blob SAS proxy (drop candidate):")

  await test("proxies to the blob storage domain over https, stripping /sas-url/", async () => {
    const res = await get("/sas-url/container/doc.pdf?sig=abc")
    assertEqual(res.status, 200, "Should proxy to the (mocked) blob host")
    const echo = await res.json()
    assertEqual(echo.url, "/container/doc.pdf?sig=abc", "The /sas-url/ prefix is stripped upstream")
  })

  await test("reaches the SAS_URL_DOMAIN_NAME upstream", async () => {
    const res = await get("/sas-url/container/doc.pdf")
    const echo = await res.json()
    assertIncludes(echo.headers.host, "mock-upstream", "Should hit the blob upstream")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation, not an endorsement).
  //
  // nginx.conf:736 reads:  add_header Content-Disposition: inline;
  // The stray colon is taken as part of the HEADER NAME, so nginx puts
  //     Content-Disposition:: inline
  // on the wire. HTTP splits on the FIRST colon, so clients see the header
  // Content-Disposition with the bogus value ": inline" — NOT "inline".
  // Almost certainly a typo for:  add_header Content-Disposition inline;
  //
  // So the intended "render this blob inline" behaviour does not work today.
  // Recorded so the refactor cannot change it unnoticed — fix deliberately.
  // ---------------------------------------------------------------------------
  await test("QUIRK: add_header 'Content-Disposition:' yields the value ': inline'", async () => {
    const res = await get("/sas-url/container/doc.pdf")
    assertEqual(
      res.headers.get("content-disposition"),
      ": inline",
      "Records today's malformed value (stray colon in the add_header name)"
    )
  })

  process.exit(summarise("Redundant / drop candidates (feature X)"))
}

main()
