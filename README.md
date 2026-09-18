# Task Management API

A task management backend built with ASP.NET Core, PostgreSQL, Entity Framework Core, and JWT
authentication.

This is a **learning-oriented project**: it exists to practice building a real, working REST API
step by step — authentication, authorization, a service layer, database persistence, validation,
soft delete, and automated tests — without pulling in architecture that the problem does not need.
**V1 is complete.**

The documentation here describes what the project actually does today. Where a feature is
partially implemented or intentionally left out, that is stated explicitly.

## Tech Stack

Versions are taken from the project files.

| Area | Technology |
| --- | --- |
| Runtime / framework | .NET 10, ASP.NET Core Web API (`net10.0`) |
| Database | PostgreSQL |
| ORM | Entity Framework Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12) |
| Password hashing | ASP.NET Core Identity `PasswordHasher<User>` |
| API docs | `Microsoft.AspNetCore.OpenApi` 10.0.11 |
| Testing | xUnit, Moq, EF Core InMemory, `Microsoft.AspNetCore.Mvc.Testing` |
| Coverage | `coverlet.collector` |
| API client | Bruno collection (`bruno/`) |
| CI | GitHub Actions (`.github/workflows/ci.yml`) |

Both projects enable nullable reference types and treat nullable warnings as errors
(`<WarningsAsErrors>nullable</WarningsAsErrors>`).

## Features

**Authentication**
- Register and login with email + password
- Password hashing (never stored or logged in plain text)
- JWT access tokens and hashed, rotating refresh tokens
- Logout by revoking a refresh token

**Task management**
- Per-user task CRUD (users only see and modify their own tasks)
- Pagination, search (PostgreSQL `ILIKE`), completion filter, and sorting
- Soft delete (deleted tasks are hidden automatically)
- Assign categories to a task

**Categories**
- Category CRUD (admin only), unique by name

**User management**
- Admin user CRUD
- One-to-one user profile (create/update)
- "Users without tasks" report
- Delete guard: a user with tasks cannot be deleted

**Authorization**
- `[Authorize]` on task endpoints
- `AdminOnly` role policy on user, category, and admin task endpoints
- Ownership scoping for tasks

**API infrastructure**
- Consistent `ApiResponse<T>` response envelope
- Global exception handling with typed exceptions
- Data Annotations validation (automatic + service-level)
- Health check endpoint
- OpenAPI document with JWT Bearer security scheme
- Per-environment CORS and configuration

**Testing**
- Unit tests (controllers with mocks; services with EF Core InMemory)
- Integration tests against a real PostgreSQL database
- Code coverage via `coverlet` + `coverage.runsettings`

## Architecture

```text
HTTP Request
    ↓
ASP.NET Core Middleware
    ↓
Controller
    ↓
Service
    ↓
EF Core DbContext
    ↓
PostgreSQL
```

- **Controllers** handle HTTP concerns (routing, status codes, reading claims) and delegate to
  services.
- **Services** contain the logic and use `AppDbContext` directly.
- **DTOs** are separate from EF Core **entities**.
- **Errors** are handled centrally by `ApiExceptionHandler`.

There is no separate repository layer, CQRS, or multi-project layering — these were left out on
purpose. See [docs/architecture.md](docs/architecture.md) for details and
[docs/decisions.md](docs/decisions.md) for the reasoning.

## Project Structure

```text
.
├── TaskManagement.slnx              # solution (new .slnx format)
├── coverage.runsettings             # coverage collector configuration
├── .github/workflows/ci.yml         # CI: restore + test with a PostgreSQL service
├── bruno/                           # Bruno API request collection
├── docs/                            # documentation
├── TaskManagement.Api/              # Web API project
│   ├── Program.cs                   # DI, auth, CORS, middleware, startup seeding
│   ├── appsettings*.json            # shared + per-environment configuration
│   ├── Controllers/                 # Auth, Tasks, Users, Categories
│   ├── Services/                    # interfaces + implementations
│   ├── Models/                      # EF Core entities
│   ├── DTOs/                        # request/response models
│   ├── Data/                        # AppDbContext + seeders
│   ├── Errors/                      # ApiExceptionHandler, DuplicateResourceException
│   ├── Extensions/                  # ControllerExtensions (Reply helpers)
│   ├── Validation/                  # RequestValidation
│   ├── Health/                      # DatabaseHealthCheck
│   └── Migrations/                  # EF Core migrations
└── TaskManagement.Api.Tests/        # xUnit test project
    ├── Controllers/  Services/  Data/  Errors/  Validation/
    └── Integration/                 # WebApplicationFactory + PostgreSQL
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL (a local server or container)
- The EF Core CLI (`dotnet tool install --global dotnet-ef`) for applying migrations

### 1. Create the database

```sql
CREATE DATABASE task_management_db;
```

### 2. Configure secrets

`appsettings.json` intentionally contains no secrets. Provide them locally with .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=task_management_db;Username=YOUR_USERNAME;Password=YOUR_PASSWORD" \
  --project TaskManagement.Api

dotnet user-secrets set "Jwt:Key" "your-local-secret-key-minimal-32-characters" \
  --project TaskManagement.Api
```

The JWT key must be at least 32 characters (HMAC SHA-256). Never commit real secrets.

### 3. Apply migrations

```bash
dotnet ef database update --project TaskManagement.Api
```

### 4. Run the API

```bash
dotnet run --project TaskManagement.Api --launch-profile http
```

Default local URL: `http://localhost:5298`

In Development the app seeds demo data on startup:

```text
Admin:  admin@mail.com / secret123
User:   user@mail.com  / secret123
```

These are demo credentials for local use only. To wipe and reseed on startup, set
`Database:RefreshOnStartup` to `true` in `appsettings.Development.json`.

## Configuration

| Key | Purpose | Where to supply |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection | User Secrets (local), env var (CI/prod) |
| `Jwt:Key` | HMAC signing key (≥ 32 chars) | User Secrets (local), env var (CI/prod) |
| `Jwt:Issuer` / `Jwt:Audience` | Token issuer/audience | `appsettings.json` (defaults to `TaskManagement.Api`) |
| `Cors:AllowedOrigins` | Allowed browser origins | `appsettings.Development.json` / Production |
| `Database:RefreshOnStartup` | Truncate + reseed on startup | Development only |
| `Logging:LogLevel` | Log verbosity | per environment |

Environment-specific configuration:

```text
Development -> appsettings.json + appsettings.Development.json + User Secrets + environment variables
Production  -> appsettings.json + appsettings.Production.json + environment variables
Testing     -> appsettings.json + appsettings.Testing.json (+ User Secrets/env vars for the connection)
```

The app fails fast at startup if `Jwt:Key` is missing. Secrets must not be committed to the
repository.

## API Documentation

- **OpenAPI** is generated at `/openapi/v1.json`, available in **Development** and **Testing**
  only. It includes a JWT Bearer security scheme.
- **Bruno collection** in `bruno/` contains ready-to-run requests, including ordered flows under
  `bruno/_flows/` (admin, user, negative) and utilities under `bruno/_tools/`.
- Full endpoint reference: [docs/api.md](docs/api.md).

## Testing

Run all tests:

```bash
dotnet test TaskManagement.slnx
```

Run unit tests only (no database needed):

```bash
dotnet test TaskManagement.slnx --filter "FullyQualifiedName!~Integration"
```

Run tests with coverage:

```bash
dotnet test TaskManagement.slnx --collect:"XPlat Code Coverage" --settings coverage.runsettings
```

Integration tests create and drop their own isolated PostgreSQL database. They read the
connection string from environment variables or User Secrets, so the local setup above is
sufficient. See [docs/testing.md](docs/testing.md).

## Health Check

`GET /health` verifies the API can reach the database (`DatabaseHealthCheck` uses
`CanConnectAsync`).

- `200` with body `Healthy` when the database is reachable.
- `503` with body `Unhealthy` otherwise.

## Documentation

| Document | Contents |
| --- | --- |
| [docs/architecture.md](docs/architecture.md) | Layers, request lifecycle, DI, error handling, validation, soft delete |
| [docs/api.md](docs/api.md) | Endpoints by resource, status codes, response format, pagination |
| [docs/authentication.md](docs/authentication.md) | JWT, refresh tokens, password storage, authorization |
| [docs/database.md](docs/database.md) | Entities, relationships, constraints, soft delete, migrations |
| [docs/testing.md](docs/testing.md) | Test layout, running tests, coverage, CI |
| [docs/decisions.md](docs/decisions.md) | Intentional design decisions and trade-offs |

## V1 Scope

V1 is considered complete. It was built to demonstrate, end to end:

- structuring an ASP.NET Core Web API with controllers and a service layer,
- persistence with EF Core and PostgreSQL migrations,
- JWT authentication with refresh token rotation,
- role-based authorization and resource ownership,
- request validation and consistent error handling,
- soft delete with EF Core global query filters,
- unit and integration testing, code coverage, and CI.

The current behavior — including what is intentionally partial (for example, no refresh-token
reuse detection) or deliberately simple — is documented in
[docs/decisions.md](docs/decisions.md).

> Future experiments are intentionally kept outside the V1 scope.
