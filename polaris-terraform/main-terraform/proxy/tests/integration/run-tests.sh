#!/bin/bash
# Integration test runner for the cmsproxy.
#
#   ./run-tests.sh              run every test file against the LIVE config (main-terraform/*)
#   ./run-tests.sh --next       run the same tests against the NEXT config (proxy/config/*), TWICE:
#                                 1. switches OFF (cmsproxy.mock.env) — parity with the live config
#                                 2. switches ON as deployed (docker-compose.switches-on.yml:
#                                    NON_DDEI_INIT_ENABLED=true, so drop1 answers /auth-refresh-inbound)
#   ./run-tests.sh --next-off   pass 1 only;   ./run-tests.sh --next-on   pass 2 only
#   ./run-tests.sh smoke        run only tests/smoke.integration.test.js
#   ./run-tests.sh --next cms   --next + a filter
#   KEEP_UP=1 ./run-tests.sh    leave the (last) stack running afterwards (for poking)
#
# Brings up the nginx config against a mock upstream, waits for health, runs the
# node test files, tears down. The test files are identical for every pass — green on
# live and next-off proves the refactor changed no behaviour (see docs/PROXY.md §6/§7);
# the few assertions that depend on a switch read PROXY_SWITCHES (test-utils switchesOn).
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCKER_DIR="$SCRIPT_DIR/docker"
PROXY_BASE="${PROXY_BASE:-http://localhost:8080}"
CONFIG_DIR="$(cd "$SCRIPT_DIR/../../config" && pwd)"

# Args: --next / --next-off / --next-on (anywhere); any other arg is a filename filter.
# A pass is "<kind>:<switches>"; each runs the whole suite against its own stack.
PASSES="live:off"
FILTER=""
for arg in "$@"; do
  case "$arg" in
    --next)     PASSES="next:off next:on" ;;
    --next-off) PASSES="next:off" ;;
    --next-on)  PASSES="next:on" ;;
    *)          FILTER="$arg" ;;
  esac
done

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'

compose_files() {
  local kind=$1 switches=$2
  local files="-f docker-compose.yml"
  [[ "$kind" = "next" ]] && files="$files -f docker-compose.next.yml"
  [[ "$switches" = "on" ]] && files="$files -f docker-compose.switches-on.yml"
  echo "$files"
}

COMPOSE_FILES=""   # the stack currently up, for cleanup
cleanup() {
  [[ -z "$COMPOSE_FILES" ]] && return
  if [[ "${KEEP_UP:-}" = "1" ]]; then
    echo -e "${YELLOW}KEEP_UP=1 — leaving stack running (${PROXY_BASE})${NC}"
    return
  fi
  echo -e "\n${YELLOW}Stopping stack...${NC}"
  (cd "$DOCKER_DIR" && docker compose $COMPOSE_FILES down -t 2 >/dev/null 2>&1 || true)
}
trap cleanup EXIT

wait_for_proxy() {
  local attempt=1 max=40
  echo -n "Waiting for proxy..."
  while [[ $attempt -le $max ]]; do
    if curl -sf "$PROXY_BASE/" >/dev/null 2>&1; then
      echo -e " ${GREEN}ready${NC}"
      return 0
    fi
    echo -n "."
    sleep 1
    attempt=$((attempt + 1))
  done
  echo -e " ${RED}timeout${NC}"
  return 1
}

FAILED=0
RAN=0

# One pass: bring the stack up, run every (filtered) test file, tear it down.
run_pass() {
  local kind=$1 switches=$2
  local label="live (main-terraform)"
  [[ "$kind" = "next" ]] && label="next (proxy/config), switches $switches"

  echo ""
  echo "cmsproxy integration tests"
  echo "config under test: $label"
  echo "=========================="

  # Previous pass's stack down first (cleanup only handles the last one).
  if [[ -n "$COMPOSE_FILES" ]]; then
    (cd "$DOCKER_DIR" && docker compose $COMPOSE_FILES down -t 2 >/dev/null 2>&1 || true)
  fi
  COMPOSE_FILES="$(compose_files "$kind" "$switches")"

  echo -e "${YELLOW}Starting docker compose...${NC}"
  (cd "$DOCKER_DIR" && docker compose $COMPOSE_FILES down -t 2 >/dev/null 2>&1 || true)
  if ! (cd "$DOCKER_DIR" && docker compose $COMPOSE_FILES up -d --build); then
    echo -e "${RED}Failed to start the stack${NC}"
    exit 1
  fi
  if ! wait_for_proxy; then
    echo -e "${RED}Proxy did not become ready. nginx logs:${NC}"
    (cd "$DOCKER_DIR" && docker compose $COMPOSE_FILES logs --tail=60 nginx)
    exit 1
  fi

  # Test files are DISCOVERED at run time from config/: nginx.conf's own suite sits
  # beside it (config/nginx.integration.test.js) and each feature ships its own
  # (config/features/<name>/<name>.integration.test.js) — so dropping in a feature
  # complete with its tests needs no change here.
  local f
  for f in $(find "$CONFIG_DIR" -name '*.integration.test.js' | sort); do
    [[ -e "$f" ]] || continue
    if [[ -n "$FILTER" ]] && [[ "$f" != *"$FILTER"* ]]; then continue; fi
    RAN=$((RAN + 1))
    echo ""
    echo -e "${YELLOW}==== $(basename "$f") [$label] ====${NC}"
    if ! PROXY_BASE="$PROXY_BASE" PROXY_CONFIG_KIND="$kind" PROXY_SWITCHES="$switches" node "$f"; then
      FAILED=$((FAILED + 1))
      echo -e "${RED}^ failed in pass: $label${NC}"
    fi
  done
}

for pass in $PASSES; do
  run_pass "${pass%%:*}" "${pass#*:}"
done

echo ""
echo "=========================="
# A filter that matches nothing (e.g. a mistyped flag, which lands here as a filter)
# must not report success.
if [[ $RAN -eq 0 ]]; then
  echo -e "${RED}No test files matched${FILTER:+ filter '$FILTER'} — nothing ran${NC}"
  exit 1
fi
if [[ $FAILED -gt 0 ]]; then
  echo -e "${RED}$FAILED test file run(s) failed${NC}"
  exit 1
fi
echo -e "${GREEN}All test files passed${NC}"
