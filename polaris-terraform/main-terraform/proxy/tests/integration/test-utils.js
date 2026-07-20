/**
 * Shared helpers for the cmsproxy integration tests.
 *
 * Deliberately dependency-free (plain node, built-in fetch) to match the
 * reference harness in the sibling repo CPS/global-components/infra/proxy.
 */

const PROXY_BASE = process.env.PROXY_BASE || "http://localhost:8080"

const state = { passed: 0, failed: 0, results: [] }

function getState() {
  return state
}

function assert(condition, message) {
  if (!condition) throw new Error(message)
}

function assertEqual(actual, expected, message) {
  if (actual !== expected) {
    throw new Error(
      `${message}\n  Expected: ${JSON.stringify(expected)}\n  Actual:   ${JSON.stringify(actual)}`
    )
  }
}

function assertIncludes(haystack, needle, message) {
  if (typeof haystack !== "string" || !haystack.includes(needle)) {
    throw new Error(
      `${message}\n  Expected to include: ${JSON.stringify(needle)}\n  Actual: ${JSON.stringify(
        typeof haystack === "string" ? haystack.slice(0, 400) : haystack
      )}`
    )
  }
}

function assertNotIncludes(haystack, needle, message) {
  if (typeof haystack === "string" && haystack.includes(needle)) {
    throw new Error(
      `${message}\n  Expected NOT to include: ${JSON.stringify(needle)}\n  Actual: ${JSON.stringify(
        haystack.slice(0, 400)
      )}`
    )
  }
}

async function test(name, fn) {
  try {
    await fn()
    state.passed++
    state.results.push({ name, status: "PASS" })
    console.log(`  ✓ ${name}`)
  } catch (err) {
    state.failed++
    state.results.push({ name, status: "FAIL", error: err.message })
    console.log(`  ✗ ${name}`)
    console.log(`    ${String(err.message).split("\n").join("\n    ")}`)
  }
}

/** GET without following redirects — the common case for this proxy. */
function get(pathOrUrl, options = {}) {
  const url = pathOrUrl.startsWith("http") ? pathOrUrl : `${PROXY_BASE}${pathOrUrl}`
  return fetch(url, { redirect: "manual", ...options })
}

/**
 * The mock upstream echoes the request it received as JSON. Use this to assert
 * what nginx forwarded — above all the `Host` header, which reveals which CMS
 * environment cmsenv.js selected.
 */
async function echoOf(response) {
  return response.json()
}

function summarise(label) {
  console.log("")
  console.log(`${label}: ${state.passed} passed, ${state.failed} failed`)
  if (state.failed > 0) {
    console.log("\nFailures:")
    for (const r of state.results.filter((x) => x.status === "FAIL")) {
      console.log(`  ✗ ${r.name}\n    ${r.error}`)
    }
  }
  return state.failed === 0 ? 0 : 1
}

module.exports = {
  PROXY_BASE,
  getState,
  assert,
  assertEqual,
  assertIncludes,
  assertNotIncludes,
  test,
  get,
  echoOf,
  summarise,
}
