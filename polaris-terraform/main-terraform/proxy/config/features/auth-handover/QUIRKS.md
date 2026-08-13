# QUIRKS — auth-handover

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

Live line numbers are the live `main-terraform/nginx.js` (the golden master); the
code now lives in `auth-handover.js` in this folder.

---

## A. Security-relevant

### A1. 🔴 An empty or trailing-comma'd `AUTH_HANDOVER_WHITELIST` allows **everything**

**Where (live):** `nginx.js:119-123` (`appAuthRedirect`)
**Where (next):** `auth-handover.js` (`appAuthRedirect`)
**Test:** `auth-handover.unit.test.js` → "QUIRK: an unset whitelist allows EVERYTHING (open redirect + cc leak)", "QUIRK: a TRAILING COMMA in the whitelist also allows everything"

```js
const whitelistedUrls = process.env.AUTH_HANDOVER_WHITELIST ?? "";
whitelistedUrls.split(",").some((url) => redirectUrl.startsWith(url));
```

`"".split(",")` is `[""]`, and **every** string `.startsWith("")` is `true`. So an
empty whitelist does not allow _nothing_ — it allows _everything_. `/init` will
then `302` to any attacker-supplied target **with the CMS session cookie attached
as `?cc=...`**: an open redirect that also leaks the cookie.

A **trailing comma** does the same (`"/auth-refresh-inbound,"` → `["/auth-refresh-inbound", ""]`)
— a plausible tfvars edit that silently disables the allow-list with no error.

**Impact:** latent, not live — terraform sets `AUTH_HANDOVER_WHITELIST` in every
environment ([`../../../docs/PROXY.md` §2](../../../docs/PROXY.md)), so the value is
non-empty today. The exposure is if it is ever unset, blanked, or trailing-comma'd.
**Suggested fix:** drop empty entries — `.split(",").filter(Boolean)` — and treat an
empty list as deny-all.
**DECIDED 2026-07-17: leave as-is.** Stays recorded here and pinned by its tests;
raise a bug ticket **after** the refactor lands rather than changing behaviour
mid-flight. (Same for A2 — a natural companion fix when the ticket is picked up.)

### A2. 🔴 The whitelist is a **prefix** match, not an origin match

**Where (live):** `nginx.js:121-123`
**Where (next):** `auth-handover.js` (`appAuthRedirect`)
**Test:** `auth-handover.unit.test.js` → "QUIRK: whitelist is a prefix match, not an origin match"

`redirectUrl.startsWith(entry)` means an entry of `https://allowed.example.org`
also permits `https://allowed.example.org.evil.com/x` — a different origin that
merely _starts with_ the allowed string.

**Impact:** depends on the real entries. `PROXY.md` §2 shows prod entries that end
in `/` (e.g. `https://cps.outsystemsenterprise.com/`), which blunts this — but
`/auth-refresh-inbound` and the bare `https://polaris.cps.gov.uk/auth-refresh-inbound?...`
entries do not all end in a delimiter.
**Suggested fix:** compare parsed origins, or require entries to end in `/`.

---

## C. Fragile / surprising behaviour

### C1. 🔴 The legacy `_POOL` regex over-captures separator-less cookie names

**Where (live):** `nginx.js:84` (`setSessionHintCookie` fallback)
**Where (next):** `auth-handover.js` (`setSessionHintCookie`)
**Test:** `auth-handover.unit.test.js` → "QUIRK: separator-less BIGipServer<pool> over-captures the domain"; `auth-handover.integration.test.js` → "QUIRK: separator-less BIGipServer<pool> cookie over-captures the domain"

```
/[a-z][a-z0-9]*(?:\.[a-z][a-z0-9]*)*\.cps\.gov\.uk(?=_POOL)/g
```

It anchors on a **lowercase** run to skip uppercase LB prefixes (the comment cites
`CPSACP-LTM-CM-WAN-CIN3-`, which works). But where lowercase letters run straight
into the domain with no separator — the classic F5 `BIGipServer<pool>` name — it
starts mid-word:

```
BIGipServercin2.cps.gov.uk_POOL  ->  "ervercin2.cps.gov.uk"
                                 ->  handoverEndpoint https://ervercin2.cps.gov.uk/polaris
```

**Impact: LEGACY — not reachable today.** Answered 2026-07-17: neither prod nor any
pre-prod environment uses `BIGipServer`-format LB cookies any more; they all use
the `[CF]-<TOKEN>-LBsessioncookie` form, which takes the _preferred_ branch and is
unaffected. This whole `_POOL` fallback is dead legacy.
**Suggested fix:** the over-capture needs no fix on its own — the question is
whether the `_POOL` fallback branch should exist at all. A candidate for deletion
during §6 (which would also remove this quirk). The test stays until then.

### C5. 🔴 A missing `cookie` arg silently produces an `{error}` session hint

**Where (live):** `nginx.js:68-112` (`setSessionHintCookie`'s `try/catch/finally`)
**Where (next):** `auth-handover.js` (`setSessionHintCookie`)
**Test:** `auth-handover.unit.test.js` → "QUIRK: a missing cookie arg yields an {error} hint payload, not a failure"

With no `cookie` query arg, `cookie.match(...)` throws; the `catch` writes
`{ error }` into the `Cms-Session-Hint` cookie and the `finally` still sets it —
and the redirect proceeds as normal. The user gets a working-looking redirect and
a poisoned hint cookie, with nothing logged.

**Suggested fix:** decide whether that is a 4xx, and at minimum log it.

---

## New-gen (drop rollout) — not golden-master, design notes

### N1. ⚪ Per-user enrolment page is open (canary)

**Where (next):** `auth-handover.{conf,js}` (`location = /auth-refresh-enrol`,
`authHandover.enrol`); the gate is the `$cookie_polaris_auth_handover` `if`s in
`/auth-refresh-inbound`.
**Test:** `auth-handover.unit.test.js` → "enrol — per-user drop enrolment cookie".

Any user can visit `/auth-refresh-enrol` and set `polaris_auth_handover=drop1|drop2`,
which **overrides the global switches** and routes only THAT browser into the drop
(remove the cookie to fall back to the globals). This is deliberate for canary rollout,
and low-risk: the drops are additive/best-effort (drop2 degrades to drop1, drop1
fail-redirects), so a self-enrolled user can't get a worse outcome than the legacy path.
**Before prod / wide rollout:** decide whether to gate the page (allowlist, `internal`,
or an auth check) so enrolment is controlled rather than open. The cookie is HttpOnly +
`SameSite=Lax` and read only server-side (nginx `$cookie_`).

### N2. ⚪ No explicit "force DDEI" opt-out value (yet)

Enrolment supports `drop1` / `drop2` / removed(→globals). There is no cookie value that
forces **legacy DDEI when a global switch is ON** — an escape hatch worth adding once a
drop becomes the global default (a `ddei` value routing to an internal DDEI `proxy_pass`
location, ahead of the global `if`s). Not built while the globals are off.
