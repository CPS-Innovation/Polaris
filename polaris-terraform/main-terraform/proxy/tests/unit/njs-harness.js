/**
 * Minimal harness for unit-testing the njs modules.
 *
 * Two jobs:
 *
 *  1. LOAD the njs module. nginx.js / cmsenv.js are plain ESM (`export default
 *     {...}`) living in main-terraform/, a directory with no package.json — so
 *     node would treat a bare `.js` as CommonJS and refuse the export syntax.
 *     Rather than pull in a bundler (the sibling global-components harness uses
 *     esbuild only because it authors njs in TypeScript), we copy the source to a
 *     temp `.mjs` and import() it. Zero dependencies, and it imports the REAL
 *     file — no copy of the logic can drift.
 *
 *  2. FAKE the njs `r` request object — the surface these handlers touch.
 *
 * njs-specific globals used by our modules:
 *   - `qs` (querystring)  nginx.js imports it; node has it built in.
 *   - process.env         nginx.js reads AUTH_HANDOVER_WHITELIST and
 *                         DEFAULT_UPSTREAM_CMS_DOMAIN_NAME this way.
 *   - r.variables         cmsenv.js reads the js_var-bridged CMS settings.
 */
const fs = require("fs")
const path = require("path")
const url = require("url")

// proxy/tests/unit -> main-terraform
const CONFIG_DIR = path.resolve(__dirname, "..", "..", "..")
const TMP_DIR = path.join(__dirname, ".tmp")

/** Load an njs module by filename, e.g. loadNjs("cmsenv.js"). */
async function loadNjs(filename) {
  const src = path.join(CONFIG_DIR, filename)
  if (!fs.existsSync(src)) throw new Error(`njs source not found: ${src}`)
  fs.mkdirSync(TMP_DIR, { recursive: true })
  const dest = path.join(TMP_DIR, filename.replace(/\.js$/, ".mjs"))
  fs.copyFileSync(src, dest)
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
 * The CMS settings that nginx.conf bridges into njs via `js_var $x ${VAR};`.
 * Mirrors docker/cmsproxy.mock.env so unit and integration agree. cmsenv.js
 * reads them as r.variables[<env> + "UpstreamCms..."], where <env> is one of
 * default | cin2 | cin4 | cin5.
 */
function cmsVariables(overrides = {}) {
  const vars = {
    endpointHttpProtocol: "http",
    websiteHostname: "proxy.example.org",
    host: "proxy.example.org",
  }
  const envs = {
    default: { ip: "10.0.0.1", modernIp: "10.0.0.2", domain: "cms.cps.gov.uk", services: "cms-services.cps.gov.uk", modern: "cmsmodern.cps.gov.uk" },
    cin2: { ip: "10.0.2.1", modernIp: "10.0.2.2", domain: "cin2.cps.gov.uk", services: "not-used-in-cin2.cps.gov.uk", modern: "cmsmodcin2.cps.gov.uk" },
    cin4: { ip: "10.0.4.1", modernIp: "10.0.4.2", domain: "cin4.cps.gov.uk", services: "not-used-in-cin4.cps.gov.uk", modern: "cmsmodstage.cps.gov.uk" },
    cin5: { ip: "10.0.5.1", modernIp: "10.0.5.2", domain: "cin5.cps.gov.uk", services: "not-used-in-cin5.cps.gov.uk", modern: "cmsmodcin5.cps.gov.uk" },
  }
  for (const [env, v] of Object.entries(envs)) {
    vars[`${env}UpstreamCmsIpCorsham`] = v.ip
    vars[`${env}UpstreamCmsModernIpCorsham`] = v.modernIp
    vars[`${env}UpstreamCmsIpFarnborough`] = `${v.ip}-fb`
    vars[`${env}UpstreamCmsModernIpFarnborough`] = `${v.modernIp}-fb`
    vars[`${env}UpstreamCmsDomainName`] = v.domain
    vars[`${env}UpstreamCmsServicesDomainName`] = v.services
    vars[`${env}UpstreamCmsModernDomainName`] = v.modern
  }
  return { ...vars, ...overrides }
}

module.exports = { loadNjs, createMockRequest, cmsVariables, CONFIG_DIR }
