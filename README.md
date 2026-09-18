# .NET Task Management API

Task Management API built with ASP.NET Core, PostgreSQL, Entity Framework Core, and JWT authentication. This project is used as a step-by-step backend learning project, starting from basic HTTP endpoints and growing into a more structured API with DTOs, validation, service layer, database persistence, protected endpoints, role-based access, user-owned tasks, refresh token rotation, categories, soft delete, and demo seed data.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core
- Npgsql EF Core provider
- JWT Bearer authentication
- xUnit, Moq, and EF Core InMemory for tests
- Bruno for API request collection
- GitHub Actions for CI

## Project Structure

```text
.
├── bruno/                          # Bruno API request collection
│   ├── _flows/                     # ordered request scenarios
│   │   ├── admin/
│   │   ├── negative/
│   │   └── user/
│   ├── _tools/
│   │   ├── health/
│   │   └── openapi/
│   ├── admin/                      # admin requests (users, categories, tasks)
│   ├── auth/
│   ├── tasks/
│   └── environments/
├── TaskManagement.Api/             # Web API project
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── CategoriesController.cs
│   │   ├── TasksController.cs
│   │   └── UsersController.cs
│   ├── DTOs/                       # request and response models
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Seeders/
│   ├── Errors/                     # ApiExceptionHandler, DuplicateResourceException
│   ├── Extensions/                 # ControllerExtensions (Reply helpers)
│   ├── Health/                     # DatabaseHealthCheck
│   ├── Migrations/
│   ├── Models/
│   │   ├── Category.cs
│   │   ├── RefreshToken.cs
│   │   ├── TaskCategory.cs
│   │   ├── TaskItem.cs
│   │   ├── User.cs
│   │   └── UserProfile.cs
│   ├── Services/
│   │   ├── IAuthService.cs / AuthService.cs
│   │   ├── ICategoryService.cs / CategoryService.cs
│   │   ├── ITaskService.cs / TaskService.cs
│   │   └── IUserService.cs / UserService.cs
│   ├── Validation/                 # RequestValidation (DataAnnotations-based)
│   └── Program.cs
└── TaskManagement.Api.Tests/       # xUnit test project
    ├── Controllers/
    ├── Data/
    ├── Errors/
    ├── Integration/
    ├── Services/
    └── Validation/
```

## Features

### Authentication and users

- Register user
- Login user with JWT access token
- Refresh token rotation (hashed refresh tokens, revocable)
- Logout by revoking the refresh token
- Password hashing
- Role-based user management access (Admin only)
- User profile with one-to-one relationship (upsert)
- Users without active tasks report

### Tasks

- Create task
- Get task list with pagination metadata
- Get task by id
- Update task (full update and completion-only update)
- Soft delete task (excluded from queries via an EF Core global query filter)
- User-owned task data (users only see and manage their own tasks)
- Search task by title or description
- Filter task by completion status
- Sort task list by supported fields
- Admin: list all tasks with owner data
- Admin: task summary grouped by user
- Admin: top task owners

### Categories

- Category CRUD (Admin only)
- Assign and list categories on a task (many-to-many)

### Cross-cutting

- DTO-based request and response models
- Request validation via Data Annotations
- Consistent `ApiResponse<T>` envelope
- Global exception handling
- Controller and service layer separation
- Structured logging without sensitive data
- PostgreSQL persistence with EF Core migrations
- Demo database seeding
- Health check endpoint
- OpenAPI metadata with JWT Bearer documentation
- CORS configuration per environment
- Local secrets via .NET User Secrets
- Unit and integration tests
- GitHub Actions CI

## API Endpoints

```text
POST    /api/auth/register
POST    /api/auth/login
POST    /api/auth/refresh
POST    /api/auth/logout

GET     /api/tasks?page=1&limit=10                                (requires Bearer token)
GET     /api/tasks?page=1&limit=10&search=login                   (requires Bearer token)
GET     /api/tasks?page=1&limit=10&isCompleted=true               (requires Bearer token)
GET     /api/tasks?page=1&limit=10&sortBy=id&sortDirection=desc   (requires Bearer token)
GET     /api/tasks/admin/all                                      (requires Admin role)
GET     /api/tasks/admin/summary?minimumTasks=3                   (requires Admin role)
GET     /api/tasks/admin/top-users?limit=5                        (requires Admin role)
GET     /api/tasks/{id}                                           (requires Bearer token)
POST    /api/tasks                                                (requires Bearer token)
PUT     /api/tasks/{id}                                           (requires Bearer token)
PATCH   /api/tasks/{id}/completion                                (requires Bearer token)
GET     /api/tasks/{id}/categories                                (requires Bearer token)
PUT     /api/tasks/{id}/categories                                (requires Bearer token)
DELETE  /api/tasks/{id}                                           (requires Bearer token)

GET     /api/users                   (requires Admin role)
GET     /api/users/without-tasks     (requires Admin role)
GET     /api/users/{id}              (requires Admin role)
POST    /api/users                   (requires Admin role)
PUT     /api/users/{id}              (requires Admin role)
GET     /api/users/{id}/profile      (requires Admin role)
PUT     /api/users/{id}/profile      (requires Admin role)
DELETE  /api/users/{id}              (requires Admin role)

GET     /api/categories              (requires Admin role)
GET     /api/categories/{id}         (requires Admin role)
POST    /api/categories              (requires Admin role)
PUT     /api/categories/{id}         (requires Admin role)
DELETE  /api/categories/{id}         (requires Admin role)

GET     /health
GET     /openapi/v1.json           (Development only)
```

Task endpoints are scoped to the authenticated user. A user can only list, create, update, and delete their own tasks. Admin can use the `/api/tasks/admin/*` endpoints to inspect all tasks with owner data, grouped summaries, and top task owners.

`GET /api/tasks` defaults to `page=1` and `limit=10` when the parameters are omitted, and rejects `limit` above `100` with `400`.

User management and category endpoints are scoped to users with the `Admin` role. Deleting a user is rejected with `400` while that user still has tasks.

Health check verifies that the API can connect to the database.

OpenAPI JSON is available in Development and describes the API contract, including JWT Bearer authentication.

## Local Setup

Create a PostgreSQL database:

```sql
CREATE DATABASE task_management_db;
```

Configure local secrets with .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=task_management_db;Username=YOUR_USERNAME;Password=YOUR_PASSWORD" --project TaskManagement.Api
dotnet user-secrets set "Jwt:Key" "your-local-secret-key-minimal-32-characters" --project TaskManagement.Api
```

`appsettings.json` keeps only safe shared values. Local secrets such as database passwords and JWT signing keys should stay in User Secrets or environment variables, not in Git.

Configuration is environment-specific:

```text
Development -> appsettings.json + appsettings.Development.json + User Secrets + environment variables
Production  -> appsettings.json + appsettings.Production.json + environment variables
```

The local launch profiles in `TaskManagement.Api/Properties/launchSettings.json` run with `ASPNETCORE_ENVIRONMENT=Development`. Production should provide secrets through environment variables, for example `ConnectionStrings__DefaultConnection` and `Jwt__Key`.

Development CORS allows common local frontend origins:

```text
http://localhost:3000
http://localhost:5173
```

Production CORS starts with an empty origin list. Set real frontend domains through configuration or environment variables before exposing the API to a browser client.

Apply EF Core migrations:

```bash
dotnet ef database update --project TaskManagement.Api
```

In Development, the app runs a demo seeder on startup. It creates:

```text
Admin:
Email: admin@mail.com
Password: secret123

User:
Email: user@mail.com
Password: secret123
```

The seeder also creates separate demo tasks and demo categories, and links tasks to categories.

To reset demo data on startup in Development, set:

```json
{
  "Database": {
    "RefreshOnStartup": true
  }
}
```

Run the API:

```bash
dotnet run --project TaskManagement.Api --launch-profile http
```

Default local URL:

```text
http://localhost:5298
```

## Bruno

The `bruno/` folder contains API requests for local testing. Set the environment values in:

```text
bruno/environments/local.bru
```

Common variables:

```text
baseUrl: http://localhost:5298
taskId: 1
userId: 1
categoryId: 1
flowUserId:
flowTaskId:
page: 1
limit: 10
search: login
isCompleted: true
sortBy: id
sortDirection: desc
authToken:
refreshToken:
```

Run `auth/LOGIN ADMIN` or `auth/LOGIN USER` to receive a JWT. The login requests store the response token into `authToken`, and the task/user requests use Bruno Bearer auth with that variable.

Run `_tools/health/HEALTH CHECK` to verify the API and database connection.

Run `_tools/openapi/OPENAPI JSON` to inspect the generated OpenAPI contract in Development.

The `_flows/` folder contains ordered request scenarios:

```text
_flows/admin     Login admin, list users, create user, update user, delete user
_flows/user      Login user, create task, list tasks, get task, update task, delete task
_flows/negative  Wrong password, missing token, forbidden user access, validation error, missing task
```

Run the requests in each flow from top to bottom. `_flows/admin` stores the created user ID in `flowUserId`, and `_flows/user` stores the created task ID in `flowTaskId`.

## Learning Progress

Completed so far:

- ASP.NET Core basics
- HTTP routing and status codes
- In-memory CRUD
- PostgreSQL and EF Core integration
- DTOs, validation, controller, service layer, and dependency injection
- Authentication with register, login, password hashing, and JWT
- Refresh token rotation with hashed, revocable tokens
- Protected task endpoints with `[Authorize]`
- Role-based authorization for user management
- User and task relationship
- User-scoped task CRUD
- Soft delete with an EF Core global query filter
- One-to-one user profile
- Many-to-many task categories
- Database seeding with demo admin, demo user, and demo tasks
- Pagination metadata
- Search, filter, and sorting for task list
- Admin reporting endpoints (all tasks with owners, per-user summary, top owners)
- Consistent `ApiResponse<T>` envelope
- Data Annotations validation with a shared request validator
- Controller response standardization with `Reply` helpers
- Interface-based services (including `IAuthService`)
- Global exception handling with typed exceptions
- Correct HTTP status semantics (400 vs 404 for invalid ids)
- Bruno request flows and negative cases
- Unit and integration testing
- Code coverage with Coverlet
- GitHub Actions CI
- Structured service logging
- Atomic database refresh with transaction
- Local secret configuration with .NET User Secrets
- Environment-specific configuration
- CORS configuration for browser clients
- Health check endpoint for API and database readiness
- OpenAPI metadata and JWT Bearer documentation

Next phase:

- Rate limiting

## API Response

Successful responses use a shared envelope:

```json
{
  "success": true,
  "message": "Login successful",
  "data": { "token": "<jwt>", "refreshToken": "<refresh-token>" }
}
```

Errors use `success: false`, an error message, and `data: null`. Validation, duplicate (email or category name), and invalid-id errors return `400`, invalid credentials return `401`, forbidden access returns `403`, missing resources return `404`, and unexpected exceptions return a generic `500` message. The global exception handler logs unexpected errors without exposing their details. Successful DELETE remains `204 No Content` with no body. Bruno login requests read the token from `data.token`.

Request DTOs use Data Annotations for required fields and email format. Invalid HTTP requests return the same API response envelope through ASP.NET Core model validation. Services also re-validate requests with a shared `RequestValidation` helper so non-HTTP callers are covered too.

## Logging

Services use structured `ILogger<T>` messages for successful registration and login, task changes, and user changes. Invalid credentials produce a generic warning; unexpected server errors are logged by the global exception handler. Logs include entity IDs, but never request passwords, JWTs, or connection strings.

## Testing

Run all unit tests:

```bash
dotnet test TaskManagement.slnx
```

Integration tests read the database connection string from environment variables or User Secrets. Use the same `ConnectionStrings:DefaultConnection` setup from Local Setup before running the full test suite locally.

Run tests and collect code coverage:

```bash
dotnet test TaskManagement.slnx --settings coverage.runsettings --collect:"XPlat Code Coverage"
```

The test suite covers controllers, services, authentication, JWT and refresh token flows, user-owned tasks, soft delete, categories, user profiles, request validation, the global exception handler, database seeders, HTTP authorization, migrations, PostgreSQL `ILIKE` search, and database refresh. Refresh runs `TRUNCATE` and reseeding in one transaction, so a seed failure rolls back the deletion. Integration tests create an isolated PostgreSQL database and remove it after the test run.
