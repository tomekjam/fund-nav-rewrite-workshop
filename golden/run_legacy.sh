#!/usr/bin/env bash
# Characterization runner: run the legacy module and diff its output against
# the golden files. Exit 0 only if the legacy behaviour is unchanged.
#
# Prereq: the database is up and seeded:
#     docker compose -f db/docker-compose.yml up -d
# Run from the repository root:
#     ./golden/run_legacy.sh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

OUT_DIR="$REPO_ROOT/out"
echo "==> Running legacy module..."
dotnet run --project legacy -- "$OUT_DIR"

status=0

echo "==> Comparing NAV per unit against golden/expected_nav.csv"
if diff -u "golden/expected_nav.csv" "$OUT_DIR/nav.csv"; then
  echo "    NAV: MATCH"
else
  echo "    NAV: DIFFERENCES (see above)"
  status=1
fi

echo "==> Comparing daily fees against golden/expected_fees.csv"
if diff -u "golden/expected_fees.csv" "$OUT_DIR/fees.csv"; then
  echo "    FEES: MATCH"
else
  echo "    FEES: DIFFERENCES (see above)"
  status=1
fi

if [ "$status" -eq 0 ]; then
  echo "==> OK: legacy output matches the characterization baseline."
else
  echo "==> FAIL: legacy output diverged from the baseline."
fi
exit "$status"
