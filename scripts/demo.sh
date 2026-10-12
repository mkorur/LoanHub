#!/usr/bin/env bash
set -euo pipefail

BASE_LOAN="${LOAN_URL:-http://localhost:8081}"
BASE_ACC="${ACCOUNTING_URL:-http://localhost:8082}"

echo "1) Yeni kredi oluştur..."
LOAN=$(curl -s -X POST "$BASE_LOAN/api/loans" \
  -H 'Content-Type: application/json' \
  -d '{"customerId":"CUST-1001","amountCents":25000000,"currency":"TRY"}')
LOAN_ID=$(echo "$LOAN" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
echo "   loanId=$LOAN_ID"

echo "2) Kredi kullandır (outbox)..."
curl -s -X POST "$BASE_LOAN/api/loans/$LOAN_ID/disburse" | python3 -m json.tool

echo "3) Outbox → Kafka → muhasebe için bekleniyor..."
for _ in 1 2 3 4 5 6; do
  if curl -sf "$BASE_ACC/health" >/dev/null; then break; fi
  sleep 2
done
sleep 5

echo "4) Muhasebe fişleri:"
curl -s "$BASE_ACC/api/journal/by-loan/$LOAN_ID" | python3 -m json.tool
