#!/usr/bin/env node
/**
 * Cross-cutting behaviour (docs/PROXY.md §7.7's "cross-cutting" row).
 * CHARACTERISATION.
 *
 * Not a feature — these are server-level concerns declared once in nginx.conf
 * and relied on by every location. When locations are sliced into per-feature
 * includes (§6), all of this MUST stay in the parent server block, and these
 * tests are what proves it survived.
 *
 *   nginx.conf:65-70  security headers (add_header ... always)
 *   nginx.conf:64     proxy_hide_header Server
 *   nginx.conf:31     server_tokens off
 *   nginx.conf:72-85  $ieaction derivation (IE / Edge negotiation)
 */
const { test, assert, assertEqual, assertIncludes, get, summarise } = require("../test-utils")

const IE_UA = "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko"
const EDGE_UA = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 Chrome/120 Safari/537.36"

// /api/ (feature 4) is a good probe for the inherited headers: it declares
// sub_filter/proxy_set_header but NO add_header, so it inherits the server set.
const INHERITING_PATH = "/api/cases/1"

async function securityHeaders() {
  console.log("\nServer-level security headers (on a location that inherits them):")

  const expected = [
    ["x-content-type-options", "nosniff"],
    ["x-frame-options", "DENY"],
    ["x-permitted-cross-domain-policies", "none"],
    ["strict-transport-security", "max-age=31536000; includeSubDomains"],
    ["cache-control", "no-store"],
    ["pragma", "no-cache"],
  ]

  for (const [name, value] of expected) {
    await test(`${name}: ${value}`, async () => {
      const res = await get(INHERITING_PATH)
      assertEqual(res.headers.get(name), value, `${name} should be inherited here`)
    })
  }

  await test("upstream Server header is hidden on proxied responses", async () => {
    const res = await get(INHERITING_PATH)
    // proxy_hide_header Server strips the UPSTREAM's Server header. nginx still
    // adds its own (bare "nginx", because server_tokens off hides the version).
    assertEqual(res.headers.get("server"), "nginx", "server_tokens off — no version leaked")
  })

  await test("no nginx version is leaked (server_tokens off)", async () => {
    const res = await get(INHERITING_PATH)
    const server = res.headers.get("server") || ""
    assert(!/\d/.test(server), `Server header should carry no version, got: ${server}`)
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation, not an endorsement) — the big one.
  //
  // nginx only inherits add_header into a location that declares NO add_header of
  // its own. So every location that sets any header for its own purposes SILENTLY
  // LOSES the whole server-level security set: X-Frame-Options, HSTS,
  // X-Content-Type-Options, Cache-Control: no-store, ...
  //
  // Affected today (non-exhaustive): = / and = /robots933456.txt (Content-Type),
  // = /api/navigate-cms and = /navigate-cms-close and /cin2../cin5
  // (X-InternetExplorerMode / Set-Cookie), /sas-url/ (Content-Disposition).
  //
  // Whether that matters depends on the endpoint, but it is certainly not
  // obvious from reading the config. Recorded here so that (a) it is visible and
  // (b) the refactor cannot change it — deliberately or accidentally.
  // ---------------------------------------------------------------------------
  await test("QUIRK: locations declaring their own add_header lose the security set", async () => {
    const withOwnHeaders = await get("/") // declares add_header Content-Type
    assertEqual(withOwnHeaders.headers.get("x-frame-options"), null, "dropped on = /")

    const inheriting = await get(INHERITING_PATH) // declares none
    assertEqual(inheriting.headers.get("x-frame-options"), "DENY", "present on /api/")
  })
}

async function ieNegotiation() {
  console.log("\n$ieaction — IE / Edge negotiation (nginx.conf:72-85):")

  // $ieaction = "<ie|nonie>+<configurable|nonconfigurable>+", built from the
  // User-Agent (Trident => ie) and X-InternetExplorerModeConfigurable: 1.
  // /cin2 is the clearest probe: it branches on all three outcomes.

  await test("nonie + nonconfigurable -> 402 (IE mode required, cannot ask for it)", async () => {
    const res = await get("/cin2", { headers: { "User-Agent": EDGE_UA } })
    assertEqual(res.status, 402, "Should refuse")
  })

  await test("nonie + configurable -> 302 asking the client to switch to IE mode", async () => {
    const res = await get("/cin2", {
      headers: { "User-Agent": EDGE_UA, "X-InternetExplorerModeConfigurable": "1" },
    })
    assertEqual(res.status, 302, "Should ask to switch")
    assertEqual(res.headers.get("x-internetexplorermode"), "1", "Should request IE mode")
  })

  await test("ie (Trident) -> proceeds", async () => {
    const res = await get("/cin2", { headers: { "User-Agent": IE_UA } })
    assertEqual(res.status, 302, "Should proceed to the CMS redirect")
    assertIncludes(res.headers.get("location"), "/CMS", "Should land on /CMS")
  })

  await test("the gate is driven by BOTH the UA and the configurable header", async () => {
    // Same UA, different configurable header => different branch.
    const a = await get("/cin2", { headers: { "User-Agent": EDGE_UA } })
    const b = await get("/cin2", {
      headers: { "User-Agent": EDGE_UA, "X-InternetExplorerModeConfigurable": "1" },
    })
    assertEqual(a.status, 402, "no header -> 402")
    assertEqual(b.status, 302, "with header -> 302")
  })
}

async function main() {
  await securityHeaders()
  await ieNegotiation()
  process.exit(summarise("Cross-cutting"))
}

main()
