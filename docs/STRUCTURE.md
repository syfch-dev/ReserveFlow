# ReserveFlow — Solution Structure

This document defines the folder organization within the solution. The architecture follows a **Clean Architecture** approach (in the style of Bookify): a four-layer solution with feature-based subfolders.

**Important:** The Application and Infrastructure layers are currently **empty skeletons**. Subfolders are not created in advance; they are added as the relevant feature or technical requirement is developed.

## Current State

```text
ReserveFlow.sln
src/
  ReserveFlow.Api/
    Extensions/
    Properties/
    appsettings.json
    appsettings.Development.json
    Dockerfile
    Program.cs

  ReserveFlow.Application/
    DependencyInjection.cs          # empty skeleton — expands as features are added

  ReserveFlow.Domain/
    Shared/                         # Entity, AggregateRoot, ValueObject, IDomainEvent
    # no feature folders yet — they will be added as needed

  ReserveFlow.Infrastructure/
    DependencyInjection.cs          # empty skeleton — expands as implementations are added

tests/
  ReserveFlow.Booking.Tests/
  ReserveFlow.Architecture.Tests/

docs/
  PROJECT.md
  DOMAIN.md
  NFR.md
  STRUCTURE.md
  adr/
```

## Target Structure (Reference)

The tree below shows the **target organization**. Not all folders need to be created upfront; only the folder relevant to the component currently being developed is created.

```text
src/
  ReserveFlow.Api/
    Controllers/                    # when an endpoint is added
    Extensions/
    Middleware/                     # when middleware is added
    OpenApi/                        # when OpenAPI customization is required
    Program.cs

  ReserveFlow.Application/
    Abstractions/                   # when port interfaces are required
    {Feature}/                      # Users, Catalog, Bookings, etc.
    Exceptions/                     # when application exceptions are required
    DependencyInjection.cs

  ReserveFlow.Domain/
    Abstractions/                   # when repository interfaces are required
    {Feature}/                      # Users, Catalog, Bookings, etc.
    Shared/                         # ✓ existing

  ReserveFlow.Infrastructure/
    Authentication/                 # when JWT is added
    Authorization/                  # when RBAC is added
    Caching/                        # when Redis is added
    Clock/                          # when testable time is required
    Configurations/                 # when EF Core configuration is added
    Data/                           # when DbContext is added
    Email/                          # when mock email is added
    Migrations/                     # when a migration is created
    Outbox/                         # when the outbox pattern is added
    Repositories/                   # when repository implementations are added
    DependencyInjection.cs
```

## Layer Responsibilities

| Layer | Responsibility | Dependency |
|--------|------------|------------|
| **Api** | HTTP, middleware, OpenAPI, DI composition root | Infrastructure |
| **Application** | Use cases, commands/queries, port definitions | Domain |
| **Domain** | Entities, value objects, domain events, business rules | None |
| **Infrastructure** | EF Core, JWT, cache, email, and outbox implementations | Application |

## Feature Folders

The Domain and Application layers are organized **feature-first**. Each feature folder represents its corresponding bounded context:

| Folder | Bounded Context | Aggregate Roots |
|--------|-----------------|-------------------|
| `Users/` | Identity | User |
| `Catalog/` | Catalog | Event, OrganizerProfile |
| `Scheduling/` | Scheduling | Provider, Appointment |
| `Bookings/` | Booking | Order, Reservation |
| `Payments/` | Payment | Payment |
| `Notifications/` | Notification | OutboxMessage |

Typical internal structure when a feature folder is added:

```text
Domain/Bookings/
  Order.cs
  Reservation.cs

Application/Bookings/
  CreateReservation/
    CreateReservationCommand.cs
    CreateReservationCommandHandler.cs
```

## Infrastructure Organization

Infrastructure is organized by technical concerns rather than by feature. Folders are created only when the corresponding implementation is written:

- **Authentication / Authorization** — JWT, RBAC
- **Data / Configurations / Migrations** — PostgreSQL + EF Core
- **Repositories** — Domain repository implementations
- **Outbox** — Reliable asynchronous notifications
- **Email** — Mock email adapter
- **Caching** — Redis (Phase 3+)
- **Clock** — Testable time abstraction

## Reference Rules

```text
Api  →  Infrastructure  →  Application  →  Domain
```

- Domain does not reference any project.
- Application references only Domain.
- Infrastructure implements Application ports.
- Api references only Infrastructure (composition root).

## Namespace Convention

```text
ReserveFlow.Domain.Bookings
ReserveFlow.Application.Bookings.CreateReservation
ReserveFlow.Infrastructure.Repositories
ReserveFlow.Api.Controllers
```

## New Feature Checklist

1. Create the `Domain/{Feature}/` folder and add entities and value objects
2. Create the `Application/{Feature}/` folder and add command/query handlers
3. When needed, create the relevant technical folder under `Infrastructure/` (e.g., `Repositories/`, `Data/`)
4. Create the `Api/Controllers/` folder (if it does not yet exist) and add the endpoint
5. Add the new service registrations to the `DependencyInjection.cs` files
6. Update the relevant NFR test evidence (`docs/NFR.md`)
