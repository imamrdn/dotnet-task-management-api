# Testing

How the test suite is organized, how to run it, and how coverage works.

## Test Project

Tests live in `TaskManagement.Api.Tests/` (xUnit). Packages: `xunit`, `xunit.runner.visualstudio`,
`Microsoft.NET.Test.Sdk`, `Moq`, `Microsoft.EntityFrameworkCore.InMemory`,
`Microsoft.EntityFrameworkCore.Relational`, `Microsoft.AspNetCore.Mvc.Testing`, and
`coverlet.collector`.

## Test Layout

| Folder | Type | What it covers |
| --- | --- | --- |
| `Controllers/` | Unit | Controller actions with mocked services (`Moq`), including status codes and the `Reply` helpers. |
| `Services/` | Unit | Service logic against the EF Core InMemory provider (`TaskService`, `UserService`, `CategoryService`, `AuthService`). |
| `Data/` | Unit | Seeders and the soft-delete global query filters. |
| `Errors/` | Unit | `ApiExceptionHandler` status mapping and cancellation handling. |
| `Validation/` | Unit | `RequestValidation.EnsureValid` for DTOs. |
| `Integration/` | Integration | Full HTTP flow through the real app against a real PostgreSQL database. |

Helpers:

- `TestDbContextFactory` — creates an `AppDbContext` backed by a unique InMemory database
  (`Guid`-named) per test, so tests are isolated.
- `TestLogger<T>` — captures log entries so tests can assert on logging (and on the absence of
  sensitive values).
- `PostgresWebApplicationFactory` — the integration test host (see below).

## Unit Tests

Unit tests do not need a database server. Services are tested with the EF Core **InMemory**
provider; controllers are tested with mocked service interfaces (`Mock<ITaskService>`, etc.).

This is why `AuthController` depends on `IAuthService`: the controller can be tested in isolation
without a real database, hasher, or configuration.

## Integration Tests

`Integration/ApiIntegrationTests.cs` uses `PostgresWebApplicationFactory`
(a `WebApplicationFactory<Program>`) to exercise the API over HTTP with a real PostgreSQL
database. On startup it:

1. Generates a unique test database name (`task_management_test_<guid>`).
2. Creates that database using a maintenance connection to the `postgres` database.
3. Replaces the app's `DbContext` registration with one pointing at the test database.
4. Applies EF Core migrations (`MigrateAsync`).
5. Seeds demo data.

On dispose it terminates connections and drops the test database, so the developer's real
database is never touched.

Integration tests cover authorization (`401`/`403`), CORS preflight, the health endpoint, the
OpenAPI document, validation errors, JWT usage, refresh/logout rotation, soft delete over HTTP,
admin reports, category assignment, and database refresh/rollback.

The factory reads the connection string from environment variables or User Secrets
(`AddUserSecrets<Program>(optional: true)` and `AddEnvironmentVariables()`), so the same
`ConnectionStrings:DefaultConnection` used for local development also drives integration tests.

## Running Tests

Run everything:

```bash
dotnet test TaskManagement.slnx
```

Run only unit tests (no database required) by excluding the integration namespace:

```bash
dotnet test TaskManagement.slnx --filter "FullyQualifiedName!~Integration"
```

Integration tests require a reachable PostgreSQL instance and a configured
`ConnectionStrings:DefaultConnection` (see the README's Getting Started section).

## Code Coverage

The repository includes `coverage.runsettings`, which configures the `XPlat Code Coverage`
collector (Cobertura format) and excludes generated/irrelevant files:

```xml
<ExcludeByFile>**/Migrations/**,**/Program.cs,**/obj/**</ExcludeByFile>
```

Collect coverage:

```bash
dotnet test TaskManagement.slnx --collect:"XPlat Code Coverage" --settings coverage.runsettings
```

This writes a `coverage.cobertura.xml` under `TaskManagement.Api.Tests/TestResults/<guid>/`.
`TestResults/` is git-ignored, so coverage output is never committed.

The project deliberately does not target 100% coverage. Trivial or unreachable code (for example
the "no constructor" branch in `RequestValidation`, or auto-generated entity properties) is left
untested on purpose, since tests for them would add noise without value.

## Continuous Integration

`.github/workflows/ci.yml` runs on pushes and pull requests to `main`:

1. Starts a PostgreSQL 16 service container.
2. Provides the connection string and JWT settings through environment variables
   (`ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`).
3. Installs the .NET 10 SDK.
4. `dotnet restore TaskManagement.slnx`
5. `dotnet test TaskManagement.slnx --no-restore`

CI runs the full suite (unit + integration). The JWT key in CI is a test-only value, not a real
secret.
