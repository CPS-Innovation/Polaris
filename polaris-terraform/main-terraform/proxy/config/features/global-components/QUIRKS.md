# QUIRKS — global-components

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

Live line numbers are the live `main-terraform/cmsenv.js` (the golden master); the
code now lives in `global-components.js` in this folder — `getDomainFromCookie` was
moved here verbatim from `cmsenv.js` because the `/api/navigate-cms` route that
calls it is a global-components route.

---

## C. Fragile / surprising behaviour

### C4. 🔴 `getDomainFromCookie` throws when nothing matches

**Where (live):** `cmsenv.js:1-6`
**Where (next):** `global-components.js` (`getDomainFromCookie`)
**Test:** `global-components.unit.test.js` → "QUIRK: throws when the cookie has no \*.cps.gov.uk domain", "QUIRK: throws when there is no Cookie header at all"

```js
let domainMatch = cookie.match(/([a-z0-9]+)\.cps\.gov\.uk/);
return domainMatch[0]; // TypeError when match() returns null
```

No null guard, so a request to `/api/navigate-cms` without a CMS domain cookie
raises an njs exception on the `js_set` rather than producing a value.

**Suggested fix:** `return domainMatch ? domainMatch[0] : ""`.
