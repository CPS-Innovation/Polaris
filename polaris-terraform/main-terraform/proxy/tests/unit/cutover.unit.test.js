#!/usr/bin/env node
/**
 * Cutover — TRIPWIRE, not a behaviour test.
 *
 * var.proxy_next_config_enabled switches the proxy onto the next config by setting the App
 * Service Startup Command to `sh /etc/nginx/templates/start-next.sh` on BOTH slots. Four places
 * must agree, and nothing else ties them together:
 *   1. the Startup Command string (app-service-proxy.tf local)       -> the script's mounted path
 *   2. the start-next.sh blob (terraform)                            -> the same file name
 *   3. both slots use that local                                     -> a swap never flips config
 *   4. the integration stack boots through the same script           -> --next proves it boots
 * The Startup Command field also splits on whitespace and strips quotes, so the command must
 * be exactly `sh <path>` (the quoting lives in the script).
 */
const fs = require("fs")
const path = require("path")
const { test, assert, assertIncludes, summarise } = require("./test-utils")

const PROXY_DIR = path.resolve(__dirname, "..", "..")
const TF_DIR = path.join(PROXY_DIR, "..")
const SCRIPT_PATH = "/etc/nginx/templates/start-next.sh"
const read = (p) => fs.readFileSync(p, "utf8")

async function main() {
  console.log("\ncutover — the Startup Command, the script and the test stack agree:")

  const tf = read(path.join(TF_DIR, "app-service-proxy.tf"))
  const slot = read(path.join(TF_DIR, "app-service-proxy-staging.tf"))
  const compose = read(path.join(PROXY_DIR, "tests", "integration", "docker", "docker-compose.next.yml"))
  const script = read(path.join(PROXY_DIR, "config", "start-next.sh"))

  await test("the Startup Command is exactly `sh <script>` (no quotes, one space)", () => {
    const m = tf.match(/proxy_app_command_line\s*=\s*var\.proxy_next_config_enabled \? "([^"]*)" : ""/)
    assert(m, "proxy_app_command_line local not found in app-service-proxy.tf")
    assert(m[1] === "sh " + SCRIPT_PATH, "unexpected command: " + m[1])
  })

  await test("both slots set app_command_line from the one local", () => {
    for (const [name, src] of [["app-service-proxy.tf", tf], ["app-service-proxy-staging.tf", slot]])
      assert(/app_command_line\s*=\s*local\.proxy_app_command_line/.test(src), name + " doesn't use the local")
  })

  await test("terraform deploys the script under the name the command runs", () => {
    assertIncludes(tf.replace(/\s+/g, " "), 'name = "start-next.sh"', "start-next.sh blob")
  })

  await test("the script boots the next entry through the image entrypoint", () => {
    assertIncludes(script, 'exec /docker-entrypoint.sh nginx -c /etc/nginx/nginx-next.conf -g "daemon off;"', "exec line")
  })

  await test("the --next integration stack boots through the same script", () => {
    assertIncludes(compose, `command: ["sh", "${SCRIPT_PATH}"]`, "compose command")
    assertIncludes(compose, `config/start-next.sh:${SCRIPT_PATH}:ro`, "script mount")
    assertIncludes(compose, "config/nginx.conf:/etc/nginx/templates/nginx-next.conf.template:ro", "entry mount")
  })

  summarise("cutover (unit)")
}

main()
