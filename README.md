# MediaTR

MediaTR is a backend API for a full-featured e-commerce marketplace.

## Project Overview

![MediaTR](assets/MediaTR.png) Users can browse and purchase products, manage shopping carts, track orders, and post marketplace advertisements. The platform is built on a dual-database strategy — MongoDB for domain data and SQL Server for the audit trail — with Redis handling distributed caching and shopping cart state. An outbox pattern ensures reliable event delivery between domain operations.

---

## Technologies

| Category | Technology |
|----------|-----------|
| Language | C# / .NET 9 |
| Architecture | Clean Architecture (8 projects: ApiService, Application, Domain, Infrastructure, SharedKernel, ServiceDefaults, AppHost, MigrationService) |
| API | ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core 9 (SQL Server) |
| Document DB | MongoDB (products, categories, users, orders, advertisements, wishlists) |
| Relational DB | SQL Server (outbox events, refresh tokens) |
| Cache | Redis via StackExchange.Redis (distributed cache, shopping cart, token blacklist) |
| CQRS / Mediator | MediatR 12 |
| Validation | FluentValidation 11 |
| Auth | JWT Bearer + Refresh Tokens (HttpOnly cookie) |
| Password Hashing | BCrypt.Net-Next |
| Logging | Serilog (structured, with correlation ID tracking) |
| Observability | OpenTelemetry (tracing + metrics) |
| Orchestration | .NET Aspire (AppHost + ServiceDefaults) |
| Migrations | Dedicated MigrationService (init container pattern) |

---

## Features

### Authentication & Authorization
- JWT access tokens (15-minute expiry) + refresh tokens (7-day / 30-day with "Remember Me")
- Refresh tokens stored in HttpOnly cookies — never exposed to JavaScript
- Token blacklist on logout (Redis-backed, JTI-based revocation)
- Account lockout after 5 failed login attempts (15-minute lock)
- Role-based authorization (`Customer`, `Seller`, `Admin`, `SuperAdmin`)

### Product Catalogue
- Full CRUD for products (name, description, slug, price, stock, SKU, attributes, tags, images)
- Multi-currency pricing via `Money` value object (Amount + Currency)
- Paginated product listing, filtering by category, slug lookup, and full-text search
- Featured products query
- Product data cached in Redis with a 1-hour TTL; cache invalidated on update

### Category Management
- Hierarchical category tree (parent → child relationships)
- Root categories, child categories, and full tree queries
- Category slug lookup
- Category-product cache with a 30-minute TTL

### Shopping Cart
- Redis-backed in-memory cart — no database write on every cart change
- Add, update quantity, remove items, and clear cart
- Guest cart merge on login (guest cart items folded into the authenticated user's cart)

### Wishlist
- Per-user wishlist stored in MongoDB
- Add and remove products; clear all
- Instantly reflects stock status changes

### Order Management
- Order checkout from cart (validates stock, calculates totals including tax, shipping, and discounts)
- Order items snapshot (price at time of purchase preserved)
- Full order lifecycle: `Pending → Confirmed → Processing → Shipped → Delivered`
- Cancellation and return flows
- Per-user order history and admin pending-orders view
- Order status updates trigger domain events via the outbox

### Marketplace Advertisements
- Sellers post listings with title, description, price, images, and contact phone
- Approval workflow: `Draft → PendingApproval → Active` (admin approves)
- Advertisement expiry and cooldown enforced at the application level
- Phone visibility toggle per listing

### Outbox Pattern (Reliable Event Delivery)
- Every state-changing command writes domain events to an `OutboxEvent` table inside the same SQL Server transaction
- `OutboxProcessor` (background service) polls every 5 seconds in batches of 10 and dispatches events to their handlers
- Handlers cover: `OrderPlaced`, `OrderShipped`, `OrderDelivered`, `OrderCancelled`, `ProductCreated`, `AdvertisementPublished`
- Guarantees at-least-once delivery even if the process crashes between save and publish

### Observability & Reliability
- Serilog structured logging with correlation ID propagation through every command and query
- OpenTelemetry traces and metrics via `ServiceDefaults`
- Health check endpoints for all services
- .NET Aspire AppHost orchestrates SQL Server, MongoDB, and Redis containers with persistent volumes
- Dedicated `MigrationService` runs EF Core migrations before the API starts (init container pattern)

---

## The Process

### `Initial commit` + `Implement order CQRS and result pattern foundation`
Solution scaffolded with Clean Architecture across 8 projects. Domain entities defined (`Product`, `Category`, `User`, `Order`, `Advertisement`, `Wishlist`), value objects created (`Money`, `Email`, `Address`), and the CQRS foundation laid with MediatR. `Result<T>` and strongly-typed `Error` types established in SharedKernel to enforce railway-oriented error handling throughout.

### `Refactor to use Result pattern and custom messaging interfaces` + `Refactor domain events to use generic base and payload`
Result pattern propagated to all command and query handlers. Domain events refactored to a generic base (`DomainEventBase<TPayload>`) so event payloads are strongly typed and handlers do not rely on casting.

### `Add error classes, middleware, and Serilog integration`
Centralised error handling added via exception middleware — all unhandled exceptions produce consistent `ProblemDetails` responses. Serilog wired into the host with console sink and correlation ID enrichment.

### `Implement CorrelationId pattern for PlaceOrder flow` + `Add CorrelationId to commands, queries, and business logic`
`CorrelationId` added as a first-class property on all `ICommand` and `IQuery` objects. Every log entry across the full request lifetime carries the same correlation ID, making distributed tracing trivial.

### `Add Serilog logging and MediatR logging behavior`
`LoggingBehavior` registered as the first MediatR pipeline behavior — logs request name, correlation ID, and elapsed time for every command and query without touching handler code.

### `Add outbox pattern core entities and configuration` + `Implement Outbox Pattern with SQL persistence` + `Add outbox event configuration and improve data persistence`
`OutboxEvent` entity added to the SQL Server `ApplicationDbContext`. EF Core configuration and initial migration created. `ApplicationDbContext.SaveChangesAsync` intercepts domain events raised on aggregates and writes them as serialised outbox rows inside the same transaction.

### `Add MigrationService for database migrations`
Standalone `MigrationService` project added — an `IHostedService` that runs `dotnet ef database update` for the SQL Server context on startup and exits. The Aspire AppHost waits for it to complete before starting the API, implementing the init container pattern without Kubernetes.

### `Implement outbox event handling for OrderPlaced` + `Add transactional command handler and outbox support` + `Implement order creation endpoint with DTO and command`
`PlaceOrderCommand` and its handler implemented with full transactional support: stock validation, order total calculation, and outbox event write happen in a single `SaveChanges` call. `OutboxProcessor` background service wired up; `OrderPlacedEventHandler` sends confirmation logic. `POST /api/orders` endpoint exposed.

### `Refactor command patterns and add domain events` + `Add outbox event handlers for new domain events`
Command base classes unified. Additional outbox handlers registered: `ProductCreated`, `AdvertisementPublished`, `OrderShipped`, `OrderDelivered`, `OrderCancelled` — giving every major state transition a reliable downstream hook.

### `Add minimal API endpoints for core resources` + `Remove MVC controllers and switch to minimal API`
MVC controller layer removed entirely. All endpoints migrated to ASP.NET Core Minimal APIs using an `IEndpoint` convention — each endpoint class self-registers its route, HTTP method, and authorization policy.

### `Add validation and time abstraction to application`
`ValidationBehavior` added as a MediatR pipeline behavior after `LoggingBehavior` — FluentValidation validators run automatically for every command and query; validation failures short-circuit and return `400 Bad Request` before the handler executes. `IDateTimeProvider` abstraction introduced so handlers never call `DateTime.UtcNow` directly, keeping them fully testable.

### `Add localization support for error responses`
Error message strings extracted to resource files. Response errors adapt to the `Accept-Language` header, making the API ready for multi-language frontends.

### `Add JWT authentication and refresh token infrastructure`
`JwtTokenService` generates signed access tokens (15-minute expiry). Refresh tokens stored in SQL Server. Token validation middleware checks the Redis blacklist on every authenticated request via an `OnTokenValidated` hook.

### `Add authentication endpoints and handlers` + `Move refresh token to HttpOnly cookie for auth endpoints`
`POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, and `POST /api/auth/refresh-token` endpoints created. Refresh token moved from the response body to an HttpOnly `Set-Cookie` header so JavaScript cannot read it, following the recommended security practice.

### `Implement token blacklist, remember-me, and Redis cache`
Logout writes the access token's JTI to Redis with a TTL matching the token's remaining lifetime. "Remember Me" flag on login extends the refresh token from 7 to 30 days. `RedisCacheService` abstraction wraps StackExchange.Redis for all caching operations (Get, Set, Remove, Exists with TTL).

### `Add shopping cart and wishlist domain and services`
`ShoppingCartService` implemented on Redis — cart state persisted as a serialised hash keyed by user/session ID. `WishlistRepository` backed by MongoDB. Cart merge logic (guest → authenticated) implemented in the service layer.

### `Implement wishlist feature with endpoints and business logic`
Wishlist endpoints (`GET`, `POST /items/{productId}`, `DELETE /items/{productId}`, `DELETE`) wired up. `WishlistBusinessLogic` validates that added products exist and are in stock before persisting.

### `Add product query endpoints with caching and pagination`
Product endpoints added with 1-hour Redis caching on individual product reads. `SearchAdverts` and category-product queries cache results for 30 minutes. Pagination (`skip` / `take`) applied at the MongoDB query level to avoid loading full collections.

### `Add category endpoints and caching to repository`
Category tree, root, children, and slug endpoints exposed. `CategoryRepository` caches the full tree in Redis; any write to a category invalidates the tree cache. Admin `POST /api/categories` endpoint secured with policy-based authorization.

### `Implement order creation and cancellation features`
Order cancellation (`OrderStatus.Cancelled`) implemented with stock restock logic. `CanBeCancelled` domain method enforces the business rule that only `Pending` and `Confirmed` orders may be cancelled. `PUT /api/orders/{orderId}/status` endpoint added for admin status progression.

---

## What I Learned

- **Dual-database strategy** — using MongoDB for flexible, document-oriented domain data (products with arbitrary attributes, nested order items) alongside SQL Server for transactional data (outbox events, refresh tokens) taught me that choosing a database per access pattern rather than one-size-fits-all is a real architectural decision with concrete trade-offs
- **Outbox pattern** — directly publishing events after `SaveChanges` is a silent reliability bug; the outbox pattern (write events to the same transaction, poll and dispatch separately) was the right fix, and building it from scratch clarified why solutions like Debezium or Wolverine exist
- **.NET Aspire** — orchestrating SQL Server, MongoDB, and Redis containers with persistent volumes and health-aware startup order in code (rather than YAML) dramatically reduced the "works on my machine" problem; the init container pattern for migrations felt natural once AppHost resource dependencies clicked
- **Redis beyond caching** — using the same Redis instance for four distinct purposes (shopping cart state, token blacklist, distributed object cache, data protection keys) via a single `ICacheService` abstraction showed that Redis is a multi-tool, not just a key-value cache
- **HttpOnly cookies for refresh tokens** — moving the refresh token from the response body to a `Set-Cookie` header is a small code change but a significant security improvement; building it forced me to understand the `SameSite`, `Secure`, and `Path` cookie attributes and how they interact with CORS
- **MediatR pipeline behaviors** — inserting `LoggingBehavior` before `ValidationBehavior` means every request gets correlation ID logging regardless of whether validation passes; the ordering of behaviors is a real design concern that affects observability
- **Correlation IDs across a request** — propagating a single correlation ID from the HTTP request through every command, query, domain event, and log entry makes debugging distributed flows dramatically easier; building this from scratch rather than relying on a framework made the mechanism transparent
- **Value objects in a document store** — storing `Money` and `Address` as embedded documents in MongoDB works naturally, but EF Core owned entities for the same types on the SQL side require explicit configuration; the mismatch reinforced why domain types should be infrastructure-agnostic
- **Cart merge on login** — merging a guest cart into an authenticated user's cart sounds simple but requires resolving quantity conflicts for items that appear in both carts; building the merge logic made me think carefully about idempotency and user expectation

---

## How Can It Be Improved

- **Payment integration** — orders move through the lifecycle but no payment gateway is connected; integrating Stripe or iyzico with webhook-driven status updates would make the order flow production-ready
- **Real-time notifications** — order status changes currently only update the database; a SignalR hub or Server-Sent Events stream would let the frontend reflect changes (shipped, delivered) without polling
- **Elasticsearch for product search** — MongoDB `$text` search is limited; Elasticsearch would provide relevance ranking, faceted filtering, typo tolerance, and the performance needed for a real product catalogue
- **Image upload pipeline** — product and advertisement images are stored as URLs; integrating Azure Blob Storage or AWS S3 with a pre-signed upload endpoint and CDN delivery would be more production-grade
- **Rate limiting** — no rate limiting beyond account lockout is applied; ASP.NET Core's built-in `RateLimiter` middleware on the auth and search endpoints would protect against brute-force and scraping
- **Outbox at-exactly-once** — the current outbox guarantees at-least-once delivery; making handlers idempotent (by tracking processed event IDs) would upgrade this to effectively-once and eliminate duplicate side effects on retry
- **Event versioning** — outbox events are serialised as raw JSON with no schema version; adding a `SchemaVersion` field and a migration strategy would allow event shapes to evolve without breaking deployed handlers
- **Architecture tests** — there are no automated checks that, for example, the Domain project never references Infrastructure; adding NetArchTest rules would enforce layer boundaries on every build
- **Integration tests** — Testcontainers for .NET makes it straightforward to spin up real MongoDB, SQL Server, and Redis containers in tests; end-to-end handler tests against real databases would catch issues that mocked unit tests miss

---

## Running The Project

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)

```bash
dotnet workload install aspire
```

### 1. Start the application via Aspire

Aspire's AppHost orchestrates all infrastructure (SQL Server, MongoDB, Redis) and the API together:

```bash
cd MediaTR.AppHost
dotnet run
```

On first run, the `MigrationService` applies EF Core migrations to SQL Server automatically before the API starts.

### 2. Configure secrets

Open `MediaTR.ApiService/appsettings.json` and fill in the placeholder values:

```json
"JwtSettings": {
  "SecretKey": "YOUR_SECRET_KEY_AT_LEAST_32_CHARS",
  "Issuer": "MediaTR",
  "Audience": "MediaTR",
  "AccessTokenExpirationInMinutes": 15,
  "RefreshTokenExpirationInDays": 7,
  "RememberMeRefreshTokenExpirationInDays": 30
}
```

> SQL Server, MongoDB, and Redis connection strings are managed by Aspire and injected automatically — no manual configuration needed for local development.

### 3. Explore the API

Swagger UI is available at `https://localhost:{port}/swagger` once the API is running.

The Aspire dashboard (launched automatically) shows service health, logs, and traces at `https://localhost:15888`.
