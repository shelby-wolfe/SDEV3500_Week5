# Credible semester enhancements

The Friday version is a stable vertical slice, not a production-finished system. These additions can evolve the same codebase alongside the course.

- **PostgreSQL:** add the Npgsql EF Core provider, migrations, provider-specific integration tests, and a configuration-selected database profile while retaining SQLite for quick local exercises.
- **Docker:** add an optional Compose file for the app and PostgreSQL. Keep `dotnet run` plus SQLite as the default so Docker never blocks a basic class exercise.
- **Continuous integration:** restore, format-check, build, unit test, integration test, publish results, and retain coverage/test artifacts on every change.
- **Automated tests:** expand core rule tests, SQLite repository tests, API contract/integration tests, and Playwright browser journeys. Add accessibility automation without treating it as a replacement for manual review.
- **Performance testing:** establish API service-level objectives, create k6 or NBomber workloads, grow the dataset, profile query counts, and compare SQLite/PostgreSQL behavior under concurrency.
- **Security testing:** introduce authentication and role-based authorization, add anti-automation/rate-limit scenarios, validate input and data exposure, generate an SBOM, and scan dependencies and source in CI.
- **Observability:** add health/readiness endpoints, structured logs, correlation IDs, OpenTelemetry traces and metrics, EF query telemetry, dashboards, and a deliberately scoped incident exercise.
- **Product depth:** cancellation, maintenance work orders, equipment bundles, overdue handling, administrative editing, exports, notifications, attachments, and a full audit log.

Recommended order: capture exploratory findings first, then add regression tests around agreed behavior before fixing the intentional issues. Follow with API integration tests and CI; introduce PostgreSQL and performance instrumentation after students have a baseline on SQLite.
