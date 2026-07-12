# ReserveFlow — Non-Functional Requirements

Her NFR aşağıdaki formatta tanımlanır:

| Alan | Açıklama |
|------|----------|
| **ID** | Benzersiz kimlik |
| **Gereksinim** | Ölçülebilir hedef |
| **Tasarım kararı** | Nasıl sağlanacak |
| **Doğrulama** | Test yöntemi ve kanıt |
| **Trade-off** | Bilinçli ödün |

Durum: `Planned` | `InProgress` | `Verified` | `Failed`

---

## Faz 1 — Maintainability & Testability

### NFR-M01: Katmanlı mimari

| Alan | Değer |
|------|-------|
| **Gereksinim** | Domain katmanı infrastructure bağımlılığı içermez |
| **Tasarım** | Clean Architecture, dependency rule |
| **Doğrulama** | `ReserveFlow.Architecture.Tests` — NetArchTest ile katman kuralları |
| **Trade-off** | Daha fazla proje/boilerplate |
| **Durum** | Verified |

### NFR-M02: Unit test coverage

| Alan | Değer |
|------|-------|
| **Gereksinim** | Domain ve application katmanlarında ≥ %70 line coverage |
| **Tasarım** | xUnit + FluentAssertions, aggregate iş kuralları test edilir |
| **Doğrulama** | `dotnet test --collect:"XPlat Code Coverage"` |
| **Trade-off** | Integration test sayısı başlangıçta düşük kalır |
| **Durum** | Planned |

### NFR-M03: API contract stability

| Alan | Değer |
|------|-------|
| **Gereksinim** | Public API endpoint'leri versioned (`/api/v1/`) |
| **Tasarım** | URL path versioning |
| **Doğrulama** | OpenAPI spec + breaking change checklist |
| **Trade-off** | Version maintenance overhead |
| **Durum** | Planned |

---

## Faz 2 — Security

### NFR-S01: Authentication

| Alan | Değer |
|------|-------|
| **Gereksinim** | Tüm protected endpoint'ler JWT gerektirir; token süresi ≤ 60 dk |
| **Tasarım** | ASP.NET Core JWT Bearer |
| **Doğrulama** | Unauthorized request → 401; expired token → 401 |
| **Trade-off** | Refresh token flow MVP'de basit tutulur |
| **Durum** | Planned |

### NFR-S02: Authorization (RBAC)

| Alan | Değer |
|------|-------|
| **Gereksinim** | Role-based erişim; Customer başkasının siparişini göremez |
| **Tasarım** | Policy-based authorization, role claims |
| **Doğrulama** | Integration test: forbidden → 403 |
| **Trade-off** | Fine-grained permission MVP'de yok |
| **Durum** | Planned |

### NFR-S03: Rate limiting

| Alan | Değer |
|------|-------|
| **Gereksinim** | Public endpoint'ler: 100 req/dk/IP; booking endpoint: 10 req/dk/user |
| **Tasarım** | ASP.NET Core Rate Limiting middleware |
| **Doğrulama** | Load test ile limit aşımında 429 |
| **Trade-off** | Shared IP (NAT) senaryosunda false positive |
| **Durum** | Planned |

### NFR-S04: Input validation

| Alan | Değer |
|------|-------|
| **Gereksinim** | Tüm input validate edilir; SQL injection / XSS vektörleri engellenir |
| **Tasarım** | FluentValidation + parameterized queries |
| **Doğrulama** | OWASP top 10 checklist + negative test cases |
| **Trade-off** | — |
| **Durum** | Planned |

### NFR-S05: Secrets management

| Alan | Değer |
|------|-------|
| **Gereksinim** | Secret'lar kodda veya git'te bulunmaz |
| **Tasarım** | User Secrets (dev), environment variables (prod) |
| **Doğrulama** | Gitleaks scan, `.env` gitignore |
| **Trade-off** | — |
| **Durum** | Planned |

---

## Faz 3 — Performance

### NFR-P01: API response latency

| Alan | Değer |
|------|-------|
| **Gereksinim** | Read endpoint'leri p95 < 200ms; write (booking) p95 < 300ms |
| **Tasarım** | Redis cache (event list), DB index, connection pooling |
| **Doğrulama** | k6 load test, Grafana p95 panel |
| **Trade-off** | Cache invalidation complexity |
| **Durum** | Planned |

### NFR-P02: Pagination

| Alan | Değer |
|------|-------|
| **Gereksinim** | List endpoint'leri cursor veya offset pagination zorunlu; default page size ≤ 20 |
| **Tasarım** | `PaginatedResult<T>` response wrapper |
| **Doğrulama** | 10.000 kayıt ile response time < 200ms |
| **Trade-off** | Offset pagination deep page'lerde yavaşlar |
| **Durum** | Planned |

### NFR-P03: Database query efficiency

| Alan | Değer |
|------|-------|
| **Gereksinim** | N+1 query yok; kritik sorgular ≤ 50ms |
| **Tasarım** | EF Core Include/ projection, composite index |
| **Doğrulama** | EF Core logging + slow query log |
| **Trade-off** | — |
| **Durum** | Planned |

---

## Faz 4 — Reliability

### NFR-R01: Zero double-booking

| Alan | Değer |
|------|-------|
| **Gereksinim** | Aynı son bilet veya slot için çift rezervasyon = 0 |
| **Tasarım** | Optimistic concurrency (row version) + DB unique constraint |
| **Doğrulama** | 100 eşzamanlı booking isteği, 1 kontenjan → tam 1 başarı |
| **Trade-off** | Concurrency conflict → retry gerekir |
| **Durum** | Planned |

### NFR-R02: Idempotency

| Alan | Değer |
|------|-------|
| **Gereksinim** | Aynı `Idempotency-Key` ile gelen istek aynı sonucu döner; çift charge yok |
| **Tasarım** | Idempotency key store (Redis veya DB) |
| **Doğrulama** | Aynı key ile 5 paralel istek → 1 order, 4 cached response |
| **Trade-off** | Key storage TTL yönetimi |
| **Durum** | Planned |

### NFR-R03: Order expiration

| Alan | Değer |
|------|-------|
| **Gereksinim** | Pending order max 15 dk sonra `EXPIRED`; quota geri açılır |
| **Tasarım** | Background job / hosted service |
| **Doğrulama** | Order oluştur → 15 dk bekle → status Expired, quota restored |
| **Trade-off** | Eventual consistency (birkaç saniye gecikme kabul edilir) |
| **Durum** | Planned |

### NFR-R04: Outbox pattern

| Alan | Değer |
|------|-------|
| **Gereksinim** | Domain event kaybı = 0; en az once delivery |
| **Tasarım** | Transactional outbox + background processor |
| **Doğrulama** | DB commit + crash simülasyonu → event eventually processed |
| **Trade-off** | At-least-once → consumer idempotent olmalı |
| **Durum** | Planned |

### NFR-R05: Payment timeout handling

| Alan | Değer |
|------|-------|
| **Gereksinim** | Payment gateway timeout (5s) → order failed, kullanıcı bilgilendirilir |
| **Tasarım** | Polly timeout policy + circuit breaker |
| **Doğrulama** | Fake gateway delay=10s → timeout, order cancelled |
| **Trade-off** | Aggressive timeout → false failure |
| **Durum** | Planned |

---

## Faz 5 — Observability

### NFR-O01: Structured logging

| Alan | Değer |
|------|-------|
| **Gereksinim** | Tüm log'lar structured JSON; correlation ID her request'te |
| **Tasarım** | Serilog + enrichers |
| **Doğrulama** | Log output'ta `CorrelationId`, `UserId`, `OrderId` alanları |
| **Trade-off** | Log volume artışı |
| **Durum** | Planned |

### NFR-O02: Distributed tracing

| Alan | Değer |
|------|-------|
| **Gereksinim** | Request → DB → outbox → notification trace edilebilir |
| **Tasarım** | OpenTelemetry + Jaeger veya Grafana Tempo |
| **Doğrulama** | Booking flow trace'de ≥ 4 span görünür |
| **Trade-off** | Overhead ~%1-3 latency |
| **Durum** | Planned |

### NFR-O03: Metrics & alerting

| Alan | Değer |
|------|-------|
| **Gereksinim** | request_count, error_rate, latency_histogram metrikleri |
| **Tasarım** | Prometheus + Grafana dashboard |
| **Doğrulama** | Dashboard'da real-time metrik; error rate > %5 alert |
| **Trade-off** | Alert fatigue riski → threshold dikkatli ayarlanır |
| **Durum** | Planned |

### NFR-O04: Audit trail

| Alan | Değer |
|------|-------|
| **Gereksinim** | Kritik işlemler (order confirm, cancel, refund) audit log'a yazılır |
| **Tasarım** | AuditLog entity, append-only |
| **Doğrulama** | Order confirm → audit entry: who, what, when |
| **Trade-off** | Storage büyümesi |
| **Durum** | Planned |

---

## Faz 6 — Scalability & Resilience

### NFR-SC01: Horizontal scalability

| Alan | Değer |
|------|-------|
| **Gereksinim** | 2 instance ile throughput ≥ 1.5x artış (linear'a yakın) |
| **Tasarım** | Stateless API, shared Redis/PostgreSQL |
| **Doğrulama** | k6: 1 pod vs 2 pod RPS karşılaştırması |
| **Trade-off** | Session/state external store gerekir |
| **Durum** | Planned |

### NFR-SC02: Throughput target

| Alan | Değer |
|------|-------|
| **Gereksinim** | Booking endpoint ≥ 200 RPS (2 instance, p95 < 500ms) |
| **Tasarım** | Connection pool tuning, async I/O |
| **Doğrulama** | k6 sustained load test raporu |
| **Trade-off** | — |
| **Durum** | Planned |

### NFR-SC03: Circuit breaker

| Alan | Değer |
|------|-------|
| **Gereksinim** | Payment gateway 5 ardışık hata → circuit open, 30s cooldown |
| **Tasarım** | Polly circuit breaker |
| **Doğrulama** | Fake gateway fail mode → circuit opens, fallback response |
| **Trade-off** | Open circuit → tüm payment rejected (graceful degradation gerekir) |
| **Durum** | Planned |

---

## Faz 7 — Availability & Disaster Recovery

### NFR-A01: Uptime target

| Alan | Değer |
|------|-------|
| **Gereksinim** | API availability ≥ %99.9 (aylık, planned maintenance hariç) |
| **Tasarım** | Health checks, readiness/liveness probes |
| **Doğrulama** | Uptime monitoring (30 gün) |
| **Trade-off** | Multi-AZ deployment maliyeti |
| **Durum** | Planned |

### NFR-A02: Health checks

| Alan | Değer |
|------|-------|
| **Gereksinim** | `/health` DB + Redis + outbox processor durumunu raporlar |
| **Tasarım** | ASP.NET Core Health Checks |
| **Doğrulama** | DB down → `/health` unhealthy → 503 |
| **Trade-off** | — |
| **Durum** | Planned |

### NFR-A03: Backup & restore

| Alan | Değer |
|------|-------|
| **Gereksinim** | RPO ≤ 1 saat, RTO ≤ 4 saat |
| **Tasarım** | PostgreSQL daily backup + WAL; restore drill |
| **Doğrulama** | Backup al → DB sil → restore → data integrity check |
| **Trade-off** | Backup storage maliyeti |
| **Durum** | Planned |

---

## NFR Matris Özeti

| ID | Kategori | Hedef | Faz | Durum |
|----|----------|-------|-----|-------|
| NFR-M01 | Maintainability | Katman kuralları | F1 | Planned |
| NFR-M02 | Testability | ≥ %70 coverage | F1 | Planned |
| NFR-M03 | Maintainability | API versioning | F1 | Planned |
| NFR-S01 | Security | JWT auth | F2 | Planned |
| NFR-S02 | Security | RBAC | F2 | Planned |
| NFR-S03 | Security | Rate limit | F2 | Planned |
| NFR-S04 | Security | Input validation | F2 | Planned |
| NFR-S05 | Security | Secrets management | F2 | Planned |
| NFR-P01 | Performance | p95 < 300ms | F3 | Planned |
| NFR-P02 | Performance | Pagination | F3 | Planned |
| NFR-P03 | Performance | No N+1 | F3 | Planned |
| NFR-R01 | Reliability | Zero double-book | F4 | Planned |
| NFR-R02 | Reliability | Idempotency | F4 | Planned |
| NFR-R03 | Reliability | Order expiration | F4 | Planned |
| NFR-R04 | Reliability | Outbox | F4 | Planned |
| NFR-R05 | Reliability | Payment timeout | F4 | Planned |
| NFR-O01 | Observability | Structured logs | F5 | Planned |
| NFR-O02 | Observability | Tracing | F5 | Planned |
| NFR-O03 | Observability | Metrics | F5 | Planned |
| NFR-O04 | Observability | Audit trail | F5 | Planned |
| NFR-SC01 | Scalability | Horizontal scale | F6 | Planned |
| NFR-SC02 | Scalability | 200 RPS | F6 | Planned |
| NFR-SC03 | Resilience | Circuit breaker | F6 | Planned |
| NFR-A01 | Availability | %99.9 uptime | F7 | Planned |
| NFR-A02 | Availability | Health checks | F7 | Planned |
| NFR-A03 | DR | RPO/RTO | F7 | Planned |

---

## Test Araçları

| Araç | Kullanım |
|------|----------|
| **xUnit + FluentAssertions** | Unit / integration test |
| **NetArchTest** | Architecture rule test |
| **k6** | Load / stress test |
| **Prometheus + Grafana** | Metrics dashboard |
| **OpenTelemetry + Jaeger** | Distributed tracing |
| **Gitleaks** | Secret scanning |

---

## Sprint Sonu Kontrol Listesi

Her sprint bitiminde:

- [ ] Hedeflenen NFR'lerin durumu güncellendi mi?
- [ ] Doğrulama kanıtı (test raporu, screenshot, log) eklendi mi?
- [ ] Trade-off'lar ADR olarak dokümante edildi mi?
- [ ] Regresyon: önceki faz NFR'leri hâlâ geçiyor mu?
