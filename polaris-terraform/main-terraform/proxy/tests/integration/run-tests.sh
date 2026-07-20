#!/bin/bash
# Integration test runner for the cmsproxy.
#
#   ./run-tests.sh              run every test file in tests/
#   ./run-tests.sh smoke        run only tests/smoke.integration.test.js
#   KEEP_UP=1 ./run-tests.sh    leave the stack running afterwards (for poking)
#
# Brings up the real nginx.conf against a mock upstream, waits for health,
# runs the node test files, tears down.
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCKER_DIR="$SCRIPT_DIR/docker"
PROXY_BASE="${PROXY_BASE:-http://localhost:8080}"
FILTER="${1:-}"

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'

echo "cmsproxy integration tests"
echo "=========================="

cleanup() {
  if [ "${KEEP_UP:-}" = "1" ]; then
    echo -e "${YELLOW}KEEP_UP=1 — leaving stack running (${PROXY_BASE})${NC}"
    return
  fi
  echo -e "\n${YELLOW}Stopping stack...${NC}"
  (cd "$DOCKER_DIR" && docker compose down -t 2 >/dev/null 2>&1 || true)
}
trap cleanup EXIT

wait_for_proxy() {
  local attempt=1 max=40
  echo -n "Waiting for proxy..."
  while [ $attempt -le $max ]; do
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

echo -e "${YELLOW}Starting docker compose...${NC}"
cd "$DOCKER_DIR"
docker compose down -t 2 >/dev/null 2>&1 || true
if ! docker compose up -d --build; then
  echo -e "${RED}Failed to start the stack${NC}"
  exit 1
fi

if ! wait_for_proxy; then
  echo -e "${RED}Proxy did not become ready. nginx logs:${NC}"
  docker compose logs --tail=60 nginx
  exit 1
fi

# Run test files
cd "$SCRIPT_DIR"
FAILED=0
for f in tests/*.integration.test.js; do
  [ -e "$f" ] || continue
  if [ -n "$FILTER" ] && [[ "$f" != *"$FILTER"* ]]; then continue; fi
  echo ""
  echo -e "${YELLOW}==== $(basename "$f") ====${NC}"
  if ! PROXY_BASE="$PROXY_BASE" node "$f"; then
    FAILED=$((FAILED + 1))
  fi
done

echo ""
echo "=========================="
if [ $FAILED -gt 0 ]; then
  echo -e "${RED}$FAILED test file(s) failed${NC}"
  exit 1
fi
echo -e "${GREEN}All test files passed${NC}"
