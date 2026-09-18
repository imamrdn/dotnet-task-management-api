# Design Decisions

Intentional choices made while building V1. Each entry states the decision, the reason, and the
trade-off. These are the decisions that shaped the current code — they are not a roadmap.

## Scope: a learning backend, kept deliberately small

The project is a learning-oriented ASP.NET Core backend. Every layer added must earn its place.
The goal is to demonstrate the core request flow and the fundamental building blocks clearly,
not to model a large production system.

## No repository layer — use `AppDbContext` directly

**Decision:** Services use `AppDbContext` directly. There is no `IRepository<T>` abstraction.

**Why:** EF Core's `DbContext` already implements the Unit of Work and Repository patterns.
Adding another abstraction over it would mean writing pass-through code that returns the same
queries with more indirection, and it would make the learning flow (Controller → Service →
DbContext) harder to follow.

**Trade-off:** If the data source changed (e.g. to a non-relational store), the service layer
would change. At this scope that is not a realistic concern.

## No CQRS / MediatR, no Clean Architecture, no microservices

**Decision:** A single API project with Controllers → Services → DbContext. No command/query
separation, no mediator, no multi-project layering, no distributed services.

**Why:** These patterns solve specific problems (very large domains, many contributors, team
boundaries) that this project does not have. Introducing them would add concepts without
demonstrating anything about the fundamentals the project is meant to teach.

## DTOs separate from entities

**Decision:** Request/response models live in `DTOs/`, distinct from EF Core entities in `Models/`.

**Why:** Keeps the public API contract independent of the persistence model, and prevents leaking
entity fields (notably `User.PasswordHash`) to clients. It also lets requests carry validation
attributes without touching entities.

## Consistent response envelope: `ApiResponse<T>`

**Decision:** All JSON responses use `ApiResponse<T>` with `success`, `message`, and `data`.

**Why:** A predictable shape is easy for clients and for the learning project's tests to assert
against. `UseStatusCodePages` gives framework-level `401`/`403`/`404` responses the same shape, so
errors are uniform.

**Trade-off:** This is a custom envelope rather than the RFC 7807 `ProblemDetails` standard. That
is a conscious choice for consistency and simplicity at this scope.

## Typed exceptions instead of string matching

**Decision:** A custom `DuplicateResourceException` is thrown for duplicate email/category name,
and `ApiExceptionHandler` maps by **exception type**, not by message text.

**Why:** An earlier version matched on the message string (`when exception.Message == ...`), which
caused a real bug: a duplicate category name fell through to `500` because its message differed
from the email case. Mapping by type is robust and self-documenting. `ArgumentException` (bad
input) → `400`, `UnauthorizedAccessException` → `401`, anything else → `500`.

## Validation in two layers, sharing one definition

**Decision:** DTOs carry Data Annotations (used automatically by `[ApiController]`), and services
also call `RequestValidation.EnsureValid(request)`.

**Why:** The automatic model validation only runs for HTTP requests. Service-level validation
protects non-HTTP callers (unit tests, and any future background processing). Both read the same
attributes, so the rule and its message stay defined once — on the DTO — and a new required field
needs only an attribute, not a new helper method.

**Note:** `RequestValidation` also validates attributes on positional record **constructor
parameters**, because plain `Validator.TryValidateObject` does not see them (this was confirmed
empirically during development).

## Soft delete with a global query filter

**Decision:** `DELETE /api/tasks/{id}` sets `IsDeleted`/`DeletedAt`; an EF Core global query filter
(`HasQueryFilter`) excludes deleted tasks from all queries.

**Why:** Soft delete preserves data and audit fields. Using a global filter means no query can
forget to exclude deleted rows — an earlier manual `WhereActive()` helper was easy to miss. A
matching filter on `TaskCategory` keeps category links consistent, as EF Core recommends for
required relationships.

**Trade-off:** Deleted rows remain in the database. `IgnoreQueryFilters()` must be used
deliberately when deleted rows matter — which is exactly what the delete-user guard does.

## Refuse to delete a user that still has tasks

**Decision:** `DELETE /api/users/{id}` returns `400` when the user has any tasks (including
soft-deleted ones), and the `TaskItem → User` foreign key is `Restrict`.

**Why:** Previously, deleting a user cascade-deleted all their tasks permanently with no trace —
inconsistent with soft delete and a real data-loss risk. Refusing the delete keeps the model
consistent and makes the consequence explicit. The `Restrict` FK is a database-level backstop.

**Considered and rejected:** soft-deleting users. It would conflict with the unique `Email` index
(a deleted user's email could not be reused), leave that user's refresh tokens valid, and require
rewriting every `FindAsync`-based lookup to respect a filter. Refusing the delete solves the
data-loss risk with far less change.

**Trade-off:** Admins must delete or reassign a user's tasks before removing the user. That is an
acceptable, explicit workflow for this scope.

## Refresh token rotation, without reuse detection

**Decision:** Refresh tokens are random, stored only as a SHA-256 hash, rotated on every refresh,
and revoked on logout. Reuse of a revoked/expired token is rejected.

**Why:** Rotation limits the value of a stolen refresh token, and hashing means a database leak
does not expose usable tokens.

**Explicitly not implemented:** automatic reuse *detection* / token-family revocation.
`ReplacedByTokenHash` is written during rotation but nothing reads it yet, so reusing an old token
is simply rejected rather than triggering broader revocation. Documented as current behavior, not
as a feature.

## Correct HTTP status semantics for ids

**Decision:** A non-positive `{id}` (e.g. `0` or `-1`) returns `400` (invalid input), while a
valid-but-missing or not-owned id returns `404`. `int.TryParse` is used for the user-id claim so a
malformed claim yields `401`, not `500`.

**Why:** `400` and `404` mean different things to clients. "Your request is malformed" is not the
same as "the resource does not exist". This was corrected after an earlier version returned `404`
for invalid ids.

## Standardized controller responses via `Reply` helpers

**Decision:** `Extensions/ControllerExtensions.cs` provides `Reply` overloads
(200/404, 204/404, always-200) and `IsValidId`, used across all controllers.

**Why:** The same null-check and delete-result patterns were repeated many times. The helpers make
controller actions one line and keep status-code behavior uniform. `Created(...)` (which sets a
`Location` header) and validation `BadRequest(...)` (unique messages per case) are intentionally
left explicit.

## Interfaces for services

**Decision:** Each service has an interface (`ITaskService`, `IUserService`, `ICategoryService`,
`IAuthService`) and controllers depend on the interface.

**Why:** Primarily consistency and testability — controllers can be tested with mocked services
(for example `Mock<IAuthService>`) without a database. It is not motivated by a plan to swap
implementations.

## Passwords hashed via DI

**Decision:** `IPasswordHasher<User>` is registered in DI and injected where needed, rather than
`new`-ing `PasswordHasher<User>` in each service.

**Why:** Password hashing was duplicated across `AuthService`, `UserService`, and `UserSeeder`.
Centralizing it through DI means changing the hashing strategy is a one-place change.

## `CancellationToken` forwarded end-to-end

**Decision:** Controllers and services accept and forward `CancellationToken` to EF Core async
calls. `AuthService` is the exception (no token), since its operations are short login-time calls.

**Why:** Client disconnects should cancel in-flight database work. The exception handler maps a
client-abort `OperationCanceledException` to `499` without logging it as an error.

## Configuration: secrets out of source control

**Decision:** `appsettings.json` holds only safe shared values. Secrets (`ConnectionStrings`,
`Jwt:Key`) are supplied through User Secrets locally, environment variables in CI/production, and
are validated at startup (the app fails fast if `Jwt:Key` is missing).

**Why:** Prevents committing credentials and makes misconfiguration fail immediately instead of at
first use.

## OpenAPI only outside production

**Decision:** `MapOpenApi()` is enabled only in Development and Testing.

**Why:** The schema is useful for local work and integration tests, but there is no reason to
expose it in production.

## Out of scope for V1

The following are intentionally **not** part of V1 and are not planned here:

- rate limiting,
- refresh-token reuse detection,
- user soft delete,
- using the `OwnerNameSnapshot` / `OwnerEmailSnapshot` columns,
- additional patterns (repository, CQRS/MediatR, Clean Architecture, microservices).

They are recorded here only to make the boundary explicit. Future experiments belong outside the
V1 scope.
