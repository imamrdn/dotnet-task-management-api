# .NET BACKEND FROM SCRATCH

> 🎯 **Tujuan:** belajar .NET Backend secara bertahap dari dasar sampai mampu membuat aplikasi backend lengkap dengan CRUD, database, authentication, authorization, testing, architecture, dan deployment.

## Roadmap Besar

| Fase | Fokus | Target | Status |
|---|---|---|---|
| 0 | C# Fundamental | Paham bahasa C# | ◻️ Belum difokuskan |
| 1 | ASP.NET Core | Bisa membuat Web API | ✅ Selesai dasar |
| 2 | HTTP & Routing | Paham cara API bekerja | ✅ Selesai dasar |
| 3 | CRUD | Bisa membuat CRUD | ✅ Selesai dasar |
| 4 | Database | PostgreSQL + EF Core | ✅ Selesai dasar |
| 5 | Struktur Project | Controller → Service → DB | ✅ Selesai dasar |
| 6 | Authentication | Register, Login, JWT | ✅ Selesai dasar |
| 7 | Authorization | Role & Permission | ✅ Selesai dasar |
| 8 | API Features | Search, filter, pagination, sorting | ✅ Selesai dasar |
| 9 | Database Relationship | Relasi data yang benar + seeding | ✅ Selesai dasar |
| 10 | Testing | Unit & integration test | ✅ Selesai dasar |
| 11 | Architecture | Clean Architecture | ◻️ Belum mulai |
| 12 | Production | Docker, deployment, logging | ◻️ Belum mulai |
| Advanced | Backend Lanjutan | Redis, Queue, Microservices | ◻️ Belum mulai |

---

# Phase 0 — C# Fundamental

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

- [ ] Variable dan data type
- [ ] Conditional: `if`, `switch`
- [ ] Loop: `for`, `foreach`, `while`
- [ ] Method
- [ ] Class dan object
- [ ] Encapsulation
- [ ] Inheritance
- [ ] Polymorphism
- [ ] Interface
- [ ] Collections
- [ ] Generics
- [ ] LINQ
- [ ] Exception handling
- [ ] `async` / `await`
- [ ] Konsep Dependency Injection

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

# Phase 1 — ASP.NET Core Dasar

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

# Phase 2 — HTTP, Routing & Request

### HTTP Method

```text
GET     → mengambil data         ✅ sudah
POST    → membuat data           ✅ sudah
PUT     → update keseluruhan     ◻️ belum
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
- [ ] `500 Internal Server Error`

### Target

Benar-benar memahami bagaimana request masuk dan response keluar.

Status: ✅ dasar HTTP method, route parameter, query parameter, request body, dan response status sudah dipraktikkan.

---

# Phase 3 — CRUD Tanpa Database

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

# Phase 4 — Database

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
- [ ] `JOIN`
- [ ] `GROUP BY`
- [x] Primary Key
- [x] Foreign Key
- [x] Relationship
- [ ] Index
- [ ] Transaction
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
- [ ] `FindAsync()`
- [x] `ToListAsync()`
- [ ] `AsNoTracking()`
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
Phase 4  → pahami konsep seeding
Phase 9  → praktikkan seeding setelah User → Task relationship dibuat
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

Catatan: connection string lokal masih disimpan di `appsettings.json` lokal dan tidak ikut commit supaya password database tidak masuk Git.

---

# Phase 5 — DTO, Validation & Service

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

# Phase 6 — Authentication

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
Return Token
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
- [ ] Refresh Token
- [x] `[Authorize]`

### Target

User bisa **register → login → mendapatkan token → mengakses API protected**.

Status: ✅ selesai dasar.

Yang sudah diterapkan:

- `User` model dengan `PasswordHash`
- `POST /api/auth/register`
- `POST /api/auth/login`
- password hashing menggunakan `PasswordHasher<User>`
- JWT generation saat login berhasil
- JWT Bearer authentication middleware
- `[Authorize]` pada `TasksController`
- Bruno login menyimpan token ke `authToken`
- request task Bruno memakai Bearer auth dari `authToken`

Protected endpoint yang sudah dipraktikkan:

```text
GET     /api/tasks?page=1&limit=10
GET     /api/tasks/{id}
POST    /api/tasks
PUT     /api/tasks/{id}
DELETE  /api/tasks/{id}
```

Catatan: authorization berbasis role sudah diterapkan pada endpoint user. Permission granular dan policy-based authorization belum diterapkan.

---

# Phase 7 — Authorization

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
- [ ] Policies
- [x] `[Authorize]`
- [x] `[Authorize(Roles = "Admin")]`

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

# Phase 8 — API yang Lebih Realistis

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
- [ ] Logging
- [ ] Validation
- [ ] Configuration
- [ ] CORS
- [ ] Swagger / OpenAPI

Response sukses menggunakan `ApiResponse<T>` dengan `success`, `message`, dan `data`. Error dari validasi, authorization, dan exception menggunakan bentuk yang sama dengan `success: false`. Login mengembalikan token pada `data.token`; DELETE sukses tetap `204 No Content` tanpa body.

---

# Phase 9 — Relationship Database

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

- [ ] One-to-One
- [x] One-to-Many
- [ ] Many-to-Many
- [x] Foreign Key
- [x] Navigation Property
- [ ] `Include()`
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

# Phase 10 — Testing

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

- [ ] Service
- [ ] Controller
- [ ] Authentication
- [ ] Database Integration

---

# Phase 11 — Clean Architecture

> ⚠️ Pelajari Clean Architecture setelah CRUD, database, auth, relationship, dan testing benar-benar dipahami.

### Struktur

```text
TaskManagement/
│
├── src/
│   │
│   ├── TaskManagement.Api/
│   │
│   ├── TaskManagement.Application/
│   │
│   ├── TaskManagement.Domain/
│   │
│   └── TaskManagement.Infrastructure/
│
└── tests/
    ├── UnitTests/
    └── IntegrationTests/
```

### Alur Dependency

```text
          API
           │
           ▼
     Application
           │
           ▼
        Domain

           ▲
           │
    Infrastructure
           │
    ┌──────┴──────┐
    │             │
PostgreSQL      Redis
```

### Pelajari

- [ ] SOLID
- [ ] Repository Pattern
- [ ] Dependency Inversion
- [ ] Clean Architecture
- [ ] Domain Logic
- [ ] Use Cases

---

# Phase 12 — Production

Sekarang buat aplikasi siap berjalan di server.

### Pelajari

- [ ] Docker
- [ ] Docker Compose
- [ ] Environment Variables
- [ ] Secrets
- [ ] HTTPS
- [ ] Logging
- [ ] Health Check
- [ ] CI/CD
- [ ] Deployment

### Arsitektur Dasar

```text
        Internet
            │
            ▼
       ASP.NET API
            │
    ┌───────┴────────┐
    ▼                ▼
PostgreSQL         Redis
```

---

# Advanced .NET Backend

Jangan mulai bagian ini sebelum fondasi utama kuat.

```text
Redis
  ↓
Background Jobs
  ↓
RabbitMQ
  ↓
CQRS
  ↓
Domain Driven Design
  ↓
Event Driven Architecture
  ↓
Microservices
  ↓
gRPC
  ↓
Kubernetes
```

> **Microservices bukan target awal.** Modular monolith yang baik jauh lebih bermanfaat untuk belajar backend fundamentals.

---

# Urutan Project

Daripada membuat banyak project tutorial, gunakan beberapa project yang semakin kompleks.

## Project 1 — Task Management API

- [ ] CRUD
- [ ] PostgreSQL
- [ ] EF Core
- [ ] DTO
- [ ] Service
- [ ] JWT
- [ ] User → Tasks

```text
Task Management API
        ↓
CRUD
        ↓
PostgreSQL
        ↓
EF Core
        ↓
DTO + Service
        ↓
JWT Authentication
        ↓
User → Tasks
```

## Project 2 — E-Commerce API

- [ ] User
- [ ] Product
- [ ] Category
- [ ] Cart
- [ ] Order
- [ ] Role
- [ ] Pagination
- [ ] Search
- [ ] Validation
- [ ] Testing

## Project 3 — Production Backend

- [ ] Clean Architecture
- [ ] PostgreSQL
- [ ] Redis
- [ ] Background Jobs
- [ ] Docker
- [ ] Testing
- [ ] CI/CD
- [ ] Deployment
- [ ] Logging

---

# Timeline Belajar

Jika belajar sekitar **1–2 jam per hari**:

```text
Bulan 1
C# + ASP.NET Core + HTTP
        ↓
Bulan 2
CRUD + PostgreSQL + EF Core
        ↓
Bulan 3
Service + DTO + Auth + JWT
        ↓
Bulan 4
Relationship + Advanced API + Testing
        ↓
Bulan 5
Clean Architecture + Docker
        ↓
Bulan 6
E-Commerce + Production + Deployment
```

---

# Prinsip Belajar

1. Pelajari konsep satu per satu, jangan melompat terlalu jauh.
2. Setelah memahami konsep, langsung implementasikan pada project.
3. Jangan hanya copy-paste tutorial; coba tulis ulang tanpa melihat contoh.
4. Gunakan satu project yang terus berkembang untuk memahami hubungan antar-konsep.
5. Jangan mulai microservices sebelum fondasi backend, database, testing, dan deployment kuat.
6. Clean Architecture dipelajari setelah memahami kebutuhan yang ingin diselesaikannya.
7. Gunakan Git dan commit progress secara rutin.

---

# Alur Utama

```text
C#
 ↓
ASP.NET Core
 ↓
HTTP & Routing
 ↓
CRUD
 ↓
PostgreSQL
 ↓
Entity Framework Core
 ↓
Database Seeding
 ↓
DTO + Validation
 ↓
Service Layer
 ↓
Authentication
 ↓
JWT
 ↓
Authorization
 ↓
Database Relationships
 ↓
Pagination / Search / Filter
 ↓
Testing
 ↓
Clean Architecture
 ↓
Docker
 ↓
Deployment
 ↓
Advanced Backend
```

> Fokus utama bukan menghafal framework, tetapi memahami bagaimana sebuah request berjalan dari client sampai database dan kembali menjadi response.
