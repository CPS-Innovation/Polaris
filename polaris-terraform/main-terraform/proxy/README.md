# proxy — cmsproxy config refactor + test harness

This folder holds two halves of one job on the nginx **CMS proxy**:

1. a **golden-master test harness** (`tests/`) that characterises what the proxy
   config does **today**, and
2. the **"next" (refactored) config** (`config/`) being grown feature-by-feature,
   proven behaviour-identical by that same harness.

The **live** config still lives one level up in `polaris-terraform/main-terraform/`
(`nginx.conf`, `nginx.js`, `cmsenv.js`, `polaris-script.js`, `global-components.*`)
and is the default boot. `config/` **is** deployed (terraform uploads it beside the live
blobs) and **runs** wherever `var.proxy_next_config_enabled` is true — see
[Cutover](#cutover-terraform-controlled).

Pointers:
- Deep-dive + repo navigation: **[`docs/PROXY.md`](./docs/PROXY.md)** (settings grid §2,
  endpoint map §3, refactor plan §6, testing §7).
- Bugs/oddities: **[`config/QUIRKS.md`](./config/QUIRKS.md)** — cross-cutting quirks +
  the index; feature-specific quirks in each `config/features/<name>/QUIRKS.md`.
- Phased roadmap: [`docs/PLAN.md`](./docs/PLAN.md).

## Layout

```
proxy/
  docs/
    PROXY.md            deep-dive + repo navigation (§1–§7) + appendices
    PLAN.md             phased roadmap
  config/               the "next" (to-be, refactored) config — deployed; booted when
                        var.proxy_next_config_enabled (see Cutover)
    nginx.conf          parent: server preamble + `include features/*/*.conf;`
                        (deployed as nginx-next.conf.template)
    start-next.sh       the cutover script the App Service Startup Command runs
    nginx.integration.test.js   suite for the parent config
    QUIRKS.md           cross-cutting quirks + the whole-set index
    features/common/    the shared njs: cmsenv.js (CMS-env detect/setting + the
                        global-components getter contract) + ie-mode.js
    features/<name>/    one self-contained folder per feature:
      <name>.conf         the location block(s)
      <name>.js           its njs module (where the feature needs request-time JS)
      <name>.unit.test.js         ⎫ tests live BESIDE the code they pin
      <name>.integration.test.js  ⎭ (not under tests/)
      QUIRKS.md           feature-specific quirks
      fixtures/           canned upstream bodies (cms-proxy only)
  tests/                shared harness infra — runners + mock; no feature tests here
    integration/
      run-tests.sh        boots a config against the mock, runs the discovered suites
      test-utils.js
      docker/             Dockerfile.nginx · Dockerfile.mock · docker-compose[.next].yml
                          · cmsproxy.mock.env · mock-server.js · stage-config.sh
    unit/
      run-tests.sh        runs every *.unit.test.js (config/features/** + here)
      njs-harness.js      loads the real njs modules with a mock `r`
      test-utils.js
      deploy-safety.unit.test.js   tripwire: test files/fixtures must never deploy
      cutover.unit.test.js         tripwire: Startup Command, start-next.sh blob, both
                                   slots and the --next stack agree
      README.md           how the unit harness works
```

Test **files** are discovered at run time from `config/` — the parent suite sits
beside `config/nginx.conf`, and each feature ships its own `*.unit.test.js` /
`*.integration.test.js`. The `tests/` tree holds only shared machinery (Docker, the
mock upstream, `test-utils.js`, the njs harness) plus the deploy-safety and cutover
tripwires.

## Live vs. next config

The integration suite runs against **either** config, selected by a flag. The test
files are identical, so **green on both proves the refactor changed no behaviour**:

```bash
cd tests/integration
./run-tests.sh          # LIVE config (main-terraform/*) — the image-default boot
./run-tests.sh --next   # NEXT config (config/*)         — what start-next.sh boots
                        #   TWICE: switches off (parity with live), then on as deployed
                        #   (NON_DDEI_INIT_ENABLED=true; --next-off / --next-on for one pass)
```

Mechanism: `tests/integration/docker/docker-compose.next.yml` repoints the config
mounts from `main-terraform/` to `config/` and boots the container through
`config/start-next.sh` — the same script the App Service Startup Command runs, so a green
`--next` also proves the cutover boot. Docker Compose merges volumes by target, so it
lists only the sources it changes.

## Running the tests

```bash
# integration (Docker + a mock upstream)
cd polaris-terraform/main-terraform/proxy/tests/integration
./run-tests.sh                 # everything, live config
./run-tests.sh --next          # everything, next config
./run-tests.sh auth-handover   # one suite (substring filter)
KEEP_UP=1 ./run-tests.sh       # leave the stack up on http://localhost:8080

# njs unit tests (fast, no Docker)
cd ../unit
./run-tests.sh                 # every *.unit.test.js (features + deploy-safety + cutover)
./run-tests.sh cmsenv          # substring filter
```

Requires Docker and Node for integration; unit needs only Node. No `npm install` —
the tests use built-in `fetch` and a tiny local `test-utils.js`.

## Why this exists — the golden master

It is the **golden master** for the refactor in [`docs/PROXY.md` §6](./docs/PROXY.md):
these tests pin today's behaviour so that slicing `nginx.conf` into per-feature
includes and condensing `cmsenv.js` → `features/common/cmsenv.js` can be proven **not**
to change it.
Write the test first, then refactor, and keep it green.

## The refactor (`config/`)

The target shape from [`docs/PROXY.md` §6](./docs/PROXY.md): `nginx.conf` sliced into
one `include` per feature, `cmsenv.js`'s shared core pulled into `features/common/cmsenv.js`, one njs
module per feature. It started as a **verbatim copy** of the live config and is
refactored **in place** here, while the live config keeps running untouched.

### Progress

- ✅ **Location slicing — DONE.** Every `location` block moved out of `config/nginx.conf`
  into `features/<name>/<name>.conf`. The parent `nginx.conf` is now just the server
  preamble (`js_import` / `limit_req_zone` / `resolver` / security headers / `$ieaction`,
  plus the `cmsenv` contract import — no `js_var` bridge: njs reads the CMS settings from
  `process.env` via `cmsenv.setting`) and `include features/*/*.conf;` — which
  also loads configs the global-components repo deploys as `features/global-components.<x>/`
  (see [`docs/PROXY.md` §6.8](./docs/PROXY.md)).
  `./run-tests.sh --next` is green — identical behaviour to the live monolith.
- ✅ **Deploy plumbing — DONE.** `app-service-proxy.tf` uploads the `config/` tree to
  the proxy's blob container beside the live blobs (the `proxy_next_*` fileset: feature
  `.conf`s get a `.template` suffix for envsubst; `.js` stay as-is; `*.test.js`/`fixtures/`
  excluded — guarded by `deploy-safety.unit.test.js`), plus the refactored root as
  `nginx-next.conf.template` and the `start-next.sh` blob. Nothing loads them unless the
  Startup Command runs `start-next.sh` (below).
- 🟡 **njs split — mostly done.** The monolithic `nginx.js` / `cmsenv.js` are gone;
  their logic lives in per-feature modules (`auth-handover.js`, `cms-proxy.js`,
  `global-components.js`, `polaris-ddei.js`, `app-launch.js`) plus the shared
  `common/cmsenv.js` (CMS-env detection + the global-components getter contract). Remaining: the `replaceCmsDomains` delete/repair decision
  ([`features/cms-proxy/QUIRKS.md` B1](./config/features/cms-proxy/QUIRKS.md)).
- 🟡 **Cutover — terraform-controlled** (#2190). `proxy_next_config_enabled` is true in
  dev, QA, UAT **and prod**; done when #2190 is applied everywhere, then the live monolith
  is deleted. See below.

### Migrating (the loop)

1. Move one feature's `location` block(s) into `config/features/<name>/<name>.conf`
   (the parent already `include`s `features/*/*.conf`).
2. `./run-tests.sh --next` — must stay green. Commit.
3. Repeat. Keep all CMS regexes together in `features/cms-proxy/cms-proxy.conf` (the
   first-match-wins ordering contract, [`docs/PROXY.md` §5](./docs/PROXY.md)).

### Cutover (terraform-controlled)

`var.proxy_next_config_enabled` sets the App Service **Startup Command** on both slots
(identical, so a swap never changes which config runs) to
`sh /etc/nginx/templates/start-next.sh`. That script execs the image's own entrypoint
with `nginx -c /etc/nginx/nginx-next.conf` — the stock envsubst startup, pointed at the
next entry file. The command is whitespace-free on purpose: the Startup Command field
splits on spaces and strips quotes, so the quoting lives in the script. Empty Startup
Command (`false`) = the image default = the live monolith.

- **Rollback:** set `proxy_next_config_enabled = false` and apply.
- **Remaining:** apply #2190 in every environment, then delete the live monolith
  (`nginx.conf`, `nginx.js`, `cmsenv.js`, … and their blobs).
- **Tripwire:** `tests/unit/cutover.unit.test.js` checks the command, the blob, both slots
  and the `--next` stack agree.

## How the harness works

- The **real** config is bind-mounted into an `nginx:1.27-alpine` image (plus the njs
  module). Editing a `.conf` needs no rebuild — just re-run.
- `docker/cmsproxy.mock.env` supplies **every** app setting the config consumes (the
  canonical list is [`docs/PROXY.md` §2](./docs/PROXY.md)). The stock nginx entrypoint
  renders `*.template` with `envsubst`, exactly as Azure does; `stage-config.sh`
  reproduces the deployed `features/` layout for the `--next` run.
- Everything the proxy dials points at one `mock-upstream` service, which **echoes the
  request back as JSON** — so tests assert what nginx forwarded, most importantly the
  `Host` header (which reveals the CMS environment `cmsenv.js` picked). Body-rewrite
  paths get fixtures instead: see `config/features/cms-proxy/fixtures/`.
- Unit tests load the **real** njs modules via `tests/unit/njs-harness.js` (stages a
  mirrored `config/` tree with an ESM `package.json` into `tests/unit/.tmp` and
  `import()`s from it, so relative imports resolve as deployed) — no bundler, no drift
  from source.

## Conventions

- Tests are **characterisation** tests: they record today's behaviour, warts and all.
  Where today's behaviour looks wrong, it is captured under a `QUIRK:` test with a
  comment (and written up in the relevant `QUIRKS.md`) — so a refactor cannot change
  it silently. Fix the quirk deliberately, updating the test in the same change.
- No test framework: plain Node + `test-utils.js` (`test` / `assert*`), mirroring the
  sibling harness in `global-components/infra/proxy`.
- Features are **drop-in**: a new feature is a `config/features/<name>/` folder with
  its `.conf` / `.js` / `*.test.js` / `QUIRKS.md`; both runners discover it, and the
  terraform `fileset` deploys it — no wiring to edit.
