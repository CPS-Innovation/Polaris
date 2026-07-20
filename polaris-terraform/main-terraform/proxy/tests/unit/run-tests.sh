#!/bin/bash
# njs unit test runner.
#
#   ./run-tests.sh            run every *.unit.test.js
#   ./run-tests.sh cmsenv     run only the matching file
#
# No Docker, no npm install — the tests import the real njs sources directly
# (see njs-harness.js).
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FILTER="${1:-}"
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'

cd "$SCRIPT_DIR"
echo "njs unit tests"
echo "=============="

FAILED=0
for f in *.unit.test.js; do
  [ -e "$f" ] || continue
  if [ -n "$FILTER" ] && [[ "$f" != *"$FILTER"* ]]; then continue; fi
  echo ""
  echo -e "${YELLOW}==== $f ====${NC}"
  if ! node "$f"; then
    FAILED=$((FAILED + 1))
  fi
done

rm -rf "$SCRIPT_DIR/.tmp"

echo ""
echo "=============="
if [ $FAILED -gt 0 ]; then
  echo -e "${RED}$FAILED test file(s) failed${NC}"
  exit 1
fi
echo -e "${GREEN}All test files passed${NC}"
