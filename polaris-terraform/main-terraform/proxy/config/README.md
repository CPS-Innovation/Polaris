# config — the "next" (to-be) proxy config

**Not operational.** These files are **not** wired into terraform and are **not**
deployed. `app-service-proxy.tf` still uploads the live config from
`main-terraform/` ([`../docs/PROXY.md` §6.5](../docs/PROXY.md)). This folder is a
staging ground for the refactor.

## What it is

The target shape from [`../docs/PROXY.md` §6](../docs/PROXY.md): `nginx.conf` sliced
into one `include` per feature, `cmsenv.js`'s shared core pulled into `common.js`,
one njs module per feature. It started as a **verbatim copy** of the live config and
is refactored **in place here**, while the live config keeps running untouched.

## Progress

- ✅ **Location slicing — DONE.** All 46 `location` blocks moved out of `nginx.conf`
  into `features/00-health.conf` … `xx-redundant.conf`. The parent `nginx.conf` is now
  just the server preamble (`js_import` / `js_var` / `limit_req_zone` / `resolver` /
  security headers / `$ieaction`) plus `include global-components*.conf;` and
  `include features/*.conf;`. `./run-tests.sh --next` is green — identical behaviour
  to the live monolith.
- ⬜ **njs split — TODO.** `nginx.js` and `cmsenv.js` are still copied here whole and
  still `js_import`ed by the parent (feature files call `nginx.*` / `cmsenv.*` across
  the include boundary, which works fine). Next: `nginx.js` → `auth-handover.js`,
  `cmsenv.js` `getDomainFromCookie` → `navigate-cms.js`, the rest → `common.js`
  ([`../docs/QUIRKS.md` B1](../docs/QUIRKS.md) decides `replaceCmsDomains`'s fate first).
- ⬜ **Cutover — TODO.** Point terraform at this folder ([§6.5](../docs/PROXY.md)),
  delete the live copies.

## How we prove it stays correct

The integration suite runs against **either** config, selected by a compose
override — the test files are identical:

```bash
cd ../tests/integration
./run-tests.sh          # live config  (main-terraform/*)
./run-tests.sh --next   # this config  (proxy/config/*)
```

Green on both = the refactor changed no behaviour. Today, `--next` is a byte-for-byte
copy, so both must pass identically; that proves the plumbing before any location
moves. As locations move into `features/*.conf`, keep `--next` green at every step.

Mechanism: `../tests/integration/docker/docker-compose.next.yml` repoints the config
mounts from `main-terraform/` to here. Docker Compose merges volumes by target, so it
only lists the sources it changes.

## Migrating (the loop)

1. Move one feature's `location` block(s) from `nginx.conf` here into
   `features/NN-<feature>.conf`, and add `include features/*.conf;` to the parent.
2. Add the matching mount line(s) to `docker-compose.next.yml` (each `.conf` needs a
   `.template`-suffixed target — envsubst only renders `*.template`).
3. `./run-tests.sh --next` — must stay green. Commit.
4. Repeat. Keep all CMS regexes together in `05-cms-proxy.conf` (the ordering
   contract, [`../docs/PROXY.md` §5](../docs/PROXY.md)).

## Cutover (later)

When `--next` is green with the fully sliced config, point the terraform blob/`md5`
wiring at this folder ([`../docs/PROXY.md` §6.5](../docs/PROXY.md)) and delete the
old files. One terraform change; the tests have already proved parity.
