# ReserveFlow

ReserveFlow is a .NET platform for event ticketing and appointment booking. The project is designed as an architecture laboratory for practicing Domain-Driven Design, Clean Architecture, and measurable non-functional requirements.

The focus is not feature volume. Each capability should exercise a domain boundary or provide evidence for an NFR such as reliability, performance, security, or observability.

## Current status

Implemented:

- User registration with validation and password hashing
- PostgreSQL persistence through EF Core
- Versioned API routes under `/api/v1`
- OpenAPI and Swagger UI in Development
- OpenTelemetry tracing and metrics
- Jaeger, Prometheus, and Grafana development stack
- Custom registration span and metric
- Architecture and booking tests

The remaining ticketing, scheduling, payment, outbox, authentication, and authorization flows are planned incrementally. See [the project plan](docs/PROJECT.md) and [use-case catalog](docs/USE_CASES.md).

## Architecture

ReserveFlow follows Clean Architecture with feature-first organization:

```text
Api  →  Infrastructure  →  Application  →  Domain
```

```text
src/
├── ReserveFlow.Api
├── ReserveFlow.Application
├── ReserveFlow.Domain
└── ReserveFlow.Infrastructure
tests/
├── ReserveFlow.Booking.Tests
└── ReserveFlow.Architecture.Tests
```

Key rules:

- Domain has no dependency on other project layers.
- Application depends only on Domain.
- Infrastructure implements Application and Domain ports.
- API is the composition root.
- Bounded contexts communicate through IDs, DTOs, and domain events instead of sharing entities.

## Technology

- .NET 10 and ASP.NET Core
- Entity Framework Core 10
- PostgreSQL 16
- FluentValidation
- OpenTelemetry
- Jaeger for distributed traces
- Prometheus for metrics
- Grafana for dashboards
- xUnit and NetArchTest
- Docker Compose

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-docker/)
- EF Core CLI tools:

```bash
dotnet tool install --global dotnet-ef
```

### Run the infrastructure

```bash
docker compose up -d
```

This starts PostgreSQL and the local observability stack.

### Apply database migrations

```bash
dotnet ef database update \
  --project src/ReserveFlow.Infrastructure \
  --startup-project src/ReserveFlow.Api
```

### Run the API

```bash
dotnet run --project src/ReserveFlow.Api --launch-profile http
```

The API is available at `http://localhost:5189`.

Useful endpoints:

- Health: `http://localhost:5189/api/v1/health`
- Swagger UI: `http://localhost:5189/swagger`
- OpenAPI document: `http://localhost:5189/openapi/v1.json`
- User registration: `POST http://localhost:5189/api/v1/users/register`

Example registration request:

```bash
curl -X POST http://localhost:5189/api/v1/users/register \
  -H "Content-Type: application/json" \
  -d '{"email":"customer@example.com","password":"SecurePass123!"}'
```

## Observability

The API emits OpenTelemetry signals through OTLP:

```text
ReserveFlow.Api
├── traces  → Jaeger
└── metrics → Prometheus → Grafana
```

Local interfaces:

- Jaeger: [http://localhost:16686](http://localhost:16686)
- Prometheus: [http://localhost:9090](http://localhost:9090)
- Grafana: [http://localhost:3000](http://localhost:3000)

Grafana automatically provisions the Prometheus and Jaeger data sources and the `ReserveFlow — Overview` dashboard. Anonymous Admin access is enabled only for local development.

Configuration is available under the `Observability` section in the API settings. Environment variables can override nested keys by using double underscores, for example:

```bash
Observability__TracesEndpoint=http://jaeger:4317
Observability__MetricsEndpoint=http://prometheus:9090/api/v1/otlp/v1/metrics
```

## Testing

Run the complete test suite:

```bash
dotnet test ReserveFlow.sln
```

Run only architecture rules:

```bash
dotnet test tests/ReserveFlow.Architecture.Tests
```

Build the solution:

```bash
dotnet build ReserveFlow.sln
```

## Documentation

- [Project scope and phase plan](docs/PROJECT.md)
- [Domain model and bounded contexts](docs/DOMAIN.md)
- [Non-functional requirements](docs/NFR.md)
- [Solution structure](docs/STRUCTURE.md)
- [Use-case catalog](docs/USE_CASES.md)

## Development services

Stop the containers:

```bash
docker compose down
```

Stop the containers and remove local data:

```bash
docker compose down -v
```

The second command permanently deletes the local PostgreSQL, Prometheus, and Grafana volumes.

## License

No license has been added yet.
