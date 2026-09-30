# GearTrack architecture overview

GearTrack is a .NET 10 ASP.NET Core Blazor Web App with interactive server rendering. It uses a small layered design that gives students genuine seams for testing without burying the domain in enterprise abstractions.

## Projects and responsibilities

### GearTrack.Core

The core project has no EF Core or web dependency. It contains:

- Domain entities: `EquipmentItem`, `Reservation`, and `ActivityRecord`
- Status enums and request/result models
- `IGearTrackService`, the workflow contract used by both UI and API
- `IGearTrackRepository`, the persistence boundary
- `GearTrackService`, which coordinates browse, reserve, checkout, return, history, and dashboard behavior

This is the primary unit-testing seam. A test can replace `IGearTrackRepository` with a fake without starting ASP.NET Core or opening a database.

### GearTrack.Infrastructure

The infrastructure project owns data access:

- `GearTrackDbContext` defines the EF Core model and relationships.
- `GearTrackRepository` implements the core repository contract.
- `DatabaseInitializer` calls `EnsureCreated` and adds a time-relative, fictional dataset when the database is empty.

The current provider is SQLite. The database lives under the web project's `App_Data` directory and is ignored by Git.

### GearTrack.Web

The web project is the composition root and delivery layer:

- `Program.cs` registers the database provider, repository, service, Blazor host, and startup seeding.
- Razor components provide the responsive interactive-server UI.
- `ApiEndpoints.cs` exposes the same workflows through independently callable minimal API endpoints.
- Local CSS supplies the complete visual design; the app has no CDN or frontend build dependency.

### GearTrack.Core.Tests

The test project contains the Week 5 test-repair starter. Its passing results are a starting point for evaluating test quality; see the README for the assignment contract.

## Request and data flow

```text
Browser component ─┐
                  ├──> IGearTrackService ──> IGearTrackRepository ──> EF Core ──> SQLite
HTTP /api endpoint ┘
```

The UI does not call its own HTTP API; both are first-class adapters over the same application service. This avoids duplicated rules while allowing UI tests and API tests to target different surfaces.

Interactive server rendering keeps component state on the ASP.NET Core server and sends UI events over the Blazor connection. The app does not require a separate JavaScript application, Node.js, or a client-side API token.

## Database lifecycle

At startup the host creates `App_Data`, opens the configured SQLite connection, creates the schema if needed, and seeds only when no equipment records exist. This makes the default path one command and preserves state between runs.

`EnsureCreated` is appropriate for the first classroom slice. Before adopting evolving production-like schemas, replace it with checked-in EF Core migrations.

## PostgreSQL extension point

Provider configuration is isolated in `Program.cs`. A future PostgreSQL profile would:

1. Add `Npgsql.EntityFrameworkCore.PostgreSQL` to Infrastructure.
2. Add a provider setting and PostgreSQL connection string.
3. Select `UseNpgsql` instead of `UseSqlite` during service registration.
4. Introduce provider-tested EF migrations.

The entities, service, repository contract, UI components, and API endpoints should not need to change. Docker Compose can then offer PostgreSQL as an optional profile while SQLite remains the zero-friction default.

## Testing and observability seams

- Core service tests can use repository fakes.
- Infrastructure tests can use temporary SQLite databases.
- API integration tests can host `Program` with `WebApplicationFactory` and replace its database registration.
- Browser tests can seed/reset a temporary database and drive the rendered UI.
- The API provides stable workloads for load tools such as k6, NBomber, or JMeter.
- ASP.NET Core logging, EF command logging, health checks, metrics, and distributed tracing can be added at the web composition root.

The architecture is intentionally small enough for students to trace a request from component or endpoint through a service and repository to the database in one class session.
