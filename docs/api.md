# API Reference

Human-readable overview of the HTTP API. This is a summary, not a field-by-field copy of the
generated OpenAPI document. All endpoints and behaviors below were verified against the
controllers and services in the repository.

Base URL locally: `http://localhost:5298`

## Authentication

| Method | Endpoint | Authentication | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | none | Create a regular user (`Role = "User"`). |
| POST | `/api/auth/login` | none | Verify credentials and return an access token and a refresh token. |
| POST | `/api/auth/refresh` | none | Exchange a valid refresh token for a new access token and rotated refresh token. |
| POST | `/api/auth/logout` | none | Revoke a refresh token. |

Details:

- Register returns `200` with `data: null` (no auto-login).
- Login returns `200` with `{ token, refreshToken }` in `data`.
- Refresh rotates the token: the old refresh token is revoked and a new one is issued.
- Logout revokes the supplied refresh token; unknown tokens are ignored (still `200`).
- These endpoints do not require a Bearer token. See [authentication.md](authentication.md).

## Tasks

All task endpoints require a Bearer token. A user can only access their **own** tasks; the
service scopes every query by the authenticated user id.

| Method | Endpoint | Authentication | Description |
| --- | --- | --- | --- |
| GET | `/api/tasks` | Bearer | List the current user's tasks with pagination, search, filter, and sort. |
| GET | `/api/tasks/{id}` | Bearer | Get one of the current user's tasks. |
| POST | `/api/tasks` | Bearer | Create a task owned by the current user. |
| PUT | `/api/tasks/{id}` | Bearer | Full update of title, description, and completion. |
| PATCH | `/api/tasks/{id}/completion` | Bearer | Update only the completion flag. |
| GET | `/api/tasks/{id}/categories` | Bearer | List categories assigned to the task. |
| PUT | `/api/tasks/{id}/categories` | Bearer | Replace the task's category assignment. |
| DELETE | `/api/tasks/{id}` | Bearer | Soft delete the task. |

Query parameters for `GET /api/tasks`:

| Parameter | Default | Notes |
| --- | --- | --- |
| `page` | `1` | Must be greater than `0`. |
| `limit` | `10` | Must be greater than `0` and not greater than `100`. |
| `search` | none | Case-insensitive `ILIKE` match on title or description (PostgreSQL). |
| `isCompleted` | none | Filter by completion status. |
| `sortBy` | `id` | Supported: `title`, `iscompleted`, `id`. Anything else falls back to `id`. |
| `sortDirection` | ascending | `desc` (case-insensitive) sorts descending. |

Behavior notes:

- **Ownership.** Listing, reading, updating, and deleting are limited to tasks where
  `UserId == current user`. A task owned by someone else behaves as "not found" (`404`).
- **Soft delete.** `DELETE` marks the task deleted; it disappears from all normal queries
  (see [database.md](database.md)).
- **Category assignment.** `PUT /api/tasks/{id}/categories` replaces the full set of categories
  for the task. If any supplied category id does not exist, the request fails with `400`.
  An empty list clears the assignment.

## Categories

| Method | Endpoint | Authentication | Description |
| --- | --- | --- | --- |
| GET | `/api/categories` | Admin | List all categories (ordered by name). |
| GET | `/api/categories/{id}` | Admin | Get one category. |
| POST | `/api/categories` | Admin | Create a category. Name must be unique. |
| PUT | `/api/categories/{id}` | Admin | Rename a category. Name must be unique. |
| DELETE | `/api/categories/{id}` | Admin | Delete a category. |

## Users

All user endpoints require the `Admin` role.

| Method | Endpoint | Authentication | Description |
| --- | --- | --- | --- |
| GET | `/api/users` | Admin | List all users. |
| GET | `/api/users/without-tasks` | Admin | List users with no active tasks (a user whose only tasks are soft-deleted is included). |
| GET | `/api/users/{id}` | Admin | Get one user. |
| POST | `/api/users` | Admin | Create a user. Email must be unique. |
| PUT | `/api/users/{id}` | Admin | Update name, email, and optionally password. |
| GET | `/api/users/{id}/profile` | Admin | Get a user's profile. |
| PUT | `/api/users/{id}/profile` | Admin | Create or update a user's profile (upsert). |
| DELETE | `/api/users/{id}` | Admin | Delete a user (rejected if the user still has tasks). |

Behavior notes:

- **Delete guard.** `DELETE /api/users/{id}` returns `400` when the user still has any tasks,
  including soft-deleted ones. This prevents accidentally wiping a user's tasks. See
  [decisions.md](decisions.md).
- **Profile.** Profile is a one-to-one relation. `PUT` creates it on first call and updates it
  afterwards.

## Admin Endpoints (tasks)

These routes live under `/api/tasks` but require the `Admin` role.

| Method | Endpoint | Authentication | Description |
| --- | --- | --- | --- |
| GET | `/api/tasks/admin/all` | Admin | All active tasks with their owner (`UserResponse`). |
| GET | `/api/tasks/admin/summary` | Admin | Task counts grouped per user; optional `minimumTasks`. |
| GET | `/api/tasks/admin/top-users` | Admin | Owners with the most tasks; `limit` default `5`. |

- `summary` accepts `minimumTasks` (must be greater than `0` when supplied).
- `top-users` accepts `limit` (must be greater than `0`); default is `5`.

## Health and OpenAPI

| Method | Endpoint | Authentication | Description |
| --- | --- | --- | --- |
| GET | `/health` | none | Returns `Healthy` (200) or `Unhealthy` (503) based on the database connection. |
| GET | `/openapi/v1.json` | none | Generated OpenAPI document (Development and Testing environments only). |

## HTTP Status Codes

Confirmed from controllers, services, the exception handler, and tests:

| Status | When |
| --- | --- |
| `200 OK` | Successful reads, updates, login/refresh/logout, and list endpoints. |
| `201 Created` | Successful `POST` for tasks, users, and categories (with a `Location` header). |
| `204 No Content` | Successful `DELETE` of a task, user, or category (empty body). |
| `400 Bad Request` | Validation failures, invalid ids (`<= 0`), duplicate email/category name, delete-user-with-tasks, invalid pagination, unknown category id. |
| `401 Unauthorized` | Missing/invalid token, invalid credentials, invalid refresh token, invalid user-id claim. |
| `403 Forbidden` | Authenticated but missing the required role (e.g. non-admin calling admin endpoints). |
| `404 Not Found` | Resource does not exist or is not owned by the caller. |
| `499` | Client closed the request (custom, set by the exception handler; no body). |
| `500 Internal Server Error` | Unhandled exception; generic message, details logged only. |
| `503 Service Unavailable` | `/health` when the database is unreachable. |

## Response Format

Successful and error responses use the same envelope, `ApiResponse<T>`
(`DTOs/ApiResponse.cs`):

```json
{
  "success": true,
  "message": "Task retrieved successfully",
  "data": { "id": 1, "title": "Example", "description": "…", "isCompleted": false }
}
```

Error responses use the same shape with `success: false` and `data: null`:

```json
{
  "success": false,
  "message": "Task not found",
  "data": null
}
```

`DELETE` responses are `204 No Content` and have no body. The health endpoint returns plain text
(`Healthy` / `Unhealthy`), not the envelope.

## Pagination

`GET /api/tasks` returns a `PaginatedResponse<TaskResponse>`:

```json
{
  "items": [ /* TaskResponse[] */ ],
  "page": 1,
  "limit": 10,
  "totalItems": 42,
  "totalPages": 5
}
```

- Default `page` is `1`, default `limit` is `10`.
- Maximum `limit` is `100`; larger values are rejected with `400`.
- `totalPages` is `ceil(totalItems / limit)`.

## OpenAPI

The generated OpenAPI document is available at `/openapi/v1.json` in **Development** and
**Testing** environments (`app.MapOpenApi()` is guarded by
`IsDevelopment() || IsEnvironment("Testing")`).

The document is customized in `Program.cs` with:

- Title `Task Management API`, version `v1`.
- A `Bearer` HTTP security scheme (JWT) applied globally.

In Production, the OpenAPI endpoint is not mapped. The Bruno collection under `bruno/` provides
ready-made requests for local exploration (see the README).
