# .NET Task Management API

Task Management API built with ASP.NET Core, PostgreSQL, Entity Framework Core, and JWT authentication. This project is used as a step-by-step backend learning project, starting from basic HTTP endpoints and growing into a more structured API with DTOs, validation, service layer, database persistence, protected endpoints, user-owned tasks, and demo seed data.

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
│   ├── CREATE TASK.bru
│   ├── DELETE TASK.bru
│   ├── LOGIN.bru
│   ├── PUT TASK.bru
│   ├── REGISTER.bru
│   ├── TASK BY ID.bru
│   └── TASK LIST.bru
└── TaskManagement.Api/
    ├── Controllers/
    │   ├── AuthController.cs
    │   └── TasksController.cs
    ├── DTOs/
    │   ├── AuthResponse.cs
    │   ├── CreateTaskRequest.cs
    │   ├── LoginRequest.cs
    │   ├── RegisterRequest.cs
    │   ├── TaskResponse.cs
    │   └── UpdateTaskRequest.cs
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── DatabaseSeeder.cs
    ├── Migrations/
    ├── Models/
    │   ├── TaskItem.cs
    │   └── User.cs
    ├── Services/
    │   ├── ITaskService.cs
    │   └── TaskService.cs
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
- User-owned task data
- User and task database relationship
- Demo database seeding
- Basic pagination for task list
- Manual request validation
- DTO-based request and response models
- Controller and service layer separation
- PostgreSQL persistence with EF Core migrations

## API Endpoints

```text
POST    /api/auth/register
POST    /api/auth/login

GET     /api/tasks?page=1&limit=10   (requires Bearer token)
GET     /api/tasks/{id}              (requires Bearer token)
POST    /api/tasks                   (requires Bearer token)
PUT     /api/tasks/{id}              (requires Bearer token)
DELETE  /api/tasks/{id}              (requires Bearer token)
```

Task endpoints are scoped to the authenticated user. A user can only list, create, update, and delete their own tasks.

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

The app runs a demo seeder on startup. If `demo@example.com` does not exist, it creates:

```text
Email: demo@example.com
Password: secret123
```

The seeder also creates demo tasks owned by that demo user.

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
authToken:
```

Run `LOGIN` to receive a JWT. The login request stores the response token into `authToken`, and the task requests use Bruno Bearer auth with that variable.

## Learning Progress

Completed so far:

- ASP.NET Core basics
- HTTP routing and status codes
- In-memory CRUD
- PostgreSQL and EF Core integration
- DTOs, validation, controller, service layer, and dependency injection
- Authentication with register, login, password hashing, and JWT
- Protected task endpoints with `[Authorize]`
- User and task relationship
- User-scoped task CRUD
- Database seeding with demo user and demo tasks

Next phase:

- API features such as search, filtering, sorting, better pagination metadata, and automated tests
