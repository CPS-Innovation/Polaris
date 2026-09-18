# unit — njs unit tests

Fast, dependency-free unit tests for the njs modules. Background: `docs/PROXY.md` §7.

```bash
cd polaris-terraform/main-terraform/proxy/tests/unit
./run-tests.sh          # everything
./run-tests.sh cmsenv   # one file (substring match)
```

No Docker, no npm install, runs in ~1s.

## Files

| File                  | Covers                                                                 |
| --------------------- | ---------------------------------------------------------------------- |
| `cmsenv.unit.test.js` | environment detection, all upstream getters × 4 envs, body filters, menu-bar injection, dev-login cookie |
| `nginx.unit.test.js`  | `/init` whitelist, `Cms-Session-Hint` payload, `/polaris`, `/auth-refresh-outbound` |
| `njs-harness.js`      | loads the real njs modules + the mock `r`                              |
| `test-utils.js`       | `test` / `assert*` / `summarise`                                        |

## How the njs modules are loaded

`nginx.js` / `cmsenv.js` are plain ESM (`export default {...}`) living in
`main-terraform/`, a directory with no `package.json` — so node would treat a bare
`.js` as CommonJS and reject the export syntax.

`njs-harness.js` sidesteps that by copying the source to a temp `.mjs` and
`import()`ing it. **No bundler needed** — the sibling `global-components` harness
uses esbuild only because it authors njs in TypeScript. We import the REAL files,
so no copy of the logic can drift.

The njs globals our modules touch are all satisfiable from plain node:

- `qs` — `nginx.js` imports `querystring`, a node built-in.
- `process.env` — `nginx.js` reads `AUTH_HANDOVER_WHITELIST` and
  `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME` this way; tests set/restore them via `withEnv`.
- `r.variables` — `cmsenv.js` reads the CMS settings that `nginx.conf` bridges in
  with `js_var $x ${VAR};`. `cmsVariables()` mirrors `docker/cmsproxy.mock.env`, so
  unit and integration agree on the values.
- `r.return()` / `r.sendBuffer()` — captured by the mock as `returnCode` /
  `returnBody` / `sentBuffer`.

## Why unit as well as integration

Integration proves the wiring; unit reaches what it cannot:

- branches that need an impossible environment — e.g. the **502** when neither a
  session hint nor `DEFAULT_UPSTREAM_CMS_DOMAIN_NAME` exists (integration always
  has the env var set);
- the exact `Cms-Session-Hint` payload and its error path;
- `replaceCmsDomains` — integration cannot distinguish it from the `sub_filter`
  directives that sit in the same location blocks (see the finding below);
- the full 4-environment × Corsham/Farnborough getter matrix, cheaply.

## Conventions

Characterisation tests: they record today's behaviour, warts and all. Anything
that looks wrong is pinned under a `QUIRK:` test with a comment explaining it, so
a refactor cannot change it silently. Fix quirks deliberately, updating the test
in the same change.
