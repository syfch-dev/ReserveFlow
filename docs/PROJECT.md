# ReserveFlow — Proje Tanımı

## Amaç

ReserveFlow, **online etkinlik biletleme** ve **randevu** işlevlerini bir arada sunan bir platformdur.

Asıl hedef feature sayısını büyütmek değil; aşağıdaki iki alanda pratik yapmaktır:

1. **Domain-Driven Design (DDD)** — bounded context, aggregate, domain event, anti-corruption layer
2. **Non-Functional Requirements (NFR)** — ölçülebilir hedefler ve test kanıtı

Bu proje bir ürün MVP'si kadar, bir **mimari laboratuvar** kadar önemlidir.

Use case kataloğu (aktörler, dikey dilimler, NFR/faz eşlemesi): [`docs/USE_CASES.md`](USE_CASES.md).

## Teknoloji

- **Runtime:** .NET (ASP.NET Core)
- **Veritabanı:** PostgreSQL
- **Cache:** Redis (Faz 3+)
- **Mesajlaşma:** Outbox + message broker (RabbitMQ veya in-process queue ile başla)
- **Gözlemlenebilirlik:** OpenTelemetry + Prometheus + Grafana (Faz 5+)
- **Load test:** k6 (Faz 6+)

## İş Modeli

Platform iki rezervasyon tipini destekler:

| Tip | Örnek | Temel kural |
|-----|-------|-------------|
| **Event** | Konser, workshop, konferans | Kontenjan bazlı bilet satışı, çift satış engeli |
| **Appointment** | Danışman, doktor, kuaför | Slot bazlı rezervasyon, overlap engeli |

## Bounded Context'ler

```text
Identity      → Kullanıcı, rol, kimlik doğrulama
Catalog       → Etkinlik, venue, bilet tipi
Scheduling    → Provider, müsaitlik, randevu
Booking       → Rezervasyon, sipariş, bilet
Payment       → Tahsilat, iade (simülasyon)
Notification  → E-posta/SMS mock, outbox
```

Context'ler arası doğrudan entity paylaşımı yoktur. İletişim **ID referansı**, **DTO** ve **domain event** ile yapılır.

## MVP Kapsamı (Dahil)

### Identity
- Kayıt / giriş (JWT)
- Roller: `Customer`, `Organizer`, `Provider`, `Admin`
- RBAC ile endpoint koruması

### Catalog (Etkinlik)
- Organizer profili
- Venue (ad, kapasite, adres)
- Etkinlik oluşturma ve listeleme
- Bilet tipi (Early Bird, VIP vb.) + kontenjan + satış penceresi

### Scheduling (Randevu)
- Provider profili (uzmanlık, varsayılan süre)
- Haftalık müsaitlik tanımı
- Slot rezervasyonu (overlap engeli)
- İptal / yeniden planlama (basit kural: 24 saat öncesine kadar)

### Booking
- Rezervasyon oluşturma (type: `Event` | `Appointment`)
- Sipariş akışı: `PENDING` → `CONFIRMED` → `CANCELLED` | `EXPIRED`
- Idempotency key (aynı isteğin iki kez işlenmemesi)
- Bilet kodu / QR üretimi (event sonrası)

### Payment
- `PaymentGateway` port + fake adapter
- Başarılı / başarısız / timeout senaryoları
- Gerçek PSP entegrasyonu yok

### Notification
- E-posta mock (log veya dosyaya yaz)
- Outbox pattern ile async gönderim

### Admin
- Etkinlik ve randevu listeleme
- Basit rapor: satış adedi, doluluk oranı

## MVP Dışı (Phase 1'de Yapma)

- Koltuk haritası (seat map)
- Gerçek ödeme (Stripe, Iyzico vb.)
- Karmaşık fiyatlandırma ve kampanya motoru
- Multi-tenant (SaaS)
- Bekleme listesi (waitlist)
- Mobil uygulama
- Çoklu para birimi
- Gerçek SMS/e-posta entegrasyonu
- WebSocket ile canlı kuyruk ekranı

## Solution Yapısı (Hedef)

Clean Architecture — 4 katmanlı proje, feature bazlı alt klasörler. Detay: `docs/STRUCTURE.md`

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

| Terim | Anlam |
|-------|-------|
| **Event** | Belirli tarih/saatte gerçekleşen toplu etkinlik |
| **TicketType** | Bir etkinliğe ait fiyat + kontenjan tanımı |
| **Ticket** | Onaylanmış sipariş sonrası üretilen bilet kaydı |
| **Provider** | Randevu veren hizmet sağlayıcı |
| **TimeSlot** | Rezerve edilebilir zaman dilimi |
| **Appointment** | Provider + müşteri + slot bağlantısı |
| **Reservation** | Event veya Appointment için genel rezervasyon kaydı |
| **Order** | Ödeme ve onay yaşam döngüsünü yöneten sipariş |
| **OutboxMessage** | Güvenilir async bildirim için kuyruklanan domain event |

## Scope Guardrails

1. Yeni feature ancak bir NFR'yi test edecekse eklenir.
2. Her bounded context'te en fazla 2 aggregate root.
3. Her sprint sonunda ölçüm kanıtı (grafik, log veya test raporu) zorunludur.

## Faz Planı

| Faz | Odak | Çıktı |
|-----|------|-------|
| F1 | DDD iskelet + CRUD | Solution yapısı, domain model, unit test |
| F2 | Security NFR | JWT, RBAC, rate limit, validation |
| F3 | Performance NFR | Redis cache, DB index, pagination |
| F4 | Reliability NFR | Outbox, retry, idempotency |
| F5 | Observability NFR | Tracing, metrics, dashboard |
| F6 | Scalability NFR | k6 load test, horizontal scale |
| F7 | Availability NFR | Health check, backup/restore drill |

## Başarı Kriterleri

Proje tamamlandığında elde olunması gerekenler:

- [ ] Bounded context haritası ve domain model diyagramı
- [ ] NFR matrisi (hedef → tasarım → test kanıtı)
- [ ] Load test raporu (en az 1 kritik endpoint)
- [ ] Observability dashboard (latency, error rate, throughput)
- [ ] Runbook (deploy, rollback, backup restore)
- [ ] En az 5 ADR (Architecture Decision Record)
