#!/usr/bin/env node
/**
 * Unit tests for auth-handover.js (was nginx.js) (feature 2 — auth handover).  CHARACTERISATION.
 *
 * Complements 02-auth-handover.integration.test.js. Unit earns its keep here on
 * the paths integration cannot reach:
 *   - the 502 when NO handover endpoint can be resolved (integration always has
 *     DEFAULT_UPSTREAM_CMS_DOMAIN_NAME set, so that branch is unreachable there);
 *   - the Cms-Session-Hint error payload;
 *   - exact _argsShim query-string construction.
 *
 * authHandover.js reads two settings via process.env (not r.variables) — the tests set
 * and restore them around each case.
 */
const {
  test,
  assert,
  assertEqual,
  assertDeepEqual,
  assertIncludes,
  assertNotIncludes,
  summarise,
} = require("./test-utils")
const { loadNjs, createMockRequest } = require("./njs-harness")

const WHITELIST = "/auth-refresh-inbound,https://allowed.example.org/"
const DEFAULT_CMS = "cms.cps.gov.uk"

function req(args = {}, headersIn = {}) {
  return createMockRequest({
    args,
    headersIn: { "X-Forwarded-Proto": "https", Host: "proxy.example.org", ...headersIn },
  })
}

function hintOf(r) {
  const sc = r.headersOut["Set-Cookie"]
  const m = /Cms-Session-Hint=([^;]+)/.exec(sc || "")
  if (!m) throw new Error(`No Cms-Session-Hint in: ${sc}`)
  return JSON.parse(decodeURIComponent(m[1]))
}

async function withEnv(env, fn) {
  const saved = {}
  for (const [k, v] of Object.entries(env)) {
    saved[k] = process.env[k]
    if (v === undefined) delete process.env[k]
    else process.env[k] = v
  }
  try {
    return await fn()
  } finally {
    for (const [k, v] of Object.entries(saved)) {
      if (v === undefined) delete process.env[k]
      else process.env[k] = v
    }
  }
}

async function appAuthRedirect(authHandover) {
  console.log("\nappAuthRedirect (/init) — whitelist enforcement:")

  await test("whitelisted r -> 302 with the cookie appended as cc", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: WHITELIST }, () => {
      const r = req({ r: "/auth-refresh-inbound", cookie: "session=abc123" })
      authHandover.appAuthRedirect(r)
      assertEqual(r.returnCode, 302, "302")
      assertIncludes(r.returnBody, "https://proxy.example.org/auth-refresh-inbound", "absolute URL")
      assertIncludes(r.returnBody, "cc=session%3Dabc123", "cookie appended, URL-encoded")
    })
  })

  await test("uses ? or & correctly when the target already has a query", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: WHITELIST }, () => {
      const r = req({ r: "/auth-refresh-inbound?a=1", cookie: "c=1" })
      authHandover.appAuthRedirect(r)
      assertIncludes(r.returnBody, "?a=1&cc=", "Should join with & when a query exists")
    })
  })

  await test("non-whitelisted r -> 403 naming the whitelist", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: WHITELIST }, () => {
      const r = req({ r: "https://evil.example.com/x", cookie: "c=1" })
      authHandover.appAuthRedirect(r)
      assertEqual(r.returnCode, 403, "403")
      assertIncludes(r.returnBody, "evil.example.com", "Should echo the rejected target")
    })
  })

  await test("an absolute whitelisted prefix is allowed", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: WHITELIST }, () => {
      const r = req({ r: "https://allowed.example.org/callback", cookie: "c=1" })
      authHandover.appAuthRedirect(r)
      assertEqual(r.returnCode, 302, "302")
      assertIncludes(r.returnBody, "https://allowed.example.org/callback", "absolute target kept as-is")
    })
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation): the whitelist is a plain prefix match
  // (redirectUrl.startsWith(entry)), so an entry like "https://allowed.example.org/"
  // also permits look-alikes that merely START with it.
  // ---------------------------------------------------------------------------
  await test("QUIRK: whitelist is a prefix match, not an origin match", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: "https://allowed.example.org" }, () => {
      const r = req({ r: "https://allowed.example.org.evil.com/x", cookie: "c=1" })
      authHandover.appAuthRedirect(r)
      assertEqual(r.returnCode, 302, "A look-alike host starting with the entry is accepted today")
    })
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation) — SECURITY-RELEVANT. Please read.
  //
  // The check is:
  //     const whitelistedUrls = process.env.AUTH_HANDOVER_WHITELIST ?? ""
  //     whitelistedUrls.split(",").some((url) => redirectUrl.startsWith(url))
  //
  // "".split(",") is [""], and EVERY string .startsWith("") === true. So an
  // empty — or merely trailing-comma'd — whitelist does not "allow nothing", it
  // ALLOWS EVERYTHING. /init would then 302 to any attacker-supplied target
  // WITH THE CMS COOKIE ATTACHED as ?cc=... i.e. an open redirect that also
  // leaks the session cookie.
  //
  // Latent today: terraform always sets AUTH_HANDOVER_WHITELIST (docs/PROXY.md
  // §2), so the value is non-empty in every deployed environment. The exposure
  // is if it is ever unset, blanked, or given a trailing comma — none of which
  // would fail loudly. A one-line guard (drop empty entries) would close it.
  //
  // Recorded, NOT fixed — this file's job is to pin today's behaviour. Fix
  // deliberately and update these two tests in the same change.
  // ---------------------------------------------------------------------------
  await test("QUIRK: an unset whitelist allows EVERYTHING (open redirect + cc leak)", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: undefined }, () => {
      const r = req({ r: "https://evil.example.com/steal", cookie: "SESSION=secret" })
      authHandover.appAuthRedirect(r)
      assertEqual(r.returnCode, 302, "Today: allowed, because ''.split(',') === [''] and startsWith('') is true")
      assertIncludes(r.returnBody, "https://evil.example.com/steal", "redirects to the attacker target")
      assertIncludes(r.returnBody, "cc=SESSION%3Dsecret", "...with the CMS cookie attached")
    })
  })

  await test("QUIRK: a TRAILING COMMA in the whitelist also allows everything", async () => {
    // The realistic footgun: this is a plausible tfvars edit that silently
    // disables the allow-list entirely, with no error anywhere.
    await withEnv({ AUTH_HANDOVER_WHITELIST: "/auth-refresh-inbound," }, () => {
      const r = req({ r: "https://evil.example.com/x", cookie: "c=1" })
      authHandover.appAuthRedirect(r)
      assertEqual(r.returnCode, 302, "The empty entry from the trailing comma matches every target")
    })
  })

  console.log("\nappAuthRedirect — _argsShim (legacy /polaris handover):")

  await test("with no r param, synthesises r=/auth-refresh-inbound from the legacy args", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: WHITELIST }, () => {
      const r = req({ q: "SOMEQ", referer: "http://x", cookie: "c=1" })
      authHandover.appAuthRedirect(r)
      assertEqual(r.returnCode, 302, "302")
      assertIncludes(r.returnBody, "/auth-refresh-inbound?", "Synthesised target")
      assertIncludes(r.returnBody, "q=SOMEQ", "Legacy args carried into the r param")
    })
  })

  await test("the synthesised r param excludes cookie and is-proxy-session", async () => {
    await withEnv({ AUTH_HANDOVER_WHITELIST: WHITELIST }, () => {
      const r = req({ q: "Q", cookie: "SECRET=1", "is-proxy-session": "true" })
      authHandover.appAuthRedirect(r)
      const beforeCc = r.returnBody.split("cc=")[0]
      assertNotIncludes(beforeCc, "SECRET", "cookie must not be serialised into r (it comes back as cc)")
      assertNotIncludes(beforeCc, "is-proxy-session", "the synthetic flag must not leak into r")
    })
  })
}

async function sessionHint(authHandover) {
  console.log("\nsetSessionHintCookie (via /init) — the Cms-Session-Hint payload:")

  const call = (args) =>
    withEnv({ AUTH_HANDOVER_WHITELIST: WHITELIST }, () => {
      const r = req({ r: "/auth-refresh-inbound", ...args })
      authHandover.appAuthRedirect(r)
      return r
    })

  await test("cookie attributes: Path/Secure/SameSite=None/Expires", async () => {
    const r = await call({ cookie: "c=1" })
    const sc = r.headersOut["Set-Cookie"]
    for (const attr of ["Path=/", "Secure", "SameSite=None", "Expires="]) {
      assertIncludes(sc, attr, attr)
    }
  })

  await test("derives cmsDomains from [CF]-TOKEN-LBsessioncookie (preferred)", async () => {
    const r = await call({ cookie: "C-CIN3-LBsessioncookie=abc" })
    assertDeepEqual(hintOf(r).cmsDomains, ["cin3.cps.gov.uk"], "token lowercased into a domain")
  })

  await test("handles multiple LB cookies, taking the first for handoverEndpoint", async () => {
    const r = await call({ cookie: "C-CIN3-LBsessioncookie=a; F-CIN4-LBsessioncookie=b" })
    const hint = hintOf(r)
    assertDeepEqual(hint.cmsDomains, ["cin3.cps.gov.uk", "cin4.cps.gov.uk"], "both found")
    assertEqual(hint.handoverEndpoint, "https://cin3.cps.gov.uk/polaris", "first wins")
  })

  await test("falls back to the legacy _POOL cookie when no LB cookie is present", async () => {
    const r = await call({ cookie: "CPSACP-LTM-CM-WAN-CIN2-cin2.cps.gov.uk_POOL=1" })
    assertDeepEqual(hintOf(r).cmsDomains, ["cin2.cps.gov.uk"], "uppercase prefix skipped")
  })

  await test("isProxySession=true overrides the domain and points at this host", async () => {
    const r = await call({ cookie: "C-CIN3-LBsessioncookie=a", "is-proxy-session": "true" })
    const hint = hintOf(r)
    assertEqual(hint.isProxySession, true, "flag recorded")
    assertEqual(hint.handoverEndpoint, "https://proxy.example.org/polaris", "points at the proxy, not the CMS")
  })

  await test("handoverEndpoint is null when no domain can be derived", async () => {
    const r = await call({ cookie: "unrelated=1" })
    const hint = hintOf(r)
    assertDeepEqual(hint.cmsDomains, [], "none found")
    assertEqual(hint.handoverEndpoint, null, "null rather than a bogus URL")
  })

  // ---------------------------------------------------------------------------
  // QUIRK (characterisation): with no `cookie` arg at all, cookie.match() throws
  // and the catch writes an ERROR payload into the cookie — the redirect still
  // proceeds, so the failure is silent to the user.
  // ---------------------------------------------------------------------------
  await test("QUIRK: a missing cookie arg yields an {error} hint payload, not a failure", async () => {
    const r = await call({})
    const hint = hintOf(r)
    assert("error" in hint, `Expected an error payload, got ${JSON.stringify(hint)}`)
    assertEqual(r.returnCode, 302, "and the redirect still happens")
  })

  // Mirrors the integration QUIRK — pinned here too, at the source.
  await test("QUIRK: separator-less BIGipServer<pool> over-captures the domain", async () => {
    const r = await call({ cookie: "BIGipServercin2.cps.gov.uk_POOL=1" })
    assertDeepEqual(hintOf(r).cmsDomains, ["ervercin2.cps.gov.uk"], "regex starts after the 'S' of Server")
  })
}

async function polarisAuthRedirect(authHandover) {
  console.log("\npolarisAuthRedirect (/polaris) — simulated CMS handover:")

  await test("redirects to /init carrying cookie, referer and is-proxy-session", async () => {
    const r = req({ r: "/target" }, { Cookie: "UID=abc", Referer: "https://cms.example/x" })
    authHandover.polarisAuthRedirect(r)
    assertEqual(r.returnCode, 302, "302")
    assertIncludes(r.returnBody, "https://proxy.example.org/init?", "absolute /init URL")
    assertIncludes(r.returnBody, "cookie=UID%3Dabc", "request cookies forwarded")
    assertIncludes(r.returnBody, "is-proxy-session=true", "marks a proxied session")
    assertIncludes(r.returnBody, "referer=", "referer forwarded")
  })

  await test("preserves the original query args", async () => {
    const r = req({ r: "/target", extra: "1" }, { Cookie: "a=1" })
    authHandover.polarisAuthRedirect(r)
    assertIncludes(r.returnBody, "extra=1", "original args kept")
  })
}

async function authRefreshOutbound(authHandover) {
  console.log("\nhandleAuthRefreshOutbound (/auth-refresh-outbound):")

  const call = (cookie, args = "", env = { DEFAULT_UPSTREAM_CMS_DOMAIN_NAME: DEFAULT_CMS }) =>
    withEnv(env, () => {
      const r = createMockRequest({
        headersIn: cookie ? { Cookie: cookie } : {},
        variables: { args },
      })
      authHandover.handleAuthRefreshOutbound(r)
      return r
    })

  await test("prefers handoverEndpoint from the Cms-Session-Hint cookie", async () => {
    const hint = encodeURIComponent(JSON.stringify({ handoverEndpoint: "https://cin4.cps.gov.uk/polaris" }))
    const r = await call(`Cms-Session-Hint=${hint}`)
    assertEqual(r.returnCode, 302, "302")
    assertEqual(r.returnBody, "https://cin4.cps.gov.uk/polaris", "hint wins")
  })

  await test("appends the query string when present", async () => {
    const hint = encodeURIComponent(JSON.stringify({ handoverEndpoint: "https://cin4.cps.gov.uk/polaris" }))
    const r = await call(`Cms-Session-Hint=${hint}`, "a=1&b=2")
    assertEqual(r.returnBody, "https://cin4.cps.gov.uk/polaris?a=1&b=2", "args appended")
  })

  await test("sets X-InternetExplorerMode: 1", async () => {
    const r = await call("")
    assertEqual(r.headersOut["X-InternetExplorerMode"], "1", "forces IE mode")
  })

  await test("falls back to DEFAULT_UPSTREAM_CMS_DOMAIN_NAME without a hint", async () => {
    const r = await call("")
    assertEqual(r.returnBody, `https://${DEFAULT_CMS}/polaris`, "env fallback")
  })

  await test("falls back when the hint cookie is unparseable JSON", async () => {
    const r = await call("Cms-Session-Hint=not-json")
    assertEqual(r.returnBody, `https://${DEFAULT_CMS}/polaris`, "bad JSON -> fallback, not a crash")
  })

  await test("falls back when the hint has no handoverEndpoint", async () => {
    const hint = encodeURIComponent(JSON.stringify({ cmsDomains: [] }))
    const r = await call(`Cms-Session-Hint=${hint}`)
    assertEqual(r.returnBody, `https://${DEFAULT_CMS}/polaris`, "null endpoint -> fallback")
  })

  await test("accepts an un-encoded hint cookie too (_maybeDecodeURIComponent)", async () => {
    const r = await call(`Cms-Session-Hint=${JSON.stringify({ handoverEndpoint: "https://x/polaris" })}`)
    assertEqual(r.returnBody, "https://x/polaris", "raw JSON also parsed")
  })

  // Unreachable from integration: the env var is always set there.
  await test("502 when there is neither a hint nor DEFAULT_UPSTREAM_CMS_DOMAIN_NAME", async () => {
    const r = await call("", "", { DEFAULT_UPSTREAM_CMS_DOMAIN_NAME: undefined })
    assertEqual(r.returnCode, 502, "502")
    assertIncludes(r.returnBody, "no handoverEndpoint", "explains the misconfiguration")
  })
}

async function devLogin(authHandover) {
  console.log("\ndevLogin (dev login) — method-branching header filter:")

  // --- GET: clear the env + BIG-IP/LB cookies ---
  await test("GET clears __CMSENV + every BIG-IP/LB cookie (17 in total)", () => {
    const r = createMockRequest({ method: "GET", headersOut: { "Set-Cookie": [] } })
    authHandover.devLogin(r)
    const cookies = r.headersOut["Set-Cookie"]
    assertEqual(cookies.length, 17, "1 env + 4 envs x (2 BIG-IP + 2 LB)")
    const joined = cookies.join("\n")
    assertIncludes(joined, "__CMSENV=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "env marker")
    assertIncludes(joined, "BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-CIN3-cin3.cps.gov.uk_POOL=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "a BIG-IP pool cookie")
    assertIncludes(joined, "F-CIN5-LBsessioncookie=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT", "an LB session cookie")
  })

  await test("GET appends to (does not replace) what the upstream set", () => {
    const r = createMockRequest({ method: "GET", headersOut: { "Set-Cookie": ["upstream=1"] } })
    authHandover.devLogin(r)
    assertEqual(r.headersOut["Set-Cookie"].length, 18, "upstream cookie kept + 17 clears")
    assertEqual(r.headersOut["Set-Cookie"][0], "upstream=1", "upstream entry preserved first")
  })

  // --- POST: stamp __CMSENV from the OUTGOING Set-Cookie ---
  await test("POST appends __CMSENV derived from the OUTGOING Set-Cookie", () => {
    // Reads headersOut, not headersIn — echoes the env this response establishes.
    const r = createMockRequest({ method: "POST", headersOut: { "Set-Cookie": ["BIGipServer~x~cin4.cps.gov.uk_POOL=1"] } })
    authHandover.devLogin(r)
    assert(r.headersOut["Set-Cookie"].some((c) => c === "__CMSENV=cin4; path=/"), `Expected __CMSENV=cin4, got ${JSON.stringify(r.headersOut["Set-Cookie"])}`)
  })

  await test("POST defaults to 'default' when the outgoing cookie names no environment", () => {
    const r = createMockRequest({ method: "POST", headersOut: { "Set-Cookie": ["session=abc"] } })
    authHandover.devLogin(r)
    assert(r.headersOut["Set-Cookie"].some((c) => c === "__CMSENV=default; path=/"), "Should fall back to default")
  })

  await test("POST preserves the existing Set-Cookie entries", () => {
    const r = createMockRequest({ method: "POST", headersOut: { "Set-Cookie": ["a=1", "b=2"] } })
    authHandover.devLogin(r)
    assertEqual(r.headersOut["Set-Cookie"].length, 3, "Should append, not replace")
  })

  await test("other methods leave Set-Cookie untouched", () => {
    const r = createMockRequest({ method: "PUT", headersOut: { "Set-Cookie": ["x=1"] } })
    authHandover.devLogin(r)
    assertEqual(r.headersOut["Set-Cookie"].length, 1, "no cookie handling for non-GET/POST")
  })
}

async function main() {
  const authHandover = await loadNjs("features/auth-handover.js")
  await appAuthRedirect(authHandover)
  await sessionHint(authHandover)
  await polarisAuthRedirect(authHandover)
  await authRefreshOutbound(authHandover)
  await devLogin(authHandover)
  process.exit(summarise("authHandover.js (unit)"))
}

main()
