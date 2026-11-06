# QUIRKS — common (CMS detection)

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

Live line numbers are the live `main-terraform/cmsenv.js` (the golden master); the
code now lives in `cmsenv.js` in this folder (`detect`).

---

## C. Fragile / surprising behaviour

### C2. ✅ Fixed: CMS environment detection was a loose substring match over the whole `Cookie` header

**Where (live):** `cmsenv.js` (`__getCmsEnvInternal`)
**Where (next):** `cmsenv.js` (`detect`)
**Test:** `cmsenv.unit.test.js` → "unrelated cookies never choose, whatever they contain", `detectOutgoing`; `cms-proxy.integration.test.js`, `auth-handover.integration.test.js` (both configs)

It used to substring-search the entire header, so any cookie mentioning `cin3` chose
CIN3. That bit in earnest once `Cms-Session-Hint` (30 days, its value names the CMS
domain of its last handover) reached the IE-mode jar: `/cin5` could no longer leave
CIN3, and each handover renewed the hint.

Now only `__CMSENV` or `DEVLOGIN` (when the value names an environment; DDEI's dev login
sends `DEVLOGIN`, so it is a contract with that repo) and the load-balancer cookies,
recognised by name (`[CF]-<ENV>-LBsessioncookie`, `BIGipServer*`), choose. The dev-login
filter stamps `__CMSENV` from all the outgoing `Set-Cookie`s (`detectOutgoing`), as DDEI's
first is usually not the load-balancer cookie and a wrong stamp now sticks. DDEI's full-cookie
login sets only `Cms-Auth-Values`, so the CMS cookies are read from inside its value (the
percent-encoded JSON's `cookies` field, any case); the old substring scan found them there by
accident.

### C3. Fixed check order: `cin3` beats `cin2` beats `cin4`/`cin5` (load-balancer cookies only)

**Where (live):** `cmsenv.js` (`__getCmsEnvInternal`)
**Where (next):** `cmsenv.js` (`detect`)
**Test:** `cmsenv.unit.test.js` → "several environments' LB cookies resolve in fixed order: cin3, cin2, cin4, cin5"

`__CMSENV` now wins outright (`__CMSENV=cin2; C-CIN3-LBsessioncookie=x` is cin2, and the
`/cin3` switch's `__CMSENV=default` beats a stale cin5 LB cookie). The fixed order only
remains between load-balancer cookies when there is no `__CMSENV` to go by.

**Suggested fix:** same as C2 — parse the value instead of scanning the header.

---

## D. Config smells and cleanup opportunities

### D9. ⚪ "DEFAULT" means CIN3

**Where (live):** `var.cms_details.default_*`, `cmsenv.js:165`
**Where (next):** `cmsenv.js` (`detect`)

`if (cookie.includes("cin3")) return "default"`. The naming misleads every reader —
worth renaming, or at least a prominent comment, during §6.
