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

CONFIG_DIR="$(cd "$SCRIPT_DIR/../../config" && pwd)"

cd "$SCRIPT_DIR"
echo "njs unit tests"
echo "=============="

# Tests are DISCOVERED at run time: each feature owns its own folder under
# config/features/<name>/ containing <name>.conf, <name>.js and <name>.unit.test.js.
# Dropping in (or deleting) a feature folder needs no change here.
FAILED=0
for f in $(find "$SCRIPT_DIR" "$CONFIG_DIR" -name '*.unit.test.js' -not -path '*/.tmp/*' | sort); do
  [ -e "$f" ] || continue
  if [ -n "$FILTER" ] && [[ "$f" != *"$FILTER"* ]]; then continue; fi
  echo ""
  echo -e "${YELLOW}==== $(basename "$f") ====${NC}"
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
