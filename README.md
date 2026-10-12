# LoanHub

PostgreSQL + Kafka (Redpanda) ile **event-driven mikroservis** öğrenme projesi — **.NET 8 / ASP.NET Core**.

Akış: kredi kullandır → Outbox → Kafka → Muhasebe (Inbox + fiş). **CQRS** + MediatR (`Commands/` / `Queries/`).

Mimari: [docs/MIMARI.md](docs/MIMARI.md)

## Gereksinimler

- Docker Desktop (veya Docker + Compose)
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (yerel geliştirme için)

## Hızlı başlangıç

```bash
docker compose --profile app up -d --build
chmod +x scripts/demo.sh && ./scripts/demo.sh
```

Yerel:

```bash
make infra-up
make run-loan        # ayrı terminal
make run-accounting
./scripts/demo.sh
```

`LoanHub.sln` — Visual Studio / Rider.

## Proje yapısı

```
LoanHub/
├── LoanHub.sln
├── src/
│   ├── LoanHub.Events/
│   ├── LoanHub.Loan.{Domain,Application,Infrastructure,Api}/
│   └── LoanHub.Accounting.{Domain,Application,Infrastructure,Api}/
├── docker-compose.yml
└── docs/MIMARI.md
```

## API

```bash
curl -s -X POST http://localhost:8081/api/loans \
  -H 'Content-Type: application/json' \
  -d '{"customerId":"CUST-1","amountCents":100000,"currency":"TRY"}'

curl -s -X POST http://localhost:8081/api/loans/<LOAN_ID>/disburse

curl -s http://localhost:8082/api/journal/by-loan/<LOAN_ID>
```

## Yapılandırma

| Ortam değişkeni | Açıklama |
|-----------------|----------|
| `ConnectionStrings__Default` | PostgreSQL |
| `Kafka__BootstrapServers` | Örn. `localhost:19092` |
| `Kafka__Topic` | `loan.events` |
| `ASPNETCORE_URLS` | Docker: `http://+:8081` / `http://+:8082` |
