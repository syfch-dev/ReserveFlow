# ReserveFlow — Solution Yapısı

Bu doküman, solution içindeki klasör organizasyonunu tanımlar. Mimari **Clean Architecture** (Bookify tarzı) yaklaşımını izler: 4 katmanlı proje + feature bazlı alt klasörler.

**Önemli:** Application ve Infrastructure katmanları şu an **boş iskelet** halindedir. Alt klasörler önceden oluşturulmaz; ilgili feature veya teknik ihtiyaç geliştikçe eklenir.

## Mevcut Durum

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
    DependencyInjection.cs          # boş iskelet — feature ekledikçe genişler

  ReserveFlow.Domain/
    Shared/                         # Entity, AggregateRoot, ValueObject, IDomainEvent
    # feature klasörleri henüz yok — ihtiyaç oldukça eklenecek

  ReserveFlow.Infrastructure/
    DependencyInjection.cs          # boş iskelet — implementasyon ekledikçe genişler

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

## Hedef Yapı (Referans)

Aşağıdaki ağaç **hedef organizasyonu** gösterir. Tüm klasörlerin baştan oluşturulması gerekmez; yalnızca o an geliştirilen parça için ilgili klasör açılır.

```text
src/
  ReserveFlow.Api/
    Controllers/                    # endpoint eklendiğinde
    Extensions/
    Middleware/                     # middleware eklendiğinde
    OpenApi/                        # OpenAPI özelleştirmesi gerektiğinde
    Program.cs

  ReserveFlow.Application/
    Abstractions/                   # port arayüzleri gerektiğinde
    {Feature}/                      # Users, Catalog, Bookings vb.
    Exceptions/                     # application exception'ları gerektiğinde
    DependencyInjection.cs

  ReserveFlow.Domain/
    Abstractions/                   # repository arayüzleri gerektiğinde
    {Feature}/                      # Users, Catalog, Bookings vb.
    Shared/                         # ✓ mevcut

  ReserveFlow.Infrastructure/
    Authentication/                 # JWT eklendiğinde
    Authorization/                  # RBAC eklendiğinde
    Caching/                        # Redis eklendiğinde
    Clock/                          # test edilebilir zaman gerektiğinde
    Configurations/                 # EF Core config eklendiğinde
    Data/                           # DbContext eklendiğinde
    Email/                          # mock e-posta eklendiğinde
    Migrations/                     # migration oluşturulduğunda
    Outbox/                         # outbox pattern eklendiğinde
    Repositories/                   # repository impl eklendiğinde
    DependencyInjection.cs
```

## Katman Sorumlulukları

| Katman | Sorumluluk | Bağımlılık |
|--------|------------|------------|
| **Api** | HTTP, middleware, OpenAPI, DI composition root | Infrastructure |
| **Application** | Use case, command/query, port tanımı | Domain |
| **Domain** | Entity, value object, domain event, iş kuralı | Yok |
| **Infrastructure** | EF Core, JWT, cache, e-posta, outbox implementasyonu | Application |

## Feature Klasörleri

Domain ve Application katmanları **feature-first** organize edilir. Her feature klasörü ilgili bounded context'i temsil eder:

| Klasör | Bounded Context | Aggregate Root'lar |
|--------|-----------------|-------------------|
| `Users/` | Identity | User |
| `Catalog/` | Catalog | Event, OrganizerProfile |
| `Scheduling/` | Scheduling | Provider, Appointment |
| `Bookings/` | Booking | Order, Reservation |
| `Payments/` | Payment | Payment |
| `Notifications/` | Notification | OutboxMessage |

Feature klasörü eklendiğinde tipik iç yapı:

```text
Domain/Bookings/
  Order.cs
  Reservation.cs

Application/Bookings/
  CreateReservation/
    CreateReservationCommand.cs
    CreateReservationCommandHandler.cs
```

## Infrastructure Organizasyonu

Infrastructure teknik endişelere göre ayrılır (feature bazlı değil). Klasörler yalnızca ilgili implementasyon yazıldığında oluşturulur:

- **Authentication / Authorization** — JWT, RBAC
- **Data / Configurations / Migrations** — PostgreSQL + EF Core
- **Repositories** — Domain repository implementasyonları
- **Outbox** — Güvenilir async bildirim
- **Email** — Mock e-posta adapter
- **Caching** — Redis (Faz 3+)
- **Clock** — Test edilebilir zaman soyutlaması

## Referans Kuralları

```text
Api  →  Infrastructure  →  Application  →  Domain
```

- Domain hiçbir projeye referans vermez.
- Application yalnızca Domain'e referans verir.
- Infrastructure, Application port'larını implement eder.
- Api yalnızca Infrastructure'ı referans alır (composition root).

## Namespace Konvansiyonu

```text
ReserveFlow.Domain.Bookings
ReserveFlow.Application.Bookings.CreateReservation
ReserveFlow.Infrastructure.Repositories
ReserveFlow.Api.Controllers
```

## Yeni Feature Ekleme Checklist

1. `Domain/{Feature}/` klasörünü oluştur, entity ve value object ekle
2. `Application/{Feature}/` klasörünü oluştur, command/query handler ekle
3. Gerektiğinde `Infrastructure/` altında ilgili teknik klasörü aç (ör. `Repositories/`, `Data/`)
4. `Api/Controllers/` klasörünü oluştur (henüz yoksa), endpoint ekle
5. `DependencyInjection.cs` dosyalarına yeni servis kayıtlarını ekle
6. İlgili NFR test kanıtını güncelle (`docs/NFR.md`)
