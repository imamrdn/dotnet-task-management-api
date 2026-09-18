# Database

Data model and EF Core behavior. Verified against the entities, `AppDbContext`, and migrations.

## Database Overview

- **Database:** PostgreSQL.
- **ORM:** Entity Framework Core 10 via the Npgsql provider
  (`Npgsql.EntityFrameworkCore.PostgreSQL`).
- **Schema management:** EF Core migrations in `Migrations/` (see the end of this document).
- **Connection string:** `ConnectionStrings:DefaultConnection`.

All table names are explicitly configured in `AppDbContext.OnModelCreating` (snake_case for the
junction table and plural names elsewhere).

## Main Entities

| Entity | Table | Purpose |
| --- | --- | --- |
| `User` | `users` | Account: name, email, password hash, role. |
| `TaskItem` | `tasks` | A task owned by a user, with completion and soft-delete fields. |
| `Category` | `categories` | A category label (unique name). |
| `TaskCategory` | `task_categories` | Join entity for the task↔category many-to-many. |
| `UserProfile` | `user_profiles` | Optional one-to-one profile (bio, location). |
| `RefreshToken` | `refresh_tokens` | Hashed refresh tokens with revocation state. |

Key properties:

- `User`: `Id`, `Name`, `Email`, `PasswordHash`, `Role` (default `"User"`).
- `TaskItem`: `Id`, `Title`, `Description`, `IsCompleted`, `IsDeleted`, `OwnerNameSnapshot`,
  `OwnerEmailSnapshot`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `UserId`.
- `Category`: `Id`, `Name`.
- `TaskCategory`: `TaskItemId`, `CategoryId` (composite key).
- `UserProfile`: `Id`, `Bio`, `Location`, `CreatedAt`, `UpdatedAt`, `UserId`.
- `RefreshToken`: `Id`, `TokenHash`, `ExpiresAt`, `CreatedAt`, `RevokedAt`,
  `ReplacedByTokenHash`, `UserId`.

## Relationships

```text
User
 ├── Tasks            (one-to-many)  User 1 ── * TaskItem
 ├── UserProfile      (one-to-one)   User 1 ── 1 UserProfile
 └── RefreshTokens    (one-to-many)  User 1 ── * RefreshToken

TaskItem
 └── TaskCategories   (one-to-many)  TaskItem 1 ── * TaskCategory
                                        TaskCategory * ── 1 Category
Category
 └── TaskCategories   (one-to-many)  Category 1 ── * TaskCategory
```

- **User → Tasks:** one-to-many, foreign key `TaskItem.UserId`, `DeleteBehavior.Restrict`.
- **User → UserProfile:** one-to-one, foreign key `UserProfile.UserId` (unique index).
- **User → RefreshTokens:** one-to-many, foreign key `RefreshToken.UserId` (cascade delete at
  the database level).
- **TaskItem ↔ Category:** many-to-many through `TaskCategory` (composite key
  `{ TaskItemId, CategoryId }`), both foreign keys cascade on delete.

## Constraints

Primary keys, unique indexes, and foreign keys confirmed from migrations:

| Table | Constraint | Details |
| --- | --- | --- |
| `users` | PK | `Id` |
| `users` | Unique index | `Email` (`IX_users_Email`) |
| `tasks` | PK | `Id` |
| `tasks` | Index | `UserId` (`IX_tasks_UserId`) |
| `tasks` | Index | `(UserId, IsDeleted, Id)` (`IX_tasks_UserId_IsDeleted_Id`) |
| `tasks` | FK | `UserId` → `users.Id`, `OnDelete = Restrict` |
| `categories` | PK | `Id` |
| `categories` | Unique index | `Name` |
| `task_categories` | PK | composite `(TaskItemId, CategoryId)` |
| `task_categories` | FK | `TaskItemId` → `tasks.Id` (cascade), `CategoryId` → `categories.Id` (cascade) |
| `user_profiles` | PK | `Id` |
| `user_profiles` | Unique index | `UserId` (enforces one-to-one) |
| `user_profiles` | FK | `UserId` → `users.Id` (cascade) |
| `refresh_tokens` | PK | `Id` |
| `refresh_tokens` | Unique index | `TokenHash` |
| `refresh_tokens` | Index | `UserId` |
| `refresh_tokens` | FK | `UserId` → `users.Id` (cascade) |

Defaults configured in `AppDbContext`:

- `User.Role` defaults to `"User"`.
- `TaskItem.CreatedAt` defaults to SQL `now()`.
- `UserProfile.CreatedAt` defaults to SQL `now()`.

The `(UserId, IsDeleted, Id)` index supports the common task-list query (filter by owner, exclude
deleted, order by id).

## Soft Delete

Tasks are soft-deleted:

- `TaskItem.IsDeleted` (bool, default `false`) and `TaskItem.DeletedAt` (nullable timestamp).
- `DELETE /api/tasks/{id}` sets both (`TaskService.DeleteTaskAsync`).

Filtering is automatic through **EF Core global query filters**:

```csharp
modelBuilder.Entity<TaskItem>()
    .HasQueryFilter(task => !task.IsDeleted);

modelBuilder.Entity<TaskCategory>()
    .HasQueryFilter(taskCategory => !taskCategory.TaskItem.IsDeleted);
```

Because these filters are global, soft-deleted tasks (and their category links) are excluded from
every query without any manual filtering. `TaskCategory` has a matching filter because EF Core
recommends filtering both ends of a required relationship when one side is filtered.

`IgnoreQueryFilters()` is used deliberately in one place — `UserService.DeleteUserAsync` — to count
**all** tasks of a user (including soft-deleted) before deciding whether the user can be deleted.
See [decisions.md](decisions.md).

Note: `Category`, `User`, `UserProfile`, and `RefreshToken` are **not** soft-deleted. Category and
user deletes are hard deletes; user deletion is guarded (see below).

## Delete Behavior

| Relationship | Behavior | Rationale |
| --- | --- | --- |
| `TaskItem → User` | `Restrict` | Prevent cascading deletion of a user's tasks. |
| `UserProfile → User` | Cascade | A profile has no meaning without its user. |
| `RefreshToken → User` | Cascade | Tokens belong to a user. |
| `TaskCategory → TaskItem` / `→ Category` | Cascade | Join rows are meaningless without either side. |

`UserService.DeleteUserAsync` additionally refuses to delete a user that still has tasks
(`ArgumentException` → `400`). The `Restrict` foreign key acts as a database-level backstop.

## Audit and Snapshot Fields

- `TaskItem.CreatedAt` is set on creation (also has a SQL `now()` default).
- `TaskItem.UpdatedAt` is set on updates.
- `TaskItem.DeletedAt` is set on soft delete.
- `TaskItem.OwnerNameSnapshot` and `OwnerEmailSnapshot` are written on task creation
  (`TaskService.CreateTaskAsync`) and by the seeder. **Current behavior:** these two columns are
  written but not read anywhere — the admin report `TaskWithOwnerResponse` reads the owner's
  live `Name`/`Email` from the `User` navigation instead. The columns exist in the schema but are
  not currently used by any query or response. This is documented as-is and not changed.

## Migrations

Migrations live in `Migrations/` and are applied with:

```bash
dotnet ef database update --project TaskManagement.Api
```

In order:

```text
InitialCreate            create tasks
AddUsers                 create users
AddUserTaskRelationship  tasks.UserId + FK to users (cascade)
AddUserRole              users.Role (default "User")
AddUserAndTaskIndexes    unique users.Email, tasks (UserId, Id)
AddTaskSoftDelete        tasks.IsDeleted
AddTaskAuditTrail        tasks.CreatedAt / UpdatedAt / DeletedAt
AddActiveTaskIndex       tasks (UserId, IsDeleted, Id)
AddTaskOwnerSnapshot     tasks.OwnerNameSnapshot / OwnerEmailSnapshot (+ backfill)
AddRefreshTokens         create refresh_tokens
AddUserProfiles          create user_profiles
AddTaskCategories        create categories + task_categories
RestrictUserTaskDelete   tasks.UserId FK cascade → restrict
```

Integration tests apply these migrations to a temporary PostgreSQL database (see
[testing.md](testing.md)).
