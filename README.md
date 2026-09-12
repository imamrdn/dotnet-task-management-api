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
```

Task endpoints are scoped to the authenticated user. A user can only list, create, update, and delete their own tasks.

User management endpoints are scoped to users with the `Admin` role.

## Local Setup

Create a PostgreSQL database:

```sql
CREATE DATABASE task_management_db;
```

Configure the connection string in `TaskManagement.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=task_management_db;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "super-secret-key-minimal-32-characters",
    "Issuer": "TaskManagement.Api",
    "Audience": "TaskManagement.Api"
  }
}
```

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
page: 1
limit: 10
search: login
isCompleted: true
sortBy: id
sortDirection: desc
authToken:
```

Run `auth/LOGIN ADMIN` or `auth/LOGIN USER` to receive a JWT. The login requests store the response token into `authToken`, and the task/user requests use Bruno Bearer auth with that variable.

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

Next phase:

- API response wrapper, better validation, global error handling, and automated tests
