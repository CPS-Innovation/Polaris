/** Shared assertions for the njs unit tests (same shape as the integration ones). */

const state = { passed: 0, failed: 0, results: [] }

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

function assertDeepEqual(actual, expected, message) {
  const a = JSON.stringify(actual)
  const e = JSON.stringify(expected)
  if (a !== e) throw new Error(`${message}\n  Expected: ${e}\n  Actual:   ${a}`)
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
    throw new Error(`${message}\n  Expected NOT to include: ${JSON.stringify(needle)}`)
  }
}

function assertThrows(fn, message) {
  try {
    fn()
  } catch (e) {
    return e
  }
  throw new Error(message)
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
  assert,
  assertEqual,
  assertDeepEqual,
  assertIncludes,
  assertNotIncludes,
  assertThrows,
  test,
  summarise,
}
