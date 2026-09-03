#!/usr/bin/env node
/**
 * Deploy safety — TRIPWIRE, not a behaviour test.
 *
 * Each feature is a self-contained folder under config/features/<name>/ holding
 * <name>.conf, <name>.js AND <name>.unit.test.js. That colocation is deliberate
 * (drop in a feature complete with its tests) but it puts test files INSIDE the
 * deployment artifact: config/ is uploaded to the proxy's blob container and
 * mounted at /etc/nginx/templates on the live App Service.
 *
 * Two independent places must therefore exclude *.test.js:
 *   1. app-service-proxy.tf  — the `fileset` that becomes azurerm_storage_blob
 *   2. stage-config.sh       — the docker staging step (keeps the test stack honest)
 *
 * If either exclusion is removed while tests still live under config/, test code
 * ships to production. This file fails loudly in that case.
 */
const fs = require("fs")
const path = require("path")
const { test, assert, assertIncludes, summarise } = require("./test-utils")

const PROXY_DIR = path.resolve(__dirname, "..", "..")
const CONFIG_DIR = path.join(PROXY_DIR, "config")
const TERRAFORM = path.join(PROXY_DIR, "..", "app-service-proxy.tf")
const STAGE_SCRIPT = path.join(PROXY_DIR, "tests", "integration", "docker", "stage-config.sh")

/** Files inside the deployable config tree that must never ship, by category. */
function undeployableInConfig(dir = CONFIG_DIR, acc = { tests: [], fixtures: [] }) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name)
    const rel = path.relative(CONFIG_DIR, p)
    if (e.isDirectory()) {
      if (e.name === "fixtures") collectAll(p, acc.fixtures)
      else undeployableInConfig(p, acc)
    } else if (e.name.endsWith(".test.js")) acc.tests.push(rel)
  }
  return acc
}

function collectAll(dir, acc) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name)
    if (e.isDirectory()) collectAll(p, acc)
    else acc.push(path.relative(CONFIG_DIR, p))
  }
  return acc
}

async function main() {
  console.log("\ndeploy safety — test files must never reach the proxy:")

  const { tests, fixtures } = undeployableInConfig()
  const tf = fs.readFileSync(TERRAFORM, "utf8").replace(/\s+/g, " ")
  const sh = fs.readFileSync(STAGE_SCRIPT, "utf8").replace(/\s+/g, " ")

  await test("test files and fixtures do live inside config/ (exclusions are load-bearing)", () => {
    assert(
      tests.length > 0,
      "Expected colocated *.test.js under config/features/<name>/. If tests moved out " +
        "of config/, this tripwire is obsolete — delete it deliberately."
    )
    assert(
      fixtures.length > 0,
      "Expected fixtures under config/features/<name>/fixtures/. If fixtures moved out, " +
        "the fixtures exclusions below are obsolete — remove them deliberately."
    )
  })

  await test("terraform excludes *.test.js from the blob fileset", () => {
    assertIncludes(
      tf,
      'if !endswith(f, ".test.js")',
      `app-service-proxy.tf must filter *.test.js out of proxy_next_files, or these ` +
        `would be uploaded and mounted into /etc/nginx: ${tests.join(", ")}`
    )
  })

  await test("terraform excludes fixtures/ from the blob fileset", () => {
    // Fixtures include .js files (uainMenuBar.js, uainGeneratedScript.aspx.js) so
    // the features/**/*.js glob WOULD pick them up without this.
    assertIncludes(
      tf,
      '!strcontains(f, "/fixtures/")',
      `app-service-proxy.tf must filter fixtures/ out of proxy_next_files, or these ` +
        `would be uploaded and mounted into /etc/nginx: ${fixtures.join(", ")}`
    )
  })

  await test("the docker staging step skips *.test.js and fixtures/", () => {
    assertIncludes(sh, "! -name '*.test.js'", "stage-config.sh must not stage test files")
    assertIncludes(sh, "! -path '*/fixtures/*'", "stage-config.sh must not stage fixtures")
  })

  process.exit(summarise("deploy safety (unit)"))
}

main()
