# proxy — tests for the cmsproxy nginx config

Test harness for the nginx CMS proxy whose config lives one level up in
`polaris-terraform/main-terraform/` (`nginx.conf`, `nginx.js`, `cmsenv.js`,
`polaris-script.js`, `global-components.conf`, `global-components.js`).

Background, design and the coverage plan: **[`docs/PROXY.md`](./docs/PROXY.md) §7**.
Bugs and oddities the harness found: **[`docs/QUIRKS.md`](./docs/QUIRKS.md)**.

```
proxy/
  docs/                  PROXY.md · SYSTEM.md · QUIRKS.md · PLAN.md
  tests/
    integration/         Docker-based: boots the REAL config against a mock upstream
      docker/            Dockerfiles, compose, the env manifest, the mock + fixtures
      tests/             *.integration.test.js — one file per feature
      run-tests.sh
      test-utils.js
    unit/                njs unit tests (cmsenv.js, nginx.js) — no docker, no npm
```

## Why this exists

It is the **golden master** for the refactor described in `docs/PROXY.md` §6. These
tests characterise what the config does **today**, so that slicing `nginx.conf`
into per-feature includes and condensing `cmsenv.js` can be proven not to change
behaviour. Write the test first, then refactor, and keep it green.

## Running

```bash
cd polaris-terraform/main-terraform/proxy/tests/integration
./run-tests.sh                 # everything
./run-tests.sh auth-handover   # one file (substring match)
KEEP_UP=1 ./run-tests.sh       # leave the stack up on http://localhost:8080
```

Requires Docker and Node (no npm install — the tests use built-in `fetch` only).

## How it works

- The **real** config is bind-mounted into an `nginx:1.27-alpine` image (plus the
  njs module). Editing `nginx.conf` needs no rebuild — just re-run.
- `docker/cmsproxy.mock.env` supplies **every** app setting the config consumes
  (the canonical list is `docs/PROXY.md` §2). The stock nginx entrypoint renders
  `*.template` with `envsubst`, exactly as Azure does.
- Everything the proxy dials is pointed at one `mock-upstream` service, which
  **echoes the request back as JSON** — so tests assert what nginx forwarded, most
  importantly the `Host` header (which reveals the CMS environment `cmsenv.js`
  picked). Body-rewrite paths get fixtures instead: see `docker/fixtures/README.md`.

## Conventions

- Tests are **characterisation** tests: they record today's behaviour, warts and
  all. Where today's behaviour looks wrong, it is captured under a `QUIRK:` test
  with a comment explaining it — so a refactor cannot change it silently. Fix the
  quirk deliberately, updating the test in the same change.
- No test framework: plain node + `test-utils.js` (`test`/`assert*`), mirroring the
  sibling harness in `global-components/infra/proxy`.
