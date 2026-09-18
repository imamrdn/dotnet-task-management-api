# Architecture

This document describes how the Task Management API is actually structured today.
It intentionally avoids assigning architecture labels the project does not genuinely implement.

## Architecture Overview

The application is a single ASP.NET Core Web API project (`TaskManagement.Api`) with a
companion test project (`TaskManagement.Api.Tests`). It follows a simple layered flow:

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

There is no separate repository layer, no CQRS, and no multi-project layering. Controllers call
services, services use `AppDbContext` directly. This is a deliberate choice — see
[decisions.md](decisions.md).

## Request Lifecycle

The middleware pipeline is configured in `Program.cs`, in this exact order:

```text
UseExceptionHandler      → global exception handling
UseStatusCodePages       → JSON body for 401 / 403 / 404 set outside controllers
UseHttpsRedirection      → HTTPS redirect
UseCors                  → CORS policy "AllowedOrigins"
UseAuthentication        → validates the JWT and builds HttpContext.User
UseAuthorization         → evaluates [Authorize] and the AdminOnly policy
MapGet("/health")        → health check endpoint
MapControllers()         → MVC controllers
```

A typical authenticated request:

1. **Middleware** — the JWT is validated by `UseAuthentication`; the resulting claims populate
   `HttpContext.User`.
2. **Authorization** — `UseAuthorization` checks `[Authorize]` attributes and the `AdminOnly`
   policy before the action runs.
3. **Controller** — the action validates simple input, reads the user id from claims, and calls a
   service. It shapes the HTTP response (status code + `ApiResponse<T>` body).
4. **Service** — business logic, database access through `AppDbContext`, and structured logging.
5. **EF Core** — LINQ queries are translated to SQL against PostgreSQL.
6. **Response** — the service returns DTOs, the controller wraps them in `ApiResponse<T>`.

## Controllers

Controllers live in `Controllers/` and are thin: they handle HTTP concerns only
(status codes, request shape, reading claims) and delegate logic to services.

| Controller | Route | Authorization | Responsibility |
| --- | --- | --- | --- |
| `AuthController` | `api/auth` | anonymous | register, login, refresh, logout |
| `TasksController` | `api/tasks` | `[Authorize]` (+ `AdminOnly` on admin routes) | task CRUD, completion, task categories, admin reports |
| `UsersController` | `api/users` | `AdminOnly` | user CRUD, profiles, users without tasks |
| `CategoriesController` | `api/categories` | `AdminOnly` | category CRUD |

Notes confirmed in code:

- `TasksController.GetUserIdFromClaims()` reads `ClaimTypes.NameIdentifier` and uses
  `int.TryParse`; a missing or non-numeric claim throws `UnauthorizedAccessException`
  (mapped to `401`).
- `TasksController` exposes `MaxPageSize = 100` and rejects larger limits with `400`.
- Controllers use the `Reply` helpers from `Extensions/ControllerExtensions.cs` to keep response
  shaping consistent (see below).

## Services

Services live in `Services/`, each with an interface and a concrete implementation registered
in DI:

| Interface | Implementation | Responsibility |
| --- | --- | --- |
| `IAuthService` | `AuthService` | registration, login, JWT + refresh token issuance, rotation, logout |
| `ITaskService` | `TaskService` | task queries and writes, task↔category assignment, admin reports |
| `IUserService` | `UserService` | user queries and writes, profile upsert |
| `ICategoryService` | `CategoryService` | category queries and writes |

Services receive `AppDbContext`, `ILogger<T>`, and (where needed) `IConfiguration` and
`IPasswordHasher<User>` through constructor injection. They return DTOs, never entities.

## DTOs vs Entities

API contracts and persistence models are kept separate:

- `Models/` — EF Core entities mapped to tables (`User`, `TaskItem`, `Category`, `TaskCategory`,
  `UserProfile`, `RefreshToken`).
- `DTOs/` — request and response records (`CreateTaskRequest`, `TaskResponse`, `ApiResponse<T>`, …).

This separation means:

- Entities can change without breaking the public API shape.
- Responses never accidentally expose sensitive entity fields (for example `User.PasswordHash`
  is never returned — the API returns `UserResponse` with `Id`, `Name`, `Email` only).
- Requests are validated independently of the database model.

## Dependency Injection

All registrations are in `Program.cs`. Lifetimes are all `Scoped`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<UserSeeder>();
builder.Services.AddScoped<CategorySeeder>();
builder.Services.AddScoped<TaskSeeder>();
```

Scoped is the correct lifetime here because services depend on `AppDbContext`, which is itself
scoped per request. `IPasswordHasher<User>` is registered so that password hashing is shared
through DI rather than `new`-ed in multiple places.

## Error Handling

Handled centrally by `Errors/ApiExceptionHandler.cs`, an `IExceptionHandler` registered with
`AddExceptionHandler` and wired via `UseExceptionHandler`.

Exception → status mapping:

| Exception | Status | Body message |
| --- | --- | --- |
| `ArgumentException` | `400` | exception message |
| `DuplicateResourceException` | `400` | exception message |
| `UnauthorizedAccessException` | `401` | exception message |
| `OperationCanceledException` (client aborted) | `499` | none (no body) |
| anything else | `500` | `"An unexpected error occurred"` |

Custom exception type that currently exists: `DuplicateResourceException` (in `Errors/`), thrown
for duplicate email and duplicate category name. Unexpected `500` errors are logged with
`LogError`, including the exception, but the response never exposes internal details.

Responses that are not produced by an action (framework-level `401`/`403`/`404`) are given a JSON
body by `UseStatusCodePages`, so every error still uses the `ApiResponse<T>` shape.

## Validation

Validation happens in two layers:

1. **ASP.NET Core model validation (automatic).** Because controllers use `[ApiController]`,
   Data Annotations on DTOs (`[Required]`, `[EmailAddress]`, `[MaxLength]`) are validated
   automatically. Invalid requests short-circuit before the action runs.
   `Program.cs` customizes the failure response via `InvalidModelStateResponseFactory`, returning
   `400` with an `ApiResponse<object>` whose message is the first validation error.

2. **Service-level validation (explicit).** Services call
   `Validation/RequestValidation.EnsureValid(request)` before doing work. This exists so that
   non-HTTP callers (unit tests, future background jobs) are validated too. `RequestValidation`
   reads the Data Annotations already declared on the DTO — including attributes placed on
   positional record constructor parameters, which plain `Validator.TryValidateObject` does not
   see. It throws `ArgumentException`, which the global handler maps to `400`.

   `CategoryService` also has a small private `ValidateName` helper for category names.

Both layers use the same Data Annotations, so the rule and its message live in one place: the DTO.

## Soft Delete

Tasks are soft-deleted, not removed:

- `TaskItem.IsDeleted` (bool) and `TaskItem.DeletedAt` (nullable `DateTime`).
- `DELETE /api/tasks/{id}` sets `IsDeleted = true` and `DeletedAt = DateTime.UtcNow`
  (`TaskService.DeleteTaskAsync`).

The filtering is enforced by **EF Core global query filters** in `AppDbContext`:

```csharp
modelBuilder.Entity<TaskItem>()
    .HasQueryFilter(task => !task.IsDeleted);

modelBuilder.Entity<TaskCategory>()
    .HasQueryFilter(taskCategory => !taskCategory.TaskItem.IsDeleted);
```

Because the filter is global, every query automatically excludes soft-deleted tasks — there is no
manual "active only" filter to remember. The `TaskCategory` filter exists because EF Core
recommends filtering both ends of a required relationship when one side is filtered.

`IgnoreQueryFilters()` is used deliberately in exactly one place: `UserService.DeleteUserAsync`
checks whether a user still has **any** tasks, including soft-deleted ones, before allowing the
delete. Without ignoring the filter, soft-deleted tasks would be invisible and the database
`Restrict` foreign key would reject the delete with a less clear error.

## Cancellation

`CancellationToken` flows from the HTTP request down to the database:

- Controllers accept `CancellationToken cancellationToken = default` and pass it to services.
- Services (`TaskService`, `UserService`, `CategoryService`) forward it to EF Core async calls
  (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, `FindAsync`).
- If the client disconnects, EF Core throws `OperationCanceledException`, which
  `ApiExceptionHandler` recognizes when `HttpContext.RequestAborted` is cancelled and returns
  `499` without logging an error.

`AuthService` methods do not currently take a `CancellationToken` (they are driven by short,
login-time operations).

## Architectural Boundaries

V1 intentionally does **not** introduce additional layers or patterns. Specifically, the project
does not have:

- a separate repository abstraction on top of `AppDbContext`,
- CQRS or MediatR,
- multi-project Clean Architecture layering,
- microservices or message-based architecture.

These are not missing features — they were deliberately left out because they would add
indirection without benefit at this scope. `AppDbContext` already acts as the unit of work, and
the Controller → Service → DbContext flow is easy to follow for a learning project. See
[decisions.md](decisions.md) for the reasoning.
