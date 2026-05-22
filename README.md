# Recruitment Platform

A two-service distributed system that demonstrates event-driven communication between an **InventoryService** and a **ProductService** over RabbitMQ, with JWT-secured APIs and idempotent message processing.

```
┌──────────────────┐        ProductCreatedEvent         ┌──────────────────┐
│                  │  ────────────────────────────────▶ │                  │
│  ProductService  │                                    │ InventoryService │
│                  │ ◀──────────────────────────────────│                  │
│                  │     ProductInventoryAddedEvent     │                  │
└──────────────────┘                                    └──────────────────┘
        │                                                       │
        ▼                                                       ▼
   product DB                                              inventory DB
   (Postgres)                                              (Postgres)

                       ┌──────────────────────┐
                       │       RabbitMQ       │
                       │ (MassTransit broker) │
                       └──────────────────────┘
```

---

## Quick start

Requirements: Docker and .NET 10 SDK if you want to run tests locally.

```bash
docker compose up --build
```

That brings up:

| Service           | URL                                  |
|-------------------|--------------------------------------|
| InventoryService  | http://localhost:5001                |
| ProductService    | http://localhost:5002                |
| RabbitMQ UI       | http://localhost:15672 (guest/guest) |
| Postgres          | localhost:5432 (postgres/postgres)   |

Databases are migrated automatically on startup (`Database:AutoMigrate=true`).

Both services expose `/health` and OpenAPI at `/openapi/v1.json` when run in `Development`.

### Getting a JWT for manual testing

All endpoints (except `/health`) require a JWT bearer token. There is a helper PowerShell script that signs a dev token with the key from `.env`:

```powershell
# A token with the 'write' role
./tools/gen-token.ps1 -Role write

# Both roles
./tools/gen-token.ps1 -Role write,read
```

The token uses claim `role` and subject `dev-user` by default. Pass `-User` to change the subject.

### Manual testing with `.http` files

Both services ship with ready-to-use `.http` files that cover every endpoint:

- `product-service/src/Product.Api/Product.Api.http` — `POST /products`, `GET /products`, `GET /products/{id}`
- `inventory-service/src/Inventory.Api/Inventory.Api.http` — `POST /inventory`

Each file declares `@token` variable. After generating a fresh token with `./tools/gen-token.ps1`, paste it into `@token` and fire the requests straight from the editor.

### Event flow

1. `POST /products` -> `Product` row inserted, `ProductCreatedEvent` published.
2. InventoryService consumes `ProductCreatedEvent` and writes a row into its own `KnownProducts` projection.
3. `POST /inventory` checks `KnownProducts`, inserts an `Inventory` entry, and publishes `ProductInventoryAddedEvent`.
4. ProductService consumes `ProductInventoryAddedEvent`, increments `Product.Amount`, and records the `EventId` for idempotency.

---

## Key design decisions

### Consumer idempotency

Re-delivery is handled in `ProductInventoryAddedConsumer` by checking and writing `ProcessedEvents.EventId` inside the same transaction that updates `Product.Amount` (`product-service/.../ProductInventoryAddedConsumer.cs`). The same mechanism makes consumer retries safe — whether they come from broker re-delivery or from a retry after a transient failure (e.g. concurrency conflict), the event is never double-counted.

### Transactional outbox

Both services use MassTransit's EF Core outbox. Publishing an event is committed atomically with the DB write - there is no window where the row exists but the event was lost (or vice versa).

### Cross-service product existence - eventual, not synchronous

InventoryService keeps a local `KnownProducts` projection populated from
`ProductCreatedEvent`. This means:

- No sync call from inventory to product (no coupling, no cascading failure).
- `POST /inventory` for a brand-new product can briefly return 422 until the projection catches up. The E2E test waits for this with an `Eventually` helper.

### Concurrency on `Product.Amount`

Three layers, each addressing a different scope:

- `UsePartitioner(16, m => m.ProductId)` - within one consumer instance, messages for the same product are processed sequentially, eliminating in-process contention.
- `IsConcurrencyToken` on `Product.Amount` - cross-instance safety. If two instances race on the same product, the loser gets `DbUpdateConcurrencyException`.
- Exponential retry on the consumer - retries after a concurrency conflict re-read fresh state and increment correctly (kept safe by the dedup table from the previous section).

### JWT + RBAC

JWT bearer with HS256, validated against `Jwt:SigningKey/Issuer/Audience` from configuration. Controllers gate endpoints with `[Authorize(Roles = "write")]` / `[Authorize(Roles = "read")]`. `RoleClaimType = "role"` is set explicitly so the helper script and tests can issue compact tokens with the `role` claim.

Api endpoints are secure by default with a `FallbackPolicy`, with and opt-out using `AllowAnonymous` (e.g. `/health`)

### Error model

`GlobalExceptionHandler` produces RFC 7807 `ProblemDetails`:

| Exception              | HTTP status | Notes                              |
|------------------------|-------------|------------------------------------|
| `ValidationException`  | 400         | FluentValidation, with field map   |
| `DomainException`      | 422         | Business rule violation            |
| everything else        | 500         | Generic message, full stack logged |

Logs are structured JSON (Serilog CompactJsonFormatter) with `service.name`, trace, and span IDs enriched for every entry.

---

## Running the tests

Unit tests are pure in-memory.
Integration tests use Testcontainers and the MassTransit in-memory test harness.
End-to-end tests use Testcontainers for both Postgres *and* RabbitMQ (one Rabbit container shared between the two `WebApplicationFactory` instances).

```bash
dotnet test
```

---

## Project layout

```
recruitment-platform/
├── compose.yaml                 # Root compose: both services, postgres, rabbit
├── .env                         # Dev defaults (DB / RabbitMQ / JWT key)
├── tools/gen-token.ps1          # JWT generator for manual testing
├── contracts/                   # Shared integration event contracts
│
├── inventory-service/src/
│   ├── Inventory.Api            # HTTP, auth, exception handler
│   ├── Inventory.Application    # AddInventory command, ProductCreated consumer
│   ├── Inventory.Domain         # Inventory, KnownProduct entities
│   └── Inventory.Infrastructure # EF + RabbitMQ wiring
├── inventory-service/tests/     # Unit + integration
│
├── product-service/src/
│   ├── Product.Api              # HTTP, auth, exception handler
│   ├── Product.Application      # CreateProduct, ProductInventoryAdded consumer
│   ├── Product.Domain           # Product entity with IncrementAmount invariant
│   └── Product.Infrastructure   # EF + RabbitMQ wiring, ProcessedEventStore
├── product-service/tests/       # Unit + integration
│
└── e2e/EndToEnd.Test/           # Two services + real RabbitMQ via Testcontainers
```
