# .NET Task Management API

Task Management API built with ASP.NET Core, PostgreSQL, Entity Framework Core, and JWT authentication. This project is used as a step-by-step backend learning project, starting from basic HTTP endpoints and growing into a more structured API with DTOs, validation, service layer, database persistence, protected endpoints, role-based access, user-owned tasks, and demo seed data.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core
- Npgsql EF Core provider
- JWT Bearer authentication
- Bruno for API request collection

## Project Structure

```text
.
├── bruno/
│   ├── auth/
│   ├── flows/
│   │   ├── admin/
│   │   ├── negative/
│   │   └── user/
│   ├── health/
│   ├── tasks/
│   └── users/
└── TaskManagement.Api/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── TasksController.cs
    │   └── UsersController.cs
    ├── DTOs/
    │   ├── AuthResponse.cs
    │   ├── CreateTaskRequest.cs
    │   ├── LoginRequest.cs
    │   ├── PaginatedResponse.cs
    │   ├── RegisterRequest.cs
    │   ├── TaskResponse.cs
    │   ├── UpdateTaskRequest.cs
    │   ├── UserRequest.cs
    │   └── UserResponse.cs
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Seeders/
    ├── Migrations/
    ├── Models/
    │   ├── TaskItem.cs
    │   └── User.cs
    ├── Services/
    │   ├── IAuthService.cs
    │   ├── ITaskService.cs
    │   ├── IUserService.cs
    │   ├── AuthService.cs
    │   ├── TaskService.cs
    │   └── UserService.cs
    └── Program.cs
```

## Features

- Create task
- Get task list
- Get task by id
- Update task
- Delete task
- Register user
- Login user
- Password hashing
- JWT token generation
- JWT-protected task endpoints
- Role-based user management access
- User-owned task data
- User and task database relationship
- Demo database seeding
- Pagination metadata for task list
- Search task by title or description
- Filter task by completion status
- Sort task list by supported fields
- Manual request validation
- DTO-based request and response models
- Controller and service layer separation
- PostgreSQL persistence with EF Core migrations

## API Endpoints

```text
POST    /api/auth/register
POST    /api/auth/login

GET     /api/tasks?page=1&limit=10                                (requires Bearer token)
GET     /api/tasks?page=1&limit=10&search=login                   (requires Bearer token)
GET     /api/tasks?page=1&limit=10&isCompleted=true               (requires Bearer token)
GET     /api/tasks?page=1&limit=10&sortBy=id&sortDirection=desc   (requires Bearer token)
GET     /api/tasks/{id}                                           (requires Bearer token)
POST    /api/tasks                                                (requires Bearer token)
PUT     /api/tasks/{id}                                           (requires Bearer token)
DELETE  /api/tasks/{id}                                           (requires Bearer token)

GET     /api/users                   (requires Admin role)
GET     /api/users/{id}              (requires Admin role)
POST    /api/users                   (requires Admin role)
PUT     /api/users/{id}              (requires Admin role)
DELETE  /api/users/{id}              (requires Admin role)

GET     /health
```

Task endpoints are scoped to the authenticated user. A user can only list, create, update, and delete their own tasks.

User management endpoints are scoped to users with the `Admin` role.

Health check verifies that the API can connect to the database.

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

The seeder also creates separate demo tasks for each account.

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
flowUserId:
flowTaskId:
page: 1
limit: 10
search: login
isCompleted: true
sortBy: id
sortDirection: desc
authToken:
```

Run `auth/LOGIN ADMIN` or `auth/LOGIN USER` to receive a JWT. The login requests store the response token into `authToken`, and the task/user requests use Bruno Bearer auth with that variable.

Run `health/HEALTH CHECK` to verify the API and database connection.

The `flows/` folder contains ordered request scenarios:

```text
flows/admin     Login admin, list users, create user, update user, delete user
flows/user      Login user, create task, list tasks, get task, update task, delete task
flows/negative  Wrong password, missing token, forbidden user access, validation error, missing task
```

Run the requests in each flow from top to bottom. `flows/admin` stores the created user ID in `flowUserId`, and `flows/user` stores the created task ID in `flowTaskId`.

## Learning Progress

Completed so far:

- ASP.NET Core basics
- HTTP routing and status codes
- In-memory CRUD
- PostgreSQL and EF Core integration
- DTOs, validation, controller, service layer, and dependency injection
- Authentication with register, login, password hashing, and JWT
- Protected task endpoints with `[Authorize]`
- Role-based authorization for user management
- User and task relationship
- User-scoped task CRUD
- Database seeding with demo admin, demo user, and demo tasks
- Pagination metadata
- Search, filter, and sorting for task list
- Consistent `ApiResponse<T>` envelope
- Global exception handling
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

Next phase:

- Swagger / OpenAPI review

## API Response

Successful responses use a shared envelope:

```json
{
  "success": true,
  "message": "Login successful",
  "data": { "token": "<jwt>" }
}
```

Errors use `success: false`, an error message, and `data: null`. Validation and duplicate email errors return `400`, invalid credentials return `401`, forbidden access returns `403`, missing resources return `404`, and unexpected exceptions return a generic `500` message. The global exception handler logs unexpected errors without exposing their details. Successful DELETE remains `204 No Content` with no body. Bruno login requests read the token from `data.token`.

Request DTOs use Data Annotations for required fields and email format. Invalid HTTP requests return the same API response envelope through ASP.NET Core model validation.

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

The test suite covers controllers, services, authentication, JWT generation, user-owned tasks, database seeders, HTTP authorization, migrations, PostgreSQL `ILIKE` search, and database refresh. Refresh runs `TRUNCATE` and reseeding in one transaction, so a seed failure rolls back the deletion. Integration tests create an isolated PostgreSQL database and remove it after the test run.
