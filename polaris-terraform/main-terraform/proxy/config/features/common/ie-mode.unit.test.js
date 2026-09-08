#!/usr/bin/env node
/**
 * Unit tests for features/common/ie-mode.js.  CHARACTERISATION.
 *
 * ie-mode.js is the shared IE/Edge-negotiation PRIMITIVE. Its single export,
 * ieAction(r), derives the $ieaction string that the per-location gates compare
 * against — encapsulating the server-preamble if-ladder that used to live in
 * nginx.conf. This pins that derivation to the exact strings the ladder produced:
 * "{ie|nonie}+{configurable|nonconfigurable}+".
 *
 * Loaded from the REAL config/features/common/ie-mode.js — see njs-harness.js.
 */
const { test, assertEqual, summarise } = require("../../../tests/unit/test-utils")
const { loadNjs, createMockRequest, applyEnv } = require("../../../tests/unit/njs-harness")

const IE_UA = "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko"
const EDGE_UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Edg/120"

const req = (ua, configurable) =>
  createMockRequest({
    headersIn: {
      ...(ua !== undefined ? { "User-Agent": ua } : {}),
      ...(configurable !== undefined
        ? { "X-InternetExplorerModeConfigurable": configurable }
        : {}),
    },
  })

async function ieaction(ie) {
  console.log("\nieAction — headers -> $ieaction string:")

  // UA (Trident => ie) × configurable header ("1" => configurable). All four cells.
  const cases = [
    [IE_UA, "1", "ie+configurable+", "Trident + configurable"],
    [IE_UA, undefined, "ie+nonconfigurable+", "Trident, header absent"],
    [EDGE_UA, "1", "nonie+configurable+", "non-Trident + configurable"],
    [EDGE_UA, undefined, "nonie+nonconfigurable+", "non-Trident, header absent"],
  ]
  for (const [ua, conf, expected, label] of cases) {
    await test(`${label} -> ${expected}`, () => {
      assertEqual(ie.ieAction(req(ua, conf)), expected, label)
    })
  }

  await test("both headers absent -> nonie+nonconfigurable+", () => {
    assertEqual(ie.ieAction(req(undefined, undefined)), "nonie+nonconfigurable+")
  })

  await test("configurable is EXACT '1' — '0' and other values are nonconfigurable", () => {
    assertEqual(ie.ieAction(req(EDGE_UA, "0")), "nonie+nonconfigurable+", "'0'")
    assertEqual(ie.ieAction(req(EDGE_UA, "true")), "nonie+nonconfigurable+", "'true'")
    assertEqual(ie.ieAction(req(EDGE_UA, "1")), "nonie+configurable+", "'1'")
  })

  await test("Trident match is case-insensitive (mirrors nginx ~*)", () => {
    assertEqual(ie.ieAction(req("x trident/7.0 y", undefined)), "ie+nonconfigurable+")
  })
}

async function coerceTests(ie) {
  console.log("\ncoerce — enforce browser mode for a js_content endpoint:")
  const restore = applyEnv({ WEBSITE_SCHEME: "https" })
  const cr = (ua, conf, uri) =>
    createMockRequest({
      headersIn: {
        ...(ua !== undefined ? { "User-Agent": ua } : {}),
        ...(conf !== undefined ? { "X-InternetExplorerModeConfigurable": conf } : {}),
      },
      variables: { host: "polaris.example", request_uri: uri || "/x" },
    })

  // want "ie", reject false  (== /polaris)
  await test("want ie: nonie+configurable -> 302 self + X-InternetExplorerMode:1 (handled)", () => {
    const r = cr(EDGE_UA, "1", "/polaris?x=1")
    assertEqual(ie.coerce(r, "ie", false), true, "handled")
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.headersOut["X-InternetExplorerMode"], "1", "asks IE")
    assertEqual(r.returnBody, "https://polaris.example/polaris?x=1", "self-redirect")
  })
  await test("want ie, no reject: nonie+nonconfigurable -> proceed (false, no response)", () => {
    const r = cr(EDGE_UA, undefined, "/polaris")
    assertEqual(ie.coerce(r, "ie", false), false, "proceed")
    assertEqual(r.returnCode, null, "emits nothing")
  })
  await test("want ie: already IE -> proceed", () => {
    assertEqual(ie.coerce(cr(IE_UA, undefined, "/polaris"), "ie", false), false)
  })

  // want "edge", reject true  (== /init)
  await test("want edge: ie+nonconfigurable -> 402 (handled)", () => {
    const r = cr(IE_UA, undefined, "/init")
    assertEqual(ie.coerce(r, "edge", true), true, "handled")
    assertEqual(r.returnCode, 402, "402")
  })
  await test("want edge: ie+configurable -> 302 self + X-InternetExplorerMode:0 (handled)", () => {
    const r = cr(IE_UA, "1", "/init")
    assertEqual(ie.coerce(r, "edge", true), true, "handled")
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.headersOut["X-InternetExplorerMode"], "0", "asks Edge")
  })
  await test("want edge: already non-IE -> proceed", () => {
    assertEqual(ie.coerce(cr(EDGE_UA, undefined, "/init"), "edge", true), false)
  })

  restore()
}

async function main() {
  const ie = await loadNjs("features/common/ie-mode.js")
  await ieaction(ie)
  await coerceTests(ie)
  process.exit(summarise("ie-mode.js (unit)"))
}

main()
