# .NET BACKEND — BEGINNER TO INTERMEDIATE

> 🎯 **Tujuan:** belajar .NET Backend secara bertahap dari beginner sampai intermediate dengan fondasi kuat pada C#, ASP.NET Core, HTTP, database, authentication, authorization, testing, dan API engineering.

## Roadmap Besar

File 01 terdiri dari **12 phase**, dinomori secara natural dari **Phase 1 sampai Phase 12**.

| Fase | Fokus | Target | Status |
|---|---|---|---|
| 1 | C# Fundamental | Paham fondasi bahasa C# | ✅ Selesai dasar |
| 2 | ASP.NET Core Dasar | Bisa membuat Web API | ✅ Selesai dasar |
| 3 | HTTP, Routing & Request | Paham request/response API | ✅ Selesai dasar |
| 4 | CRUD Tanpa Database | Menguasai hubungan HTTP dan CRUD | ✅ Selesai dasar |
| 5 | Database | PostgreSQL + EF Core dasar | ✅ Selesai dasar |
| 6 | DTO, Validation & Service | Struktur backend lebih rapi | ✅ Selesai dasar |
| 7 | Authentication | Register, login, JWT | ✅ Selesai dasar |
| 8 | Authorization | Role, claims, akses endpoint | ✅ Selesai dasar |
| 9 | API yang Lebih Realistis | Pagination, search, filter, sorting, error handling | ✅ Selesai dasar |
| 10 | Relationship Database | User → Task, ownership, seeding | ✅ Selesai dasar |
| 11 | Testing | Unit & integration testing | ✅ Selesai dasar |
| 12 | Foundation Upgrade | Advanced C#, SQL/EF Core, API design, advanced auth | ◻️ Berjalan |

---

# Phase 1 — C# Fundamental

Jangan terlalu lama di tahap ini, tetapi pastikan konsep dasar benar-benar nyaman digunakan.

```text
Variable & Data Type
       ↓
if / switch
       ↓
for / foreach / while
       ↓
Method
       ↓
Class & Object
       ↓
OOP
       ↓
Interface
       ↓
Collections
       ↓
LINQ
       ↓
Exception Handling
       ↓
async / await
       ↓
Dependency Injection concept
```

### Materi

- [x] Variable dan data type
- [x] Conditional: `if`, `switch`
- [x] Loop: `for`, `foreach`, `while`
- [x] Method
- [x] Class dan object
- [x] Encapsulation
- [x] Inheritance
- [x] Polymorphism
- [x] Interface
- [x] Collections
- [x] Generics
- [x] LINQ
- [x] Exception handling
- [x] `async` / `await`
- [x] Konsep Dependency Injection

### Target

Bisa membaca kode seperti:

```csharp
public async Task<User?> GetUserAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);

    if (user == null)
        return null;

    return user;
}
```

### Mini Project

**Todo Console App**

---

# Phase 2 — ASP.NET Core Dasar

Buat aplikasi Web API pertama.

```bash
dotnet new webapi -n TaskManagement.Api
cd TaskManagement.Api
dotnet run
```

### Pelajari

- [x] Struktur project
- [x] `Program.cs`
- [x] `appsettings.json`
- [x] Controllers
- [x] Dependency Injection
- [x] Middleware
- [x] Environment
- [x] Configuration

### Request Lifecycle

```text
Request
   ↓
ASP.NET Core
   ↓
Middleware
   ↓
Routing
   ↓
Controller
   ↓
Response
```

### Target

Bisa membuat endpoint berikut sendiri:

```text
GET /api/hello
```

Status: ✅ dasar Web API sudah dipraktikkan menggunakan Minimal API.

---

# Phase 3 — HTTP, Routing & Request

### HTTP Method

```text
GET     → mengambil data         ✅ sudah
POST    → membuat data           ✅ sudah
PUT     → update keseluruhan     ✅ sudah
PATCH   → update sebagian        ◻️ belum
DELETE  → menghapus              ✅ sudah
```

### Routing

Pelajari:

```csharp
[HttpGet]
[HttpGet("{id:int}")]
[HttpPost]
[HttpPut("{id:int}")]
[HttpDelete("{id:int}")]
```

### Route Parameter

```text
GET /api/tasks/10
               ↑
              id
```

```csharp
[FromRoute] int id
```

Status: ✅ sudah dipraktikkan di `GET /api/tasks/{id}` dan `DELETE /api/tasks/{id}`.

### Query Parameter

```text
GET /api/tasks?page=1&limit=10
                   ↑       ↑
                 query parameters
```

```csharp
[FromQuery] int page
```

Status: ✅ sudah dipraktikkan di `GET /api/tasks?page=1&limit=10`.

### Request Body

```json
{
    "title": "Belajar .NET"
}
```

```csharp
[FromBody] CreateTaskRequest request
```

Status: ✅ sudah dipraktikkan di `POST /api/tasks`.

### HTTP Status Codes

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
500 Internal Server Error
```

Status yang sudah dipraktikkan:

- [x] `200 OK`
- [x] `201 Created`
- [x] `204 No Content`
- [x] `400 Bad Request`
- [x] `401 Unauthorized`
- [x] `403 Forbidden`
- [x] `404 Not Found`
- [x] `500 Internal Server Error`

### Target

Benar-benar memahami bagaimana request masuk dan response keluar.

Status: ✅ dasar HTTP method, route parameter, query parameter, request body, dan response status sudah dipraktikkan.

---

# Phase 4 — CRUD Tanpa Database

Status: ✅ selesai dasar.

Buat model sederhana:

```csharp
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
}
```

Gunakan `List<TaskItem>` sebagai penyimpanan sementara.

### Endpoint

```text
GET     /api/tasks          ✅ sudah
GET     /api/tasks/{id}     ✅ sudah
POST    /api/tasks          ✅ sudah
PUT     /api/tasks/{id}     ✅ sudah
DELETE  /api/tasks/{id}     ✅ sudah
```

### Target

- [x] Bisa membuat CRUD tanpa database
- [x] Bisa membuat CRUD tanpa copy-paste tutorial
- [x] Mengerti hubungan HTTP method dengan operasi CRUD

Catatan: data masih disimpan di memory menggunakan `List<TaskItem>`, jadi data akan hilang ketika aplikasi restart.

### Project

**Task Management API v1**

---

# Phase 5 — Database

Setelah CRUD dipahami, baru masuk database.

Status: ✅ selesai dasar.

### Stack

```text
PostgreSQL
    +
Entity Framework Core
```

### SQL Dasar

- [x] `SELECT`
- [x] `INSERT`
- [x] `UPDATE`
- [x] `DELETE`
- [x] `WHERE`
- [x] `JOIN`
- [x] `GROUP BY`
- [x] Primary Key
- [x] Foreign Key
- [x] Relationship
- [x] Index
- [x] Transaction
- [x] Seed data

### Entity Framework Core

```text
Entity
   ↓
DbContext
   ↓
DbSet
   ↓
Migration
   ↓
Database
```

Pelajari:

- [x] `DbContext`
- [x] `DbSet<T>`
- [x] Connection String
- [x] Migration
- [x] LINQ Query
- [x] `SaveChangesAsync()`
- [x] `FindAsync()`
- [x] `ToListAsync()`
- [x] `AsNoTracking()`
- [x] Database seeding

### Database Seeding

Database seeding adalah proses mengisi data awal ke database agar aplikasi mudah dites setelah migration dijalankan.

Contoh data seed:

```text
Demo user
Demo tasks
Default roles
Default categories
```

Di ASP.NET Core + EF Core, seeding bisa dilakukan dengan beberapa cara:

```text
HasData() di OnModelCreating
Custom DatabaseSeeder class
Seeder saat aplikasi startup
Migration manual insert
```

Untuk project ini, pilihan yang paling cocok adalah **custom `DatabaseSeeder` class**, karena password user perlu di-hash dan task nanti perlu dikaitkan ke user.

Catatan urutan belajar:

```text
Phase 5  → pahami konsep seeding
Phase 10 → praktikkan seeding setelah User → Task relationship dibuat
```

Alasannya: kalau task sudah punya `UserId`, seed task harus dibuat bersama demo user agar foreign key valid.

Status: ✅ selesai dasar menggunakan custom seeders.

Seeder yang sudah diterapkan:

- demo admin `admin@mail.com`
- demo user `user@mail.com`
- password demo account di-hash
- beberapa demo task milik demo admin dan demo user
- tidak membuat data duplikat jika demo account sudah ada
- mode refresh development dengan `Database:RefreshOnStartup`

### Perubahan Arsitektur

```text
SEBELUM

Controller
    ↓
List<TaskItem>


SESUDAH

Controller
    ↓
EF Core
    ↓
PostgreSQL
```

### Target

Data tidak hilang ketika aplikasi restart.

Status: ✅ CRUD task sudah menggunakan PostgreSQL melalui Entity Framework Core.

### Configuration & User Secrets

Pada project .NET, konfigurasi bisa berasal dari beberapa sumber:

```text
appsettings.json
 ↓
appsettings.Development.json
 ↓
appsettings.Production.json
 ↓
User Secrets
 ↓
Environment Variables
```

File environment yang dipakai tergantung nilai `ASPNETCORE_ENVIRONMENT`.

```text
Development -> appsettings.json + appsettings.Development.json + User Secrets + Environment Variables
Production  -> appsettings.json + appsettings.Production.json + Environment Variables
```

Value yang dibaca belakangan akan menimpa value sebelumnya. Karena itu, `appsettings.json` cukup menyimpan struktur config dan value umum yang aman, sedangkan value sensitif disimpan di luar Git.

Contoh data sensitif:

- password database pada `ConnectionStrings:DefaultConnection`
- JWT signing key pada `Jwt:Key`

Project ini sudah menggunakan .NET User Secrets untuk config lokal.

Untuk mengaktifkan User Secrets pada project:

```bash
dotnet user-secrets init --project TaskManagement.Api
```

Untuk menyimpan connection string lokal:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=task_management_db;Username=postgres;Password=YOUR_PASSWORD" --project TaskManagement.Api
```

Untuk menyimpan JWT key lokal:

```bash
dotnet user-secrets set "Jwt:Key" "your-local-secret-key-minimal-32-characters" --project TaskManagement.Api
```

Untuk melihat User Secrets yang sudah diset:

```bash
dotnet user-secrets list --project TaskManagement.Api
```

Contoh output:

```text
ConnectionStrings:DefaultConnection = Host=localhost;Port=5432;Database=task_management_db;Username=postgres;Password=YOUR_PASSWORD
Jwt:Key = your-local-secret-key-minimal-32-characters
```

Secara fisik, User Secrets tersimpan di local machine, bukan di folder project:

```text
~/.microsoft/usersecrets/<UserSecretsId>/secrets.json
```

`UserSecretsId` bisa dilihat di file:

```text
TaskManagement.Api/TaskManagement.Api.csproj
```

Catatan penting:

- User Secrets hanya untuk local development.
- User Secrets tidak ikut commit dan tidak ikut push.
- Setiap developer perlu mengisi User Secrets di laptop masing-masing.
- Untuk server, CI, atau production, gunakan environment variables.
- Jangan menaruh password database atau JWT key asli di README, docs, Bruno environment yang di-commit, atau `appsettings.json`.

File yang digunakan project ini:

```text
TaskManagement.Api/appsettings.json
  Config umum yang aman di-commit.

TaskManagement.Api/appsettings.Development.json
  Config khusus local development.

TaskManagement.Api/appsettings.Production.json
  Config khusus production, misalnya logging lebih ringkas.

TaskManagement.Api/Properties/launchSettings.json
  Profile lokal untuk `dotnet run`, saat ini memakai `ASPNETCORE_ENVIRONMENT=Development`.
```

Contoh environment variable untuk production:

```bash
ConnectionStrings__DefaultConnection="Host=server;Port=5432;Database=task_management_db;Username=app;Password=secret"
Jwt__Key="production-secret-key-minimal-32-characters"
ASPNETCORE_ENVIRONMENT="Production"
```

---

# Phase 6 — DTO, Validation & Service

Setelah database berjalan, rapikan struktur aplikasi.

Status: ✅ selesai dasar.

```text
TaskManagement/
│
├── Controllers/
│   └── TasksController.cs
│
├── Services/
│   ├── ITaskService.cs
│   └── TaskService.cs
│
├── DTOs/
│   ├── CreateTaskRequest.cs
│   ├── UpdateTaskRequest.cs
│   └── TaskResponse.cs
│
├── Models/
│   └── TaskItem.cs
│
├── Data/
│   └── AppDbContext.cs
│
└── Program.cs
```

### Alur

```text
Request
   ↓
Controller
   ↓
DTO
   ↓
Service
   ↓
EF Core
   ↓
Database
```

### Pelajari

- [x] DTO
- [x] Request DTO
- [x] Response DTO
- [x] Validation
- [x] Service Layer
- [x] Dependency Injection
- [x] Mapping
- [x] Separation of Concerns

Catatan: endpoint task sudah dipindahkan dari Minimal API ke `TasksController`, dan logic database sudah dipindahkan ke `TaskService`.

### Project

**Task Management API v2**

---

# Phase 7 — Authentication

Setelah CRUD dan database dipahami, baru masuk authentication.

Tambahkan user:

```text
User
├── Id
├── Name
├── Email
└── PasswordHash
```

### Endpoint

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
```

### Register Flow

```text
Email + Password
       ↓
Validation
       ↓
Hash Password
       ↓
Save User
       ↓
Database
```

### Login Flow

```text
Email + Password
       ↓
Find User
       ↓
Verify Password
       ↓
Generate JWT
       ↓
Generate Refresh Token
       ↓
Return Access Token + Refresh Token
```

### Protected Endpoint

```text
GET /api/profile

Authorization: Bearer <token>
```

### Pelajari

- [x] Authentication
- [x] JWT
- [x] Claims
- [x] Password Hashing
- [x] Access Token
- [x] Refresh Token
- [x] Advanced authorization policy
- [x] `[Authorize]`

### Target

User bisa **register → login → mendapatkan token → mengakses API protected**.

Status: ✅ selesai dasar.

Yang sudah diterapkan:

- `User` model dengan `PasswordHash`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- password hashing menggunakan `PasswordHasher<User>`
- JWT generation saat login berhasil
- refresh token disimpan sebagai hash di tabel `refresh_tokens`
- refresh token rotation saat `POST /api/auth/refresh`
- refresh token revocation saat `POST /api/auth/logout`
- JWT Bearer authentication middleware
- `[Authorize]` pada `TasksController`
- policy `AdminOnly` untuk endpoint admin
- Bruno login menyimpan access token ke `authToken` dan refresh token ke `refreshToken`
- request task Bruno memakai Bearer auth dari `authToken`

Protected endpoint yang sudah dipraktikkan:

```text
GET     /api/tasks?page=1&limit=10
GET     /api/tasks/{id}
POST    /api/tasks
PUT     /api/tasks/{id}
PATCH   /api/tasks/{id}/completion
DELETE  /api/tasks/{id}
```

Catatan: authorization berbasis role dan policy `AdminOnly` sudah diterapkan. Permission granular per aksi/resource belum diterapkan karena belum dibutuhkan oleh project saat ini.

---

# Phase 8 — Authorization

Authentication menjawab:

```text
"Siapa kamu?"
```

Authorization menjawab:

```text
"Apa yang boleh kamu lakukan?"
```

### Role

```text
Admin
User
```

Contoh:

```text
User
 ↓
GET /api/tasks       ✓
POST /api/tasks      ✓
DELETE /api/users    ✕

Admin
 ↓
GET /api/tasks       ✓
POST /api/tasks      ✓
DELETE /api/users    ✓
```

### Pelajari

- [x] Roles
- [x] Claims
- [x] Policies
- [x] `[Authorize]`
- [x] `[Authorize(Roles = "Admin")]` (pernah diterapkan; sekarang dirapikan menjadi policy `AdminOnly`)

Status: ✅ selesai dasar.

Yang sudah diterapkan:

- `User.Role`
- default role `User`
- demo admin role `Admin`
- role claim di JWT
- `UsersController` hanya bisa diakses role `Admin`
- user biasa mendapat `403 Forbidden` saat mengakses `/api/users`

Catatan: permission granular seperti `users:read`, `tasks:delete`, atau policy-based authorization belum diterapkan.

---

# Phase 9 — API yang Lebih Realistis

Tambahkan fitur yang umum pada backend production.

### Pagination

```text
GET /api/tasks?page=1&limit=10
```

Status: ✅ pagination, search, filtering, dan sorting dasar sudah diterapkan pada query task.

### Search

```text
GET /api/tasks?search=belajar
```

### Filter

```text
GET /api/tasks?isCompleted=true
```

### Sorting

```text
GET /api/tasks?sortBy=title&sortDirection=desc
```

### Pelajari

- [x] Pagination
- [x] Search
- [x] Filtering
- [x] Sorting
- [x] Global Exception Handling
- [x] Logging
- [x] Validation
- [x] Configuration
- [x] CORS
- [x] Swagger / OpenAPI

Response sukses menggunakan `ApiResponse<T>` dengan `success`, `message`, dan `data`. Error dari validasi, authorization, dan exception menggunakan bentuk yang sama dengan `success: false`. Login mengembalikan token pada `data.token`; DELETE sukses tetap `204 No Content` tanpa body.

### Swagger / OpenAPI

OpenAPI adalah kontrak dokumentasi API. Bruno tetap menjadi alat utama untuk mencoba request, tetapi OpenAPI berguna untuk melihat daftar endpoint, request/response schema, dan kebutuhan auth secara formal.

Endpoint development:

```text
GET /openapi/v1.json
```

Yang sudah ditambahkan:

- API title
- API version
- API description
- JWT Bearer security scheme
- Bruno request `openapi/OPENAPI JSON`

### CORS

CORS digunakan saat API dipanggil dari browser frontend yang beda origin. Bruno tidak terkena aturan CORS, karena CORS adalah proteksi browser.

Config development yang diterapkan:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

Development mengizinkan origin frontend lokal umum. Production memakai list kosong supaya domain frontend production harus diisi secara sadar melalui config atau environment variables.

Middleware CORS dipasang sebelum authentication dan authorization:

```csharp
app.UseCors("AllowedOrigins");
app.UseAuthentication();
app.UseAuthorization();
```

### Bruno Flow

Bruno digunakan sebagai API client utama untuk belajar alur request.

Struktur flow yang sudah diterapkan:

```text
bruno/_flows/admin
  01 LOGIN ADMIN
  02 GET ALL USERS
  03 CREATE USER
  04 UPDATE USER
  05 DELETE USER

bruno/_flows/user
  01 LOGIN USER
  02 CREATE TASK
  03 GET MY TASKS
  04 GET TASK BY ID
  05 UPDATE TASK
  06 PATCH TASK COMPLETION
  07 DELETE TASK

bruno/_flows/negative
  00 LOGIN USER
  01 LOGIN WRONG PASSWORD
  02 GET TASKS WITHOUT TOKEN
  03 USER ACCESS USERS ENDPOINT
  04 CREATE TASK WITHOUT TITLE
  05 GET MISSING TASK
```

Struktur endpoint collection:

```text
bruno/admin/users
  USER LIST
  USER BY ID
  USERS WITHOUT TASKS
  CREATE USER
  UPDATE USER
  DELETE USER
  USER PROFILE
  UPSERT USER PROFILE

bruno/admin/tasks
  TASK LIST WITH OWNERS
  TASK SUMMARY BY USER
  TOP TASK OWNERS

bruno/admin/categories
  CATEGORY LIST
  CATEGORY BY ID
  CREATE CATEGORY
  UPDATE CATEGORY
  DELETE CATEGORY

bruno/tasks
  TASK LIST
  TASK BY ID
  CREATE TASK
  DELETE TASK
  PUT TASK
  PATCH TASK COMPLETION
  TASK CATEGORIES
  ASSIGN TASK CATEGORIES
  TASK SEARCH
  TASK FILTER COMPLETED
  TASK SORT

bruno/_tools/health
  HEALTH CHECK

bruno/_tools/openapi
  OPENAPI JSON
```

Tujuan flow:

- membuktikan admin hanya untuk endpoint user management
- membuktikan user hanya melihat dan mengubah task miliknya
- membuktikan error response untuk `401`, `403`, `400`, dan `404`
- membiasakan menjalankan API sebagai alur sistem, bukan endpoint terpisah

Variable yang dipakai:

```text
authToken   -> token dari login
flowUserId  -> user hasil create pada admin flow
flowTaskId  -> task hasil create pada user flow
```

---

# Phase 10 — Relationship Database

Upgrade model aplikasi agar lebih realistis.

```text
User
 │
 │ 1
 │
 │ N
 ▼
Task
```

Database:

```text
users
────────────
id
name
email


tasks
────────────
id
title
completed
user_id
```

### Pelajari

- [x] One-to-One (`User` -> `UserProfile`, satu user hanya punya satu profile)
- [x] One-to-Many
- [x] Many-to-Many (`Task` ↔ `Category` melalui tabel join `task_categories`)
- [x] Foreign Key
- [x] Navigation Property
- [x] `Include()`
- [x] Soft Delete
- [x] Audit Trail (`CreatedAt`, `UpdatedAt`, `DeletedAt`)
- [x] Database seeding dengan relasi

### Target

Setiap user hanya bisa membaca dan memodifikasi task miliknya sendiri.

Status: ✅ selesai dasar.

Yang sudah diterapkan:

- `TaskItem.UserId`
- navigation property `TaskItem.User`
- navigation property `User.Tasks`
- foreign key `tasks.UserId → users.Id`
- task list difilter berdasarkan user login
- create task otomatis memakai user dari JWT claim
- get/update/delete task hanya berlaku untuk task milik user login

Tambahkan seeding setelah relasi dibuat:

```text
Demo User
   ↓
Demo Tasks milik Demo User
```

Target seeding:

- [x] Membuat demo user jika belum ada
- [x] Hash password demo user
- [x] Membuat beberapa demo task milik demo user
- [x] Tidak membuat data duplikat setiap aplikasi restart

---

# Phase 11 — Testing

Pelajari automated testing agar aplikasi bisa diubah dengan lebih aman.

### Materi

- [x] Unit Testing
- [x] Integration Testing
- [x] Mocking
- [x] xUnit
- [x] Arrange / Act / Assert

Contoh:

```csharp
[Fact]
public async Task GetTask_WhenTaskExists_ReturnsTask()
{
    // Arrange

    // Act

    // Assert
}
```

### Test

- [x] Service
- [x] Controller
- [x] Authentication
- [x] Database Integration

---

---

# Phase 12 — Foundation Upgrade Sebelum Architecture

> Tujuan fase ini adalah menutup gap fundamental sebelum masuk ke architecture, production hardening, dan distributed systems. Beberapa materi seperti `AsNoTracking()` dan `Include()` sudah pernah dipraktikkan pada phase sebelumnya; di Phase 12 materi tersebut diperdalam dari sisi behavior, trade-off, dan dampaknya ke query/performance.

## Advanced C#

- [x] Nullable Reference Types (`<Nullable>enable</Nullable>` dan nullable warning diperlakukan sebagai error)
- [x] Lambda expression dan extension methods (`Where`, `Select`, filter task aktif)
- [x] `IEnumerable<T>` vs `IQueryable<T>` (query database sampai `ToListAsync()`, mapping setelah materialisasi)
- [x] `CancellationToken` (diteruskan dari controller ke service dan EF Core)
- [ ] `Task.WhenAll()` (belum diterapkan; belum ada operasi async independen yang aman. Hindari parallel query pada satu `DbContext`)
- [x] Async exception handling (exception dari async service ditangani global handler; request canceled tidak diperlakukan sebagai error 500)

Materi tambahan sesuai kebutuhan, bukan blocker File 01:

- [ ] Delegates
- [ ] `Func`
- [ ] `Action`
- [ ] `Predicate`
- [x] Records (DTO request/response memakai `record`, misalnya `TaskResponse`, `ApiResponse<T>`, `CategoryResponse`)
- [x] Pattern matching (`is null`, `is not null`, dan switch expression dipakai di service/exception handler)
- [ ] Generic constraints

## SQL Intermediate

- [x] `JOIN`, `GROUP BY`, `HAVING`
- [x] Aggregate query (`COUNT`, `ORDER BY COUNT`, `LIMIT`)
- [x] Subquery
- [ ] CTE (dipahami sebagai query sementara bernama untuk report/agregasi bertahap; belum diterapkan karena CRUD utama masih cukup dengan LINQ/query biasa)
- [x] Index (index task disesuaikan dengan query aktif)
- [x] Composite index (`tasks(UserId, IsDeleted, Id)` untuk filter user + soft delete + sort/detail)
- [x] Unique index (`users.Email` menjaga email tidak duplikat)
- [x] `EXPLAIN` (melihat rencana query task aktif)
- [x] `EXPLAIN ANALYZE` (membandingkan estimasi dan eksekusi aktual)
- [x] Membaca query plan (tabel kecil bisa memilih `Seq Scan`; index terlihat cocok lewat `IX_tasks_UserId_IsDeleted_Id`)
- [ ] Transaction isolation level (belum relevan diterapkan ke API; cocok untuk use case stok, booking, payment, atau claim task)
- [ ] Locking dasar (belum relevan diterapkan; dipelajari saat ada konflik update bersamaan)
- [ ] Deadlock dasar (belum relevan diterapkan; dipelajari sebagai mini-lab/database troubleshooting)
- [x] Normalization (users dan tasks dipisah; tasks menyimpan `UserId`, bukan data user duplikat)
- [x] Denormalization dasar (`tasks` menyimpan snapshot nama/email owner saat task dibuat)

## EF Core Intermediate

- [x] Tracking vs `AsNoTracking()` (read-only query memakai `AsNoTracking()`, write query tetap tracking)
- [x] Change Tracker dasar (update/delete memakai entity tracking agar `SaveChangesAsync()` mendeteksi perubahan)
- [x] `Include()` (admin task owner endpoint mengambil task bersama data user pemilik)
- [ ] `ThenInclude()` (belum relevan; project belum punya relasi bertingkat seperti `Workspace -> Projects -> Tasks`)
- [x] Projection (`Select()` membentuk DTO dan mengambil field yang dibutuhkan sebelum `ToListAsync()`)
- [x] N+1 problem (dihindari dengan `Include()`/projection, bukan query relasi di dalam loop)
- [x] Eager loading (`Include()` memuat relasi user bersama query task)
- [ ] Explicit loading (belum relevan diterapkan; kebutuhan relasi saat ini lebih jelas memakai `Include()`/projection)
- [x] Transaction (`DatabaseSeeder.RefreshAsync()` memakai `BeginTransactionAsync()` agar truncate + seed menjadi satu proses)
- [x] Membaca generated SQL (query `GET /api/tasks` diverifikasi mengambil kolom response saja, memakai `WHERE`, `ORDER BY`, dan `LIMIT/OFFSET`; tidak perlu endpoint debug di API)
- [x] Query performance (`GET /api/tasks` dievaluasi dengan `EXPLAIN ANALYZE`; data kecil memilih `Seq Scan`, index `IX_tasks_UserId_IsDeleted_Id` terbukti cocok saat sequential scan dimatikan)

Catatan:

- `AsNoTracking()` dasar sudah pernah dipraktikkan untuk read query.
- `Include()` dasar sudah pernah dipraktikkan untuk mengambil owner task.
- Phase 12 memperdalam kapan fitur tersebut dipakai, trade-off-nya, dan bagaimana melihat SQL yang dihasilkan.

## API Design & Advanced Auth

- [x] PUT vs PATCH (`PUT /api/tasks/{id}` untuk update penuh; `PATCH /api/tasks/{id}/completion` untuk update status sebagian)
- [ ] Idempotency (`GET`, `PUT`, dan `PATCH /completion` aman diulang; `POST` tidak idempotent. `Idempotency-Key` belum relevan sebelum ada operasi kritikal seperti payment/checkout)
- [x] Resource naming (endpoint utama memakai noun/resource; Bruno dipisah menjadi admin/users, admin/tasks, tasks, auth, _flows, dan _tools)
- [x] HTTP semantics (`GET`, `POST`, `PUT`, `PATCH`, `DELETE` memakai status code sesuai: 200, 201, 204, 400, 401, 403, 404)
- [ ] ProblemDetails (dipahami sebagai standar error response; project saat ini tetap memakai `ApiResponse<T>` agar kontrak response konsisten selama belajar)
- [x] Refresh Token (`/api/auth/login` mengembalikan access token + refresh token; refresh token disimpan sebagai hash di database)
- [x] Refresh-token rotation (`POST /api/auth/refresh` mencabut refresh token lama dan membuat refresh token baru)
- [x] Token revocation (`POST /api/auth/logout` mencabut refresh token)
- [x] Policy-based authorization (`AdminOnly` policy menggantikan `[Authorize(Roles = "Admin")]` di endpoint admin)

## Dipindahkan ke File 02 / Bukan Blocker File 01

- Reflection mendalam
- Attributes mendalam
- `IAsyncEnumerable<T>`
- Async streaming
- `IAsyncDisposable`
- Window functions lanjutan
- Partial index
- Optimistic concurrency mendalam
- EF Core interceptors lanjutan
- EF Core value converters lanjutan
- API versioning/deprecation strategy mendalam
- Resource-based authorization
- OAuth 2.0 mendalam
- OpenID Connect mendalam

## Target File 1

Setelah file ini selesai, kamu harus mampu:

- membangun REST API tanpa tutorial langkah demi langkah
- memahami request lifecycle ASP.NET Core
- merancang relational database
- menggunakan PostgreSQL dan EF Core
- membaca generated SQL
- mengenali query bermasalah
- menerapkan authentication
- menerapkan authorization
- menulis automated tests
- memahami Controller / DTO / Service separation
- membangun backend monolith yang kuat dan maintainable

Yang sengaja bukan syarat File 01:

- Redis
- Messaging
- RabbitMQ
- Kafka
- DDD
- CQRS
- Event-driven architecture
- Microservices
- gRPC
- Kubernetes
- Distributed systems
- .NET runtime internals mendalam
- Rate limiting
