# unit — njs unit tests

Fast, dependency-free unit tests for the njs modules of the **next** config
(`proxy/config/`). Background: `docs/PROXY.md` §7. The live monolith is covered by the
integration suite only.

```bash
cd polaris-terraform/main-terraform/proxy/tests/unit
./run-tests.sh          # everything
./run-tests.sh cmsenv   # files whose path contains the substring
```

No Docker, no npm install, runs in seconds.

## Where the tests live

`run-tests.sh` discovers and runs **every** `*.unit.test.js` under `config/` and this
folder (skipping `.tmp/`). Feature tests sit **beside the code they pin**, in
`config/features/<name>/<name>.unit.test.js` (plus `common/cmsenv.unit.test.js` and
`common/ie-mode.unit.test.js`) — a new feature folder needs no wiring here.

This folder holds only shared machinery and the two tripwires:

| File                         | Role                                                                                         |
| ---------------------------- | -------------------------------------------------------------------------------------------- |
| `run-tests.sh`               | discovers + runs every `*.unit.test.js`; removes `.tmp/` afterwards                          |
| `njs-harness.js`             | loads the real njs modules + the mock `r`; `cmsEnvObject` / `applyEnv` for `process.env`     |
| `test-utils.js`              | `test` / `assert*` / `summarise`                                                             |
| `deploy-safety.unit.test.js` | tripwire: `*.test.js` / `fixtures/` are excluded from the terraform fileset and the staging  |
| `cutover.unit.test.js`       | tripwire: Startup Command, `start-next.sh` blob, both slots and the `--next` stack agree      |

## How the njs modules are loaded

The feature modules are plain ESM (`export default {...}`) in `config/features/`, a tree
with no `package.json` — so node would treat a bare `.js` as CommonJS and reject the
export syntax. They also import each other across folders (e.g.
`import cmsenv from "../common/cmsenv.js"`).

`njs-harness.js` `loadNjs()` therefore stages every non-test `.js` under `config/`
(skipping `fixtures/`) into `tests/unit/.tmp`, **mirroring** the `config/` layout, with a
`package.json` marking it ESM, then `import()`s the target from there — so every
relative import resolves exactly as deployed. **No bundler needed** — the sibling
`global-components` harness uses esbuild only because it authors njs in TypeScript. We
import the REAL files, so no copy of the logic can drift.

The njs globals our modules touch are all satisfiable from plain node:

- `qs` — `auth-handover.js` imports `querystring`, a node built-in.
- `process.env` — the modules read their app settings this way (there is no `js_var`
  bridge in the next config): `cmsenv.setting`'s `<ENV>_UPSTREAM_CMS_*` lookups,
  `AUTH_HANDOVER_WHITELIST`, `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME`, `ENDPOINT_HTTP_PROTOCOL`,
  the drop switches, … Tests set/restore them with `applyEnv`; `cmsEnvObject()` supplies
  the CMS settings, mirroring `docker/cmsproxy.mock.env` for DEFAULT, CIN2, CIN4 and CIN5
  (the mock env also has CPT; `cmsEnvObject` does not, so the CPT settings are exercised by integration only).
- `r.variables` — carries only the per-request built-in `$host`.
- `r.return()` / `r.sendBuffer()` — captured by the mock as `returnCode` /
  `returnBody` / `sentBuffer`.

## Why unit as well as integration

Integration proves the wiring; unit reaches what it cannot:

- branches that need an impossible environment — e.g. the **502** when neither a
  session hint nor `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME` exists (integration always
  has the env var set);
- the exact `Cms-Session-Hint` payload and its error path;
- `replaceCmsDomains` — integration cannot distinguish it from the `sub_filter`
  directives that sit in the same location blocks (QUIRKS **B1**,
  `config/features/cms-proxy/QUIRKS.md`);
- the per-environment × Corsham/Farnborough getter matrix, cheaply;
- the new-gen drops (drop1 / drop2), which the parity suite runs switched off.

## Conventions

Characterisation tests: they record today's behaviour, warts and all. Anything
that looks wrong is pinned under a `QUIRK:` test with a comment explaining it, so
a refactor cannot change it silently. Fix quirks deliberately, updating the test
in the same change.
