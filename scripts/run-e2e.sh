#!/usr/bin/env bash
# ============================================================
# One-command E2E test runner
# Usage: ./scripts/run-e2e.sh [--headed] [--filter=po]
# ============================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
E2E_DIR="$ROOT_DIR/e2e"

HEADED=""
FILTER=""
for arg in "$@"; do
  case "$arg" in
    --headed) HEADED="--headed" ;;
    --filter=*) FILTER="${arg#*=}" ;;
  esac
done

echo "=== Starting ERP test stack ==="
cd "$ROOT_DIR"
docker compose -f docker-compose.test.yml up db api frontend -d --build --wait

echo "=== Stack healthy. Installing Playwright dependencies ==="
cd "$E2E_DIR"
npm install --silent
npx playwright install chromium --with-deps

echo "=== Running Playwright tests ==="
ARGS="$HEADED"
if [ -n "$FILTER" ]; then
  ARGS="$ARGS tests/$FILTER"
fi

set +e
npx playwright test $ARGS
EXIT_CODE=$?
set -e

echo "=== Generating report ==="
npx playwright show-report --host 0.0.0.0 --port 9323 &

echo ""
echo "=== Test complete. Exit code: $EXIT_CODE ==="
echo "    Report: http://localhost:9323"
echo ""

echo "=== Tearing down stack ==="
cd "$ROOT_DIR"
docker compose -f docker-compose.test.yml down

exit $EXIT_CODE
