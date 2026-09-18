/**
 * Minimal harness for unit-testing the njs modules.
 *
 * Two jobs:
 *
 *  1. LOAD the njs module. The feature modules are plain ESM (`export default
 *     {...}`) living in config/ and config/features/, directories with no
 *     package.json — so node would treat a bare `.js` as CommonJS and refuse the
 *     export syntax. They also import the shared primitive from a subdir
 *     (`import common from "./common/cms-detection.js"`), so a lone temp copy
 *     wouldn't resolve that. Rather than pull in a bundler (the sibling
 *     global-components harness uses esbuild only because it authors njs in
 *     TypeScript), we stage the .js sources into a temp dir that MIRRORS the real
 *     config/ layout (features/, features/common/, ...) with a package.json marking
 *     it ESM, so every relative import (./x, ../x, ./common/x) resolves exactly as
 *     in production, then import() the target. Zero dependencies, real files.
 *
 *  2. FAKE the njs `r` request object — the surface these handlers touch.
 *
 * njs-specific globals used by our modules:
 *   - `qs` (querystring)  auth-handover.js imports it; node has it built in.
 *   - process.env         the modules read every CMS app setting this way
 *                         (AUTH_HANDOVER_WHITELIST, DEFAULT_UPSTREAM_CMS_DOMAIN_NAME,
 *                         and common.cms-detection's <ENV>_UPSTREAM_CMS_* lookups).
 *   - r.variables         carries only the per-request built-in $host.
 */
const fs = require("fs")
const path = require("path")
const url = require("url")

// proxy/tests/unit -> proxy/config (the "next" config we are refactoring; the
// live monolith in main-terraform/ is verified by the integration suite).
const CONFIG_DIR = path.resolve(__dirname, "..", "..", "config")
const TMP_DIR = path.join(__dirname, ".tmp")

/**
 * Load an njs module by path relative to CONFIG_DIR, e.g.
 * loadNjs("features/common/cms-detection.js") or loadNjs("features/cms-proxy.js").
 *
 * Recursively stages every .js under config/ into a temp dir, preserving the
 * directory structure (features/, features/common/, ...), alongside a
 * package.json marking it ESM, so every relative import (./x, ../x,
 * ./common/x) resolves exactly as in production. The target is imported with a
 * cache-bust query so repeated loads in one process pick up current files.
 */
async function loadNjs(filename) {
  const src = path.join(CONFIG_DIR, filename)
  if (!fs.existsSync(src)) throw new Error(`njs source not found: ${src}`)
  fs.mkdirSync(TMP_DIR, { recursive: true })
  fs.writeFileSync(path.join(TMP_DIR, "package.json"), '{ "type": "module" }')
  const stage = (rel) => {
    for (const entry of fs.readdirSync(path.join(CONFIG_DIR, rel), { withFileTypes: true })) {
      const childRel = path.join(rel, entry.name)
      // fixtures/ holds canned upstream bodies for the integration mock — test
      // data, never modules. Skipped here for the same reason it is excluded from
      // the deploy fileset (it contains .js files).
      if (entry.isDirectory() && entry.name !== "fixtures") {
        fs.mkdirSync(path.join(TMP_DIR, childRel), { recursive: true })
        stage(childRel)
      } else if (entry.name.endsWith(".js") && !entry.name.endsWith(".test.js")) {
        // .test.js files now sit beside the modules they test (one folder per
        // feature); they are never staged — and never deployed (see the
        // .test.js exclusion in app-service-proxy.tf, guarded by deploy-safety).
        fs.copyFileSync(path.join(CONFIG_DIR, childRel), path.join(TMP_DIR, childRel))
      }
    }
  }
  stage(".")
  const dest = path.join(TMP_DIR, filename)
  // Cache-bust so repeated loads in one process pick up the current file.
  const mod = await import(`${url.pathToFileURL(dest).href}?t=${Date.now()}`)
  return mod.default
}

/**
 * A stand-in for njs's `r`. Captures what the handler did:
 *   r.return(code, body)      -> r.returnCode / r.returnBody
 *   r.sendBuffer(data, flags) -> r.sentBuffer / r.sentFlags   (js_body_filter)
 */
function createMockRequest(options = {}) {
  return {
    method: options.method || "GET",
    uri: options.uri || "/",
    args: options.args || {},
    headersIn: options.headersIn || {},
    headersOut: options.headersOut || {},
    variables: options.variables || {},
    status: options.status !== undefined ? options.status : 200,
    returnCode: null,
    returnBody: null,
    sentBuffer: null,
    sentFlags: null,
    return(code, body) {
      this.returnCode = code
      this.returnBody = body
    },
    sendBuffer(data, flags) {
      this.sentBuffer = data
      this.sentFlags = flags
    },
  }
}

/**
 * The CMS app settings as common.cms-detection consumes them: straight from process.env
 * (there is no js_var bridge). Keys are the env var names. Mirrors
 * docker/cmsproxy.mock.env so unit and integration agree. `host` is NOT here —
 * it's the per-request nginx built-in $host, set via createMockRequest variables.
 */
function cmsEnvObject(overrides = {}) {
  const env = {
    ENDPOINT_HTTP_PROTOCOL: "http",
    WEBSITE_HOSTNAME: "proxy.example.org",
  }
  const envs = {
    DEFAULT: { ip: "10.0.0.1", modernIp: "10.0.0.2", domain: "cms.cps.gov.uk", services: "cms-services.cps.gov.uk", modern: "cmsmodern.cps.gov.uk" },
    CIN2: { ip: "10.0.2.1", modernIp: "10.0.2.2", domain: "cin2.cps.gov.uk", services: "not-used-in-cin2.cps.gov.uk", modern: "cmsmodcin2.cps.gov.uk" },
    CIN4: { ip: "10.0.4.1", modernIp: "10.0.4.2", domain: "cin4.cps.gov.uk", services: "not-used-in-cin4.cps.gov.uk", modern: "cmsmodstage.cps.gov.uk" },
    CIN5: { ip: "10.0.5.1", modernIp: "10.0.5.2", domain: "cin5.cps.gov.uk", services: "not-used-in-cin5.cps.gov.uk", modern: "cmsmodcin5.cps.gov.uk" },
  }
  for (const [E, v] of Object.entries(envs)) {
    env[`${E}_UPSTREAM_CMS_IP_CORSHAM`] = v.ip
    env[`${E}_UPSTREAM_CMS_MODERN_IP_CORSHAM`] = v.modernIp
    env[`${E}_UPSTREAM_CMS_IP_FARNBOROUGH`] = `${v.ip}-fb`
    env[`${E}_UPSTREAM_CMS_MODERN_IP_FARNBOROUGH`] = `${v.modernIp}-fb`
    env[`${E}_UPSTREAM_CMS_DOMAIN_NAME`] = v.domain
    env[`${E}_UPSTREAM_CMS_SERVICES_DOMAIN_NAME`] = v.services
    env[`${E}_UPSTREAM_CMS_MODERN_DOMAIN_NAME`] = v.modern
  }
  return { ...env, ...overrides }
}

/** Set process.env from an object; returns a restore() that undoes it. */
function applyEnv(obj) {
  const saved = {}
  for (const [k, v] of Object.entries(obj)) {
    saved[k] = process.env[k]
    process.env[k] = v
  }
  return () => {
    for (const [k, v] of Object.entries(saved)) {
      if (v === undefined) delete process.env[k]
      else process.env[k] = v
    }
  }
}

module.exports = { loadNjs, createMockRequest, cmsEnvObject, applyEnv, CONFIG_DIR }
