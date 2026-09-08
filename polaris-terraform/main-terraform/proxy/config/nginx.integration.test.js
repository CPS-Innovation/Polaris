#!/usr/bin/env node
/**
 * nginx.conf — the core config.  CHARACTERISATION.
 *
 * Sits beside the file it tests, exactly like each feature's suite sits beside its
 * own .conf. Scope is strictly what nginx.conf ITSELF declares:
 *
 *   - the config renders (envsubst) and the server block boots
 *   - the two inline locations: `= /` (health) and `= /robots933456.txt` (Azure probe)
 *   - server-level cross-cutting concerns every location inherits:
 *       nginx.conf  security headers (add_header ... always)
 *                   proxy_hide_header Server / server_tokens off
 *                   $ieaction derivation (IE / Edge negotiation)
 *
 * Anything about a feature's routes belongs in that feature's own suite at
 * config/features/<name>/<name>.integration.test.js.
 *
 * It deliberately does NOT probe "is njs loaded" per module: a broken js_import
 * fails nginx STARTUP, which run-tests.sh's wait_for_proxy already catches before
 * any test runs — and each feature suite exercises its own njs directly.
 *
 * FEATURE DEPENDENCIES — deliberate, and unavoidable. Server-level behaviour is
 * only OBSERVABLE through a location, and nginx.conf exposes just `= /` and
 * `= /robots933456.txt` — both of which declare their own add_header and therefore
 * DROP the inherited security set. So probing needs routes owned by features
 * (named at each constant below). If one of those features is deleted, repoint the
 * probe: the feature is incidental, the server-level behaviour is what is tested.
 */
const { test, assert, assertEqual, assertIncludes, get, summarise } = require("../tests/integration/test-utils")

const IE_UA = "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko"
const EDGE_UA = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 Chrome/120 Safari/537.36"

// FEATURE DEPENDENCY: /api/ is owned by cwa-materials (feature 4). Needed because
// it declares sub_filter/proxy_set_header but NO add_header, so it inherits the
// server set — the only way to observe the inheriting case. Any add_header-free
// location will do.
const INHERITING_PATH = "/api/cases/1"

// FEATURE DEPENDENCY: /cin2 is owned by cms-proxy (feature 5). The $ieaction gate
// is declared in nginx.conf, but can only be exercised through a location that
// acts on it. Any $ieaction-gated location will do.
const IEACTION_PATH = "/cin2"

async function boots() {
  console.log("\nConfig renders and the proxy boots:")

  // If envsubst missed a variable the body/headers would still contain "${...}".
  await test("no unsubstituted ${VAR} placeholders leak into a response", async () => {
    const res = await get("/")
    const body = await res.text()
    assert(!body.includes("${"), `Body should have no un-rendered placeholders, got: ${body}`)
  })

  // The one canary that must reach through a feature: it distinguishes "the docker
  // stack / upstream wiring is broken" from "the config is broken", which no
  // feature suite can tell you on its own.
  // FEATURE DEPENDENCY: /api/ is owned by cwa-materials. If that feature is ever
  // removed, repoint this at any other proxied location — it is the proxying that
  // is under test here, not the route.
  await test("proxy reaches the mock upstream (gateway API echo)", async () => {
    const res = await get("/api/some-endpoint")
    assertEqual(res.status, 200, "Should proxy to the mock")
    assertEqual(res.headers.get("x-mock-echo"), "1", "Should have hit the mock echo handler")
  })
}

async function healthLocations() {
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
    //
    // POSITIVE CONTROL FIRST: this is a negative assertion, so on its own it would
    // still pass if the catch-all it is guarding against had simply been deleted —
    // silently testing nothing. Prove the catch-all is live before trusting the
    // negative. (If feature 5 is ever removed, this control fails loudly and this
    // test should be retired rather than left as a vacuous green.)
    const control = await get("/a-path-no-other-location-claims")
    assertEqual(
      control.headers.get("x-mock-echo"),
      "1",
      "control: `location /` must be live and proxying, or the check below is vacuous"
    )

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
  // inherited set. nginx.conf acknowledges exactly this ("inherited by location
  // blocks that do not define their own add_header directives").
  //
  // Low impact here (a health string and a probe), but the same rule silently
  // strips the security headers from EVERY location that sets any header of its
  // own — see the fuller picture below. Recorded so the refactor cannot change it
  // unnoticed.
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
}

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
  // (X-InternetExplorerMode / Set-Cookie).
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
  console.log("\n$ieaction — IE / Edge negotiation:")

  // $ieaction = "<ie|nonie>+<configurable|nonconfigurable>+", built from the
  // User-Agent (Trident => ie) and X-InternetExplorerModeConfigurable: 1.
  // /cin2 is the clearest probe: it branches on all three outcomes.

  await test("nonie + nonconfigurable -> 402 (IE mode required, cannot ask for it)", async () => {
    const res = await get(IEACTION_PATH, { headers: { "User-Agent": EDGE_UA } })
    assertEqual(res.status, 402, "Should refuse")
  })

  await test("nonie + configurable -> 302 asking the client to switch to IE mode", async () => {
    const res = await get(IEACTION_PATH, {
      headers: { "User-Agent": EDGE_UA, "X-InternetExplorerModeConfigurable": "1" },
    })
    assertEqual(res.status, 302, "Should ask to switch")
    assertEqual(res.headers.get("x-internetexplorermode"), "1", "Should request IE mode")
  })

  await test("ie (Trident) -> proceeds", async () => {
    const res = await get(IEACTION_PATH, { headers: { "User-Agent": IE_UA } })
    assertEqual(res.status, 302, "Should proceed to the CMS redirect")
    assertIncludes(res.headers.get("location"), "/CMS", "Should land on /CMS")
  })

  await test("the gate is driven by BOTH the UA and the configurable header", async () => {
    // Same UA, different configurable header => different branch.
    const a = await get(IEACTION_PATH, { headers: { "User-Agent": EDGE_UA } })
    const b = await get(IEACTION_PATH, {
      headers: { "User-Agent": EDGE_UA, "X-InternetExplorerModeConfigurable": "1" },
    })
    assertEqual(a.status, 402, "no header -> 402")
    assertEqual(b.status, 302, "with header -> 302")
  })
}

async function main() {
  await boots()
  await healthLocations()
  await securityHeaders()
  await ieNegotiation()
  process.exit(summarise("nginx core (nginx.conf)"))
}

main()
