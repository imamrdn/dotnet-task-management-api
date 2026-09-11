# .NET Task Management API

Task Management API built with ASP.NET Core, PostgreSQL, and Entity Framework Core. This project is used as a step-by-step backend learning project, starting from basic HTTP endpoints and growing into a more structured API with DTOs, validation, service layer, and database persistence.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core
- Npgsql EF Core provider
- Bruno for API request collection

## Project Structure

```text
.
├── bruno/
│   ├── CREATE TASK.bru
│   ├── DELETE TASK.bru
│   ├── PUT TASK.bru
│   ├── TASK BY ID.bru
│   └── TASK LIST.bru
└── TaskManagement.Api/
    ├── Controllers/
    │   └── TasksController.cs
    ├── DTOs/
    │   ├── CreateTaskRequest.cs
    │   ├── TaskResponse.cs
    │   └── UpdateTaskRequest.cs
    ├── Data/
    │   └── AppDbContext.cs
    ├── Migrations/
    ├── Models/
    │   └── TaskItem.cs
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
- Manual request validation
- DTO-based request and response models
- Controller and service layer separation
- PostgreSQL persistence with EF Core migrations

## API Endpoints

```text
GET     /api/tasks?page=1&limit=10
GET     /api/tasks/{id}
POST    /api/tasks
PUT     /api/tasks/{id}
DELETE  /api/tasks/{id}
```

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
  }
}
```

Apply EF Core migrations:

```bash
dotnet ef database update --project TaskManagement.Api
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
```

## Learning Progress

Completed so far:

- ASP.NET Core basics
- HTTP routing and status codes
- In-memory CRUD
- PostgreSQL and EF Core integration
- DTOs, validation, controller, service layer, and dependency injection

Next phase:

- Authentication with register, login, and JWT
