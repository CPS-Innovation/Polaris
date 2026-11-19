/**
 * Mock upstream — impersonates EVERY backend the cmsproxy dials.
 *
 *   :3000 http    CMS (classic/modern/services), DDEI, gateway API, SPA,
 *                 Materials UI, WM-MDS
 *   :3443 https   blob / SAS storage, global-components blob
 *
 * Two behaviours:
 *
 *  1. FIXTURE routes — DISCOVERED from the feature folders at startup. A feature
 *     that needs canned upstream bodies ships them itself:
 *
 *       config/features/<name>/fixtures/routes.json   [{ re, file, type }, ...]
 *       config/features/<name>/fixtures/<file>
 *
 *     so this mock holds NO per-feature knowledge and a feature stays complete in
 *     one folder. This is also the FIXTURE SEAM: drop a real captured CMS response
 *     in over the synthesised one and the tests still hold, because they assert on
 *     the *transformation*, not the whole body.
 *
 *     Nothing under a fixtures/ directory is ever deployed or staged into nginx —
 *     see the exclusions in app-service-proxy.tf and stage-config.sh.
 *
 *  2. ECHO (default) — everything else returns the request it received as JSON
 *     (method/url/headers). That is how tests assert what nginx forwarded — most
 *     importantly the `Host` header, which reveals which CMS environment
 *     cmsenv.js selected (cms/cin2/cin4/cin5) and which upstream was chosen.
 */
const http = require("http")
const https = require("https")
const fs = require("fs")
const path = require("path")

const PORT = 3000
const HTTPS_PORT = 3443
// 443 as well, because nginx.conf's `/v2/` block proxy_passes to a HARDCODED
// https://uksouth-1.in.applicationinsights.azure.com/v2/ (no env var). We give
// this container a docker network alias for that hostname, so the request lands
// here — but only if we're listening on the default https port.
const HTTPS_DEFAULT_PORT = 443
// The features tree, bind-mounted read-only (same source both configs use).
const FEATURES_DIR = process.env.FEATURES_DIR || "/config-features"

/**
 * Build the ordered fixture table by scanning every feature for a
 * fixtures/routes.json. Features are visited in sorted order for determinism;
 * within a feature the manifest's own order is preserved (first match wins,
 * mirroring nginx's regex-location ordering). A feature with no fixtures/ dir
 * simply contributes nothing.
 */
function discoverFixtureRoutes() {
  const routes = []
  if (!fs.existsSync(FEATURES_DIR)) return routes
  for (const feature of fs.readdirSync(FEATURES_DIR).sort()) {
    const manifest = path.join(FEATURES_DIR, feature, "fixtures", "routes.json")
    if (!fs.existsSync(manifest)) continue
    let entries
    try {
      entries = JSON.parse(fs.readFileSync(manifest, "utf8"))
    } catch (e) {
      console.error(`[mock] BAD fixture manifest ${manifest}: ${e.message}`)
      continue
    }
    for (const r of entries) {
      routes.push({
        re: new RegExp(r.re, "i"),
        type: r.type,
        file: r.file,
        path: path.join(FEATURES_DIR, feature, "fixtures", r.file),
        feature,
      })
    }
    console.log(`[mock] fixtures: ${feature} -> ${entries.length} route(s)`)
  }
  return routes
}

const fixtureRoutes = discoverFixtureRoutes()

function handler(req, res) {
  const url = req.url || "/"
  // eslint-disable-next-line no-console
  console.log(`[mock] ${req.method} ${url}  host=${req.headers.host}`)

  if (url.startsWith("/__mock/health")) {
    res.writeHead(200, { "Content-Type": "text/plain" })
    return res.end("ok")
  }

  for (const route of fixtureRoutes) {
    if (route.re.test(url)) {
      const file = route.path
      if (fs.existsSync(file)) {
        res.writeHead(200, {
          "Content-Type": route.type,
          // Lets a test prove it hit a fixture rather than the echo default.
          "X-Mock-Fixture": route.file,
          // Echo the Host back in a header too, so body-fixture routes can still
          // assert which CMS environment was selected.
          "X-Mock-Host": req.headers.host || "",
        })
        return res.end(fs.readFileSync(file, "utf8"))
      }
    }
  }

  // Default: echo what we received.
  res.writeHead(200, {
    "Content-Type": "application/json",
    "X-Mock-Echo": "1",
    "X-Mock-Host": req.headers.host || "",
  })
  res.end(
    JSON.stringify(
      { method: req.method, url, headers: req.headers },
      null,
      2
    )
  )
}

http.createServer(handler).listen(PORT, () => {
  console.log(`[mock] http  listening on ${PORT}`)
})

const tls = {
  key: fs.readFileSync(path.join(__dirname, "key.pem")),
  cert: fs.readFileSync(path.join(__dirname, "cert.pem")),
}

https.createServer(tls, handler).listen(HTTPS_PORT, () => {
  console.log(`[mock] https listening on ${HTTPS_PORT}`)
})

https.createServer(tls, handler).listen(HTTPS_DEFAULT_PORT, () => {
  console.log(`[mock] https listening on ${HTTPS_DEFAULT_PORT} (app-insights alias)`)
})
