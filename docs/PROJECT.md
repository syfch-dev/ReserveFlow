# ReserveFlow — Project Definition

## Purpose

ReserveFlow is a platform that combines **online event ticketing** and **appointment scheduling** capabilities.

The primary goal is not to increase the number of features, but to gain practical experience in the following two areas:

1. **Domain-Driven Design (DDD)** — bounded context, aggregate root, domain event, anti-corruption layer
2. **Non-Functional Requirements (NFRs)** — measurable targets and verification evidence

This project is as much an **architecture laboratory** as it is a product MVP.

Use case catalog (actors, vertical slices, NFR/phase mapping): [`docs/USE_CASES.md`](USE_CASES.md).

## Technology

- **Runtime:** .NET (ASP.NET Core)
- **Database:** PostgreSQL
- **Cache:** Redis (Phase 3+)
- **Messaging:** Outbox + message broker (start with RabbitMQ or an in-process queue)
- **Observability:** OpenTelemetry + Prometheus + Grafana (Phase 5+)
- **Load testing:** k6 (Phase 6+)

## Business Model

The platform supports two reservation types:

| Type | Example | Core rule |
|------|---------|-----------|
| **Event** | Concert, workshop, conference | Capacity-based ticket sales with prevention of double-selling |
| **Appointment** | Consultant, physician, hairdresser | Time-slot-based reservations with overlap prevention |

## Bounded Contexts

```text
Identity      → User, role, authentication
Catalog       → Event, venue, ticket type
Scheduling    → Provider, availability, appointment
Booking       → Reservation, order, ticket
Payment       → Payment collection, refund (simulation)
Notification  → Email/SMS mock, outbox
```

Entities are not shared directly across bounded contexts. Communication uses **ID references**, **DTOs**, and **domain events**.

## MVP Scope (Included)

### Identity
- Registration / sign-in (JWT)
- Roles: `Customer`, `Organizer`, `Provider`, `Admin`
- Endpoint protection with RBAC

### Catalog (Event)
- Organizer profile
- Venue (name, capacity, address)
- Event creation and listing
- Ticket type (Early Bird, VIP, etc.) + capacity + sales window

### Scheduling (Appointment)
- Provider profile (specialty, default duration)
- Weekly availability definition
- Time-slot reservation (overlap prevention)
- Cancellation / rescheduling (simple rule: permitted up to 24 hours in advance)

### Booking
- Reservation creation (type: `Event` | `Appointment`)
- Order flow: `PENDING` → `CONFIRMED` → `CANCELLED` | `EXPIRED`
- Idempotency key (prevents the same request from being processed twice)
- Ticket code / QR generation (after the event)

### Payment
- `PaymentGateway` port + fake adapter
- Success / failure / timeout scenarios
- No real PSP integration

### Notification
- Email mock (write to a log or file)
- Asynchronous delivery using the Outbox pattern

### Admin
- Event and appointment listing
- Basic report: number of sales, occupancy rate

## Out of MVP Scope (Do Not Implement in Phase 1)

- Seat map
- Real payment processing (Stripe, Iyzico, etc.)
- Complex pricing and promotion engine
- Multi-tenancy (SaaS)
- Waitlist
- Mobile application
- Multiple currencies
- Real SMS/email integration
- Live queue display using WebSocket

## Solution Structure (Target)

Clean Architecture — a four-layer solution with feature-based subdirectories. Details: `docs/STRUCTURE.md`

```text
ReserveFlow.sln
src/
  ReserveFlow.Api/
  ReserveFlow.Application/
  ReserveFlow.Domain/
  ReserveFlow.Infrastructure/
tests/
  ReserveFlow.Booking.Tests/
  ReserveFlow.Architecture.Tests/
docs/
  PROJECT.md
  DOMAIN.md
  NFR.md
  STRUCTURE.md
  USE_CASES.md
  adr/
```

## Ubiquitous Language

| Term | Meaning |
|------|---------|
| **Event** | A group event held at a specific date and time |
| **TicketType** | The price + capacity definition for an event |
| **Ticket** | A ticket record generated after an order is confirmed |
| **Provider** | A service provider who offers appointments |
| **TimeSlot** | A reservable period of time |
| **Appointment** | The association between a Provider, customer, and time slot |
| **Reservation** | A general reservation record for an Event or Appointment |
| **Order** | An order that manages the payment and confirmation lifecycle |
| **OutboxMessage** | A domain event queued for reliable asynchronous notification |

## Scope Guardrails

1. A new feature is added only if it verifies an NFR.
2. Each bounded context has no more than two aggregate roots.
3. Measurement evidence (a chart, log, or test report) is required at the end of every sprint.

## Phase Plan

| Phase | Focus | Deliverable |
|-------|-------|-------------|
| F1 | DDD foundation + CRUD | Solution structure, domain model, unit tests |
| F2 | Security NFR | JWT, RBAC, rate limiting, validation |
| F3 | Performance NFR | Redis cache, database index, pagination |
| F4 | Reliability NFR | Outbox, retry, idempotency |
| F5 | Observability NFR | Tracing, metrics, dashboard |
| F6 | Scalability NFR | k6 load test, horizontal scaling |
| F7 | Availability NFR | Health check, backup/restore drill |

## Success Criteria

The following should be available when the project is complete:

- [ ] Bounded context map and domain model diagram
- [ ] NFR matrix (target → design → verification evidence)
- [ ] Load test report (for at least one critical endpoint)
- [ ] Observability dashboard (latency, error rate, throughput)
- [ ] Runbook (deployment, rollback, backup restoration)
- [ ] At least five ADRs (Architecture Decision Records)
