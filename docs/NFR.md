# ReserveFlow — Non-Functional Requirements

Each NFR is defined in the following format:

| Field | Description |
|------|----------|
| **ID** | Unique identifier |
| **Requirement** | Measurable target |
| **Design decision** | How it will be achieved |
| **Verification** | Test method and evidence |
| **Trade-off** | Conscious compromise |

Status: `Planned` | `InProgress` | `Verified` | `Failed`

---

## Phase 1 — Maintainability & Testability

### NFR-M01: Layered architecture

| Field | Value |
|------|-------|
| **Requirement** | Domain layer has no infrastructure dependency |
| **Design** | Clean Architecture, dependency rule |
| **Verification** | `ReserveFlow.Architecture.Tests` — layer rules via NetArchTest |
| **Trade-off** | More projects/boilerplate |
| **Status** | Verified |

### NFR-M02: Unit test coverage

| Field | Value |
|------|-------|
| **Requirement** | ≥ 70% line coverage in domain and application layers |
| **Design** | xUnit + FluentAssertions; aggregate business rules are tested |
| **Verification** | `dotnet test --collect:"XPlat Code Coverage"` |
| **Trade-off** | Integration test count remains low initially |
| **Status** | Planned |

### NFR-M03: API contract stability

| Field | Value |
|------|-------|
| **Requirement** | Public API endpoints are versioned (`/api/v1/`) |
| **Design** | URL path versioning |
| **Verification** | OpenAPI spec + breaking change checklist |
| **Trade-off** | Version maintenance overhead |
| **Status** | Planned |

---

## Phase 2 — Security

### NFR-S01: Authentication

| Field | Value |
|------|-------|
| **Requirement** | All protected endpoints require JWT; token lifetime ≤ 60 min |
| **Design** | ASP.NET Core JWT Bearer |
| **Verification** | Unauthorized request → 401; expired token → 401 |
| **Trade-off** | Refresh token flow is kept simple in MVP |
| **Status** | Planned |

### NFR-S02: Authorization (RBAC)

| Field | Value |
|------|-------|
| **Requirement** | Role-based access; Customer cannot view another customer's order |
| **Design** | Policy-based authorization, role claims |
| **Verification** | Integration test: forbidden → 403 |
| **Trade-off** | Fine-grained permission not in MVP |
| **Status** | Planned |

### NFR-S03: Rate limiting

| Field | Value |
|------|-------|
| **Requirement** | Public endpoints: 100 req/min/IP; booking endpoint: 10 req/min/user |
| **Design** | ASP.NET Core Rate Limiting middleware |
| **Verification** | Load test yields 429 when limit is exceeded |
| **Trade-off** | False positives in shared IP (NAT) scenarios |
| **Status** | Planned |

### NFR-S04: Input validation

| Field | Value |
|------|-------|
| **Requirement** | All input is validated; SQL injection / XSS vectors are blocked |
| **Design** | FluentValidation + parameterized queries |
| **Verification** | OWASP top 10 checklist + negative test cases |
| **Trade-off** | — |
| **Status** | Planned |

### NFR-S05: Secrets management

| Field | Value |
|------|-------|
| **Requirement** | Secrets are not present in code or git |
| **Design** | User Secrets (dev), environment variables (prod) |
| **Verification** | Gitleaks scan, `.env` gitignore |
| **Trade-off** | — |
| **Status** | Planned |

---

## Phase 3 — Performance

### NFR-P01: API response latency

| Field | Value |
|------|-------|
| **Requirement** | Read endpoints p95 < 200ms; write (booking) p95 < 300ms |
| **Design** | Redis cache (event list), DB index, connection pooling |
| **Verification** | k6 load test, Grafana p95 panel |
| **Trade-off** | Cache invalidation complexity |
| **Status** | Planned |

### NFR-P02: Pagination

| Field | Value |
|------|-------|
| **Requirement** | List endpoints require cursor or offset pagination; default page size ≤ 20 |
| **Design** | `PaginatedResult<T>` response wrapper |
| **Verification** | Response time < 200ms with 10,000 records |
| **Trade-off** | Offset pagination slows down on deep pages |
| **Status** | Planned |

### NFR-P03: Database query efficiency

| Field | Value |
|------|-------|
| **Requirement** | No N+1 queries; critical queries ≤ 50ms |
| **Design** | EF Core Include/projection, composite index |
| **Verification** | EF Core logging + slow query log |
| **Trade-off** | — |
| **Status** | Planned |

---

## Phase 4 — Reliability

### NFR-R01: Zero double-booking

| Field | Value |
|------|-------|
| **Requirement** | Double reservation for the same last ticket or slot = 0 |
| **Design** | Optimistic concurrency (row version) + DB unique constraint |
| **Verification** | 100 concurrent booking requests, 1 quota → exactly 1 success |
| **Trade-off** | Concurrency conflict → retry required |
| **Status** | Planned |

### NFR-R02: Idempotency

| Field | Value |
|------|-------|
| **Requirement** | Requests with the same `Idempotency-Key` return the same result; no double charge |
| **Design** | Idempotency key store (Redis or DB) |
| **Verification** | 5 parallel requests with the same key → 1 order, 4 cached responses |
| **Trade-off** | Key storage TTL management |
| **Status** | Planned |

### NFR-R03: Order expiration

| Field | Value |
|------|-------|
| **Requirement** | Pending order becomes `EXPIRED` after max 15 min; quota is released |
| **Design** | Background job / hosted service |
| **Verification** | Create order → wait 15 min → status Expired, quota restored |
| **Trade-off** | Eventual consistency (a few seconds of delay is acceptable) |
| **Status** | Planned |

### NFR-R04: Outbox pattern

| Field | Value |
|------|-------|
| **Requirement** | Domain event loss = 0; at-least-once delivery |
| **Design** | Transactional outbox + background processor |
| **Verification** | DB commit + crash simulation → event eventually processed |
| **Trade-off** | At-least-once → consumer must be idempotent |
| **Status** | Planned |

### NFR-R05: Payment timeout handling

| Field | Value |
|------|-------|
| **Requirement** | Payment gateway timeout (5s) → order failed, user is notified |
| **Design** | Polly timeout policy + circuit breaker |
| **Verification** | Fake gateway delay=10s → timeout, order cancelled |
| **Trade-off** | Aggressive timeout → false failure |
| **Status** | Planned |

---

## Phase 5 — Observability

### NFR-O01: Structured logging

| Field | Value |
|------|-------|
| **Requirement** | All logs are structured JSON; correlation ID on every request |
| **Design** | Serilog + enrichers |
| **Verification** | `CorrelationId`, `UserId`, `OrderId` fields in log output |
| **Trade-off** | Increased log volume |
| **Status** | Planned |

### NFR-O02: Distributed tracing

| Field | Value |
|------|-------|
| **Requirement** | Request → DB → outbox → notification is traceable |
| **Design** | OpenTelemetry (OTLP) → Jaeger. ASP.NET Core + HttpClient + Npgsql auto-instrumentation; Application layer custom `ActivitySource` (`ReserveFlow.Application`) |
| **Verification** | `ReserveFlow.Api` service and request spans visible in Jaeger UI; target ≥ 4 spans when booking flow arrives |
| **Trade-off** | Overhead ~1-3% latency |
| **Status** | InProgress |

### NFR-O03: Metrics & alerting

| Field | Value |
|------|-------|
| **Requirement** | request_count, error_rate, latency_histogram metrics |
| **Design** | OpenTelemetry metrics (OTLP) → Prometheus OTLP receiver → Grafana dashboard; custom metric `reserveflow.users.registered` |
| **Verification** | `http_server_request_duration_seconds_*` series in Prometheus; request rate / error rate / p95 panels on Grafana "ReserveFlow — Overview" dashboard |
| **Trade-off** | Alert fatigue risk → thresholds set carefully |
| **Status** | InProgress |

### NFR-O04: Audit trail

| Field | Value |
|------|-------|
| **Requirement** | Critical operations (order confirm, cancel, refund) are written to the audit log |
| **Design** | AuditLog entity, append-only |
| **Verification** | Order confirm → audit entry: who, what, when |
| **Trade-off** | Storage growth |
| **Status** | Planned |

---

## Phase 6 — Scalability & Resilience

### NFR-SC01: Horizontal scalability

| Field | Value |
|------|-------|
| **Requirement** | With 2 instances, throughput ≥ 1.5x increase (near-linear) |
| **Design** | Stateless API, shared Redis/PostgreSQL |
| **Verification** | k6: 1 pod vs 2 pod RPS comparison |
| **Trade-off** | Session/state requires an external store |
| **Status** | Planned |

### NFR-SC02: Throughput target

| Field | Value |
|------|-------|
| **Requirement** | Booking endpoint ≥ 200 RPS (2 instances, p95 < 500ms) |
| **Design** | Connection pool tuning, async I/O |
| **Verification** | k6 sustained load test report |
| **Trade-off** | — |
| **Status** | Planned |

### NFR-SC03: Circuit breaker

| Field | Value |
|------|-------|
| **Requirement** | Payment gateway 5 consecutive failures → circuit open, 30s cooldown |
| **Design** | Polly circuit breaker |
| **Verification** | Fake gateway fail mode → circuit opens, fallback response |
| **Trade-off** | Open circuit → all payments rejected (graceful degradation required) |
| **Status** | Planned |

---

## Phase 7 — Availability & Disaster Recovery

### NFR-A01: Uptime target

| Field | Value |
|------|-------|
| **Requirement** | API availability ≥ 99.9% (monthly, excluding planned maintenance) |
| **Design** | Health checks, readiness/liveness probes |
| **Verification** | Uptime monitoring (30 days) |
| **Trade-off** | Multi-AZ deployment cost |
| **Status** | Planned |

### NFR-A02: Health checks

| Field | Value |
|------|-------|
| **Requirement** | `/health` reports DB + Redis + outbox processor status |
| **Design** | ASP.NET Core Health Checks |
| **Verification** | DB down → `/health` unhealthy → 503 |
| **Trade-off** | — |
| **Status** | Planned |

### NFR-A03: Backup & restore

| Field | Value |
|------|-------|
| **Requirement** | RPO ≤ 1 hour, RTO ≤ 4 hours |
| **Design** | PostgreSQL daily backup + WAL; restore drill |
| **Verification** | Take backup → delete DB → restore → data integrity check |
| **Trade-off** | Backup storage cost |
| **Status** | Planned |

---

## NFR Matrix Summary

| ID | Category | Target | Phase | Status |
|----|----------|-------|-----|-------|
| NFR-M01 | Maintainability | Layer rules | F1 | Planned |
| NFR-M02 | Testability | ≥ 70% coverage | F1 | Planned |
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
| NFR-O02 | Observability | Tracing | F5 | InProgress |
| NFR-O03 | Observability | Metrics | F5 | InProgress |
| NFR-O04 | Observability | Audit trail | F5 | Planned |
| NFR-SC01 | Scalability | Horizontal scale | F6 | Planned |
| NFR-SC02 | Scalability | 200 RPS | F6 | Planned |
| NFR-SC03 | Resilience | Circuit breaker | F6 | Planned |
| NFR-A01 | Availability | 99.9% uptime | F7 | Planned |
| NFR-A02 | Availability | Health checks | F7 | Planned |
| NFR-A03 | DR | RPO/RTO | F7 | Planned |

---

## Test Tools

| Tool | Usage |
|------|----------|
| **xUnit + FluentAssertions** | Unit / integration test |
| **NetArchTest** | Architecture rule test |
| **k6** | Load / stress test |
| **Prometheus + Grafana** | Metrics dashboard |
| **OpenTelemetry + Jaeger** | Distributed tracing |
| **Gitleaks** | Secret scanning |

---

## End-of-Sprint Checklist

At the end of each sprint:

- [ ] Were the statuses of targeted NFRs updated?
- [ ] Was verification evidence (test report, screenshot, log) added?
- [ ] Were trade-offs documented as ADRs?
- [ ] Regression: do previous-phase NFRs still pass?
