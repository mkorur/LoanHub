# LoanHub — Mimari

## Event akışı

```
Kredi kullandırıldı
    → Loan tablosu güncellendi (loan_db)
    → Outbox'a LoanCreated yazıldı (aynı transaction)
    → Outbox worker Kafka'ya gönderdi
    → Muhasebe servisi event'i aldı
    → Inbox kontrolü (messageId = eventId)
    → Muhasebe fişi (journal_entries)
    → messageId inbox'a kaydedildi
```

## Servisler

| Servis | Port | Veritabanı | Sorumluluk |
|--------|------|------------|------------|
| **LoanHub.Loan.Api** | 8081 | `loan_db` | Kredi, Outbox |
| **LoanHub.Accounting.Api** | 8082 | `accounting_db` | Kafka, Inbox, fiş |

Servisler HTTP ile birbirini çağırmaz; yalnızca Kafka üzerinden konuşur (*database-per-service*).

## Desenler

**Transactional Outbox** — `Disburse` tek transaction: `loans` güncelleme + `outbox_events` insert. Worker Kafka'ya yazar, `published_at` işaretler.

**Inbox** — `eventId` = `messageId`; tekrarlayan mesajlarda ikinci fiş oluşturulmaz.

**Event sözleşmesi:** `src/LoanHub.Events/LoanCreatedEvent.cs`

## CQRS (MediatR)

| Tür | Kredi örnekleri | Muhasebe örnekleri |
|-----|-----------------|-------------------|
| **Command** (yazma) | `CreateLoanCommand`, `DisburseLoanCommand` | `ProcessLoanCreatedCommand` |
| **Query** (okuma) | `GetLoanQuery` | `GetJournalByLoanQuery` |

API ve Kafka consumer `IMediator.Send(...)` kullanır. Yazma/okuma portları ayrı: `ILoanWriteRepository` / `ILoanReadRepository`, `IAccountingWriteRepository` / `IAccountingReadRepository` (aynı PostgreSQL; ileride read model ayrılabilir).

## Clean Architecture

Bağımlılıklar içe doğru: **Api → Infrastructure → Application → Domain**

```
Api (HTTP, DI)
  → Infrastructure (Npgsql, Kafka, BackgroundService)
    → Application (handler'lar, ILoanRepository, IOutboxRelay, …)
      → Domain (Loan.Disburse(), JournalEntry, …)
```

| Kredi | Muhasebe |
|--------|----------|
| `LoanHub.Loan.Domain` | `LoanHub.Accounting.Domain` |
| `LoanHub.Loan.Application` | `LoanHub.Accounting.Application` |
| `LoanHub.Loan.Infrastructure` | `LoanHub.Accounting.Infrastructure` |
| `LoanHub.Loan.Api` | `LoanHub.Accounting.Api` |

**Kullandır:** Api → `DisburseLoanCommand` → `Loan.Disburse()` → `LoanRepository` → Outbox → `IOutboxRelay`

**Muhasebe:** Kafka → `ProcessLoanCreatedCommand` → `PostJournalWithInboxAsync`

## Kafka UI

http://localhost:8080 — `loan.events` topic
