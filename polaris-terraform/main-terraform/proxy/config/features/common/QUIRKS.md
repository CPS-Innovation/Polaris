# QUIRKS — common (CMS detection)

Quirks owned by this feature. See [`../../QUIRKS.md`](../../QUIRKS.md) for the status
key, the full index, the decisions log and the cross-cutting/terraform quirks.

Live line numbers are the live `main-terraform/cmsenv.js` (the golden master); the
code now lives in `cms-detection.js` in this folder.

---

## C. Fragile / surprising behaviour

### C2. 🔴 CMS environment detection is a loose substring match over the whole `Cookie` header

**Where (live):** `cmsenv.js:163-170` (`__getCmsEnvInternal`)
**Where (next):** `cms-detection.js` (CMS-env detection)
**Test:** `cms-detection.unit.test.js` → "QUIRK: any cookie containing 'cin2' selects cin2, even unrelated ones"

```js
cookie = cookie.toLowerCase();
if (cookie.includes("cin3")) return "default";
if (cookie.includes("cin2")) return "cin2";
...
```

It does not parse `__CMSENV` — it substring-searches the **entire** Cookie header.
So _any_ cookie containing `cin2` anywhere selects cin2, including a BIG-IP cookie
naming a domain, or an unrelated `somethingcin2something=1`.

**Suggested fix:** read the `__CMSENV` cookie value properly.

### C3. 🔴 Fixed check order means `cin3` beats `cin2` beats `cin4`/`cin5`

**Where (live):** `cmsenv.js:165-168`
**Where (next):** `cms-detection.js` (CMS-env detection)
**Test:** `cms-detection.unit.test.js` → "QUIRK: cin3 wins over cin2 when both appear (fixed check order)", "QUIRK: cin2 wins over cin4/cin5 when several appear"

The checks return on the first hit in the order cin3 → cin2 → cin4 → cin5. With
C2, a header mentioning several environments resolves by that fixed order rather
than by which cookie is authoritative:

```
Cookie: __CMSENV=cin2; other=cin3   ->  DEFAULT   (not cin2!)
```

Realistic when stale BIG-IP cookies from another environment linger — which is
exactly what the `/cinN` switch blocks exist to clear.

**Suggested fix:** same as C2 — parse the value instead of scanning the header.

---

## D. Config smells and cleanup opportunities

### D9. ⚪ "DEFAULT" means CIN3

**Where (live):** `var.cms_details.default_*`, `cmsenv.js:165`
**Where (next):** `cms-detection.js` (CMS-env detection)

`if (cookie.includes("cin3")) return "default"`. The naming misleads every reader —
worth renaming, or at least a prominent comment, during §6.
