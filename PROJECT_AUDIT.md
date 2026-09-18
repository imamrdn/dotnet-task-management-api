# PROJECT AUDIT — Task Management API

> Audit ini dibuat berdasarkan pembacaan source code yang benar-benar ada di repository.
> Tidak ada source code yang diubah, tidak ada migration yang dijalankan, dan tidak ada file lain yang dibuat.
> Semua kesimpulan di bawah disertai bukti berupa file/class dari project ini.

---

## Project Overview

### Ini aplikasi apa?

Project ini adalah **REST Web API** bernama **Task Management API**, dibangun dengan **ASP.NET Core**.
Fungsinya: mengelola *task* (tugas) milik user, lengkap dengan autentikasi, hak akses berbasis role,
kategori, dan profil user.

Berdasarkan `README.md` dan kode yang ada, project ini dipakai sebagai **project latihan backend bertahap**
mulai dari endpoint HTTP sederhana, lalu berkembang ke: DTO, validasi, service layer, persistensi database,
endpoint terproteksi, role-based access, task milik user, sampai demo seed data.

### Tujuan aplikasi (berdasarkan kode)

- User bisa register & login (`Controllers/AuthController.cs`).
- User yang login bisa CRUD **task miliknya sendiri** (`Controllers/TasksController.cs`).
- Admin bisa mengelola **user** dan **kategori**, serta melihat data lintas user (`Controllers/UsersController.cs`, `Controllers/CategoriesController.cs`).
- Ada health check database (`Health/DatabaseHealthCheck.cs`) dan OpenAPI document (`Program.cs`).

### Tech Stack

| Aspek | Nilai | Bukti |
| --- | --- | --- |
| Jenis aplikasi | ASP.NET Core Web API | `TaskManagement.Api/TaskManagement.Api.csproj` (`Microsoft.NET.Sdk.Web`) |
| Versi .NET | **.NET 10** (`net10.0`) | `TaskManagement.Api.csproj` baris `<TargetFramework>net10.0</TargetFramework>` |
| Database | **PostgreSQL** | `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 di `.csproj` |
| ORM | Entity Framework Core 10 | `Microsoft.EntityFrameworkCore.Design` 10.0.12 |
| Auth | JWT Bearer | `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12 |
| OpenAPI | Microsoft.AspNetCore.OpenApi | `Microsoft.AspNetCore.OpenApi` 10.0.11 |
| Testing | xUnit + Moq + EF InMemory + Mvc.Testing | `TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj` |
| API client manual | Bruno | folder `bruno/` |
| CI | GitHub Actions | `.github/workflows/ci.yml` |

Catatan penting: `Nullable` di-`enable` dan `WarningsAsErrors>nullable` di kedua `.csproj`.
Artinya peringatan nullability dianggap error. Ini praktik yang bagus dan disengaja.

### Cara menjalankan (dari repository)

Dari `README.md`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=task_management_db;Username=...;Password=..." --project TaskManagement.Api
dotnet user-secrets set "Jwt:Key" "your-local-secret-key-minimal-32-characters" --project TaskManagement.Api
dotnet ef database update --project TaskManagement.Api
dotnet run --project TaskManagement.Api --launch-profile http
```

URL default lokal: `http://localhost:5298` (lihat `TaskManagement.Api/Properties/launchSettings.json`).

Di environment **Development**, app menjalankan demo seeder saat startup (`Program.cs` baris 113–128).
Akun demo: `admin@mail.com` / `user@mail.com` dengan password `secret123` (`Data/Seeders/UserSeeder.cs`).

### Struktur Project

```text
dotnet-task-management-api/
├── TaskManagement.slnx              # solution (format .slnx baru)
├── README.md
├── coverage.runsettings             # konfigurasi code coverage
├── .github/workflows/ci.yml         # CI: restore + test (pakai service PostgreSQL 16)
├── bruno/                           # kumpulan request API manual (auth, tasks, admin, flows)
├── TaskManagement.Api/              # project utama (Web API)
│   ├── Program.cs                   # composition root: DI, auth, CORS, middleware, seeding
│   ├── appsettings.json             # config shared (kosong untuk secret)
│   ├── appsettings.Development.json # CORS dev, RefreshOnStartup
│   ├── appsettings.Production.json  # CORS kosong, log Warning
│   ├── appsettings.Testing.json     # CORS untuk environment Testing
│   ├── Properties/launchSettings.json
│   ├── Controllers/                 # endpoint HTTP
│   │   ├── AuthController.cs
│   │   ├── TasksController.cs
│   │   ├── UsersController.cs
│   │   └── CategoriesController.cs
│   ├── DTOs/                        # request/response models (record)
│   ├── Services/                    # business logic (interface + implementasi)
│   ├── Models/                      # entity EF Core
│   ├── Data/                        # AppDbContext + Seeders
│   │   ├── AppDbContext.cs
│   │   └── Seeders/                 # DatabaseSeeder, UserSeeder, TaskSeeder, CategorySeeder
│   ├── Migrations/                  # EF Core migrations
│   ├── Errors/                      # ApiExceptionHandler (global exception handler)
│   ├── Extensions/                  # TaskQueryExtensions (WhereActive)
│   └── Health/                      # DatabaseHealthCheck
└── TaskManagement.Api.Tests/        # project test
    ├── Controllers/                 # unit test controller (Moq)
    ├── Services/                    # unit test service (EF InMemory)
    ├── Integration/                 # integration test (WebApplicationFactory + Postgres)
    ├── Data/                        # test seeder
    ├── Errors/                      # test exception handler
    ├── TestDbContextFactory.cs
    └── TestLogger.cs
```

### Tanggung jawab tiap layer

| Layer | Tanggung jawab | Contoh file |
| --- | --- | --- |
| `Controllers/` | Menerima HTTP request, validasi ringan, memanggil service, membentuk HTTP response | `TasksController.cs` |
| `Services/` | Business logic, akses database lewat `AppDbContext`, logging | `TaskService.cs`, `AuthService.cs` |
| `Models/` | Entity yang dipetakan ke tabel database | `TaskItem.cs`, `User.cs` |
| `DTOs/` | Bentuk data masuk/keluar API (bukan entity) | `CreateTaskRequest.cs`, `TaskResponse.cs` |
| `Data/` | `DbContext` + seeding | `AppDbContext.cs` |
| `Data/Seeders/` | Mengisi data demo | `DatabaseSeeder.cs` |
| `Migrations/` | Riwayat skema database | `20260910141500_InitialCreate.cs` |
| `Errors/` | Penanganan exception global → response JSON | `ApiExceptionHandler.cs` |
| `Extensions/` | Helper query yang dipakai berulang | `TaskQueryExtensions.cs` |
| `Health/` | Health check koneksi database | `DatabaseHealthCheck.cs` |

Arsitektur yang terlihat: **Controller → Service → DbContext (EF Core) → PostgreSQL**.
Tidak ada layer Repository terpisah — service langsung memakai `AppDbContext`. Ini wajar dan tidak perlu diubah.

---

## How The Application Works

### Alur umum (Web API)

```text
HTTP Request
  → Middleware (ExceptionHandler → StatusCodePages → HttpsRedirection → CORS → Authentication → Authorization)
  → Controller (Controllers/*.cs)
  → Service (Services/*.cs)
  → AppDbContext (Data/AppDbContext.cs)
  → PostgreSQL
  → Service mengembalikan DTO
  → Controller membungkus dalam ApiResponse<T>
  → HTTP Response (JSON)
```

Semua endpoint mengembalikan bentuk seragam `ApiResponse<T>` (`DTOs/ApiResponse.cs`) yang berisi
`Success`, `Message`, dan `Data`.

### Contoh flow nyata #1 — Login (`POST /api/auth/login`)

1. `AuthController.Login` menerima `LoginRequest` (`Controllers/AuthController.cs` baris 25–30).
2. Controller memanggil `AuthService.LoginAsync` (`Services/AuthService.cs` baris 66).
3. `AuthService` mencari user berdasarkan email lewat `_dbContext.Users.FirstOrDefaultAsync(...)`.
4. Password diverifikasi dengan `PasswordHasher<User>.VerifyHashedPassword`.
   Jika gagal → `throw new UnauthorizedAccessException("Invalid email or password")`.
5. Jika berhasil → `GenerateJwtToken(user)` membuat access token (berlaku 1 jam),
   lalu membuat refresh token acak, **di-hash** (`HashRefreshToken`), dan disimpan ke tabel `refresh_tokens`.
6. `AuthService` mengembalikan `AuthResponse(Token, RefreshToken)`.
7. `AuthController` mengembalikan `Ok(ApiResponse<AuthResponse>.Ok(...))`.

Jika kredensial salah, exception naik ke `Errors/ApiExceptionHandler.cs`, yang memetakan
`UnauthorizedAccessException` → HTTP **401** dengan body `ApiResponse` yang sama.

### Contoh flow nyata #2 — Membuat task (`POST /api/tasks`)

1. Request masuk ke `TasksController.CreateTask` (`Controllers/TasksController.cs` baris 100–108).
   Karena controller diberi `[Authorize]`, middleware Authentication memvalidasi JWT lebih dulu.
2. `CreateTaskRequest` divalidasi otomatis oleh `[ApiController]` berdasarkan DataAnnotations
   (`[Required]` di `DTOs/CreateTaskRequest.cs`).
3. Controller mengambil `userId` dari claim JWT lewat `GetUserIdFromClaims()`
   (membaca `ClaimTypes.NameIdentifier`).
4. `TaskService.CreateTaskAsync(userId, request, cancellationToken)` dipanggil (`Services/TaskService.cs` baris 191).
5. Service mengambil data owner dari tabel `users`, membuat `TaskItem` baru
   (menyimpan juga `OwnerNameSnapshot` & `OwnerEmailSnapshot`), lalu `_dbContext.Tasks.Add(task)`
   dan `SaveChangesAsync`.
6. Controller mengembalikan `Created(...)` → HTTP **201**.

### Contoh flow nyata #3 — Soft delete task (`DELETE /api/tasks/{id}`)

1. `TasksController.DeleteTask` → `TaskService.DeleteTaskAsync`.
2. Service **tidak** menghapus baris; ia hanya set `IsDeleted = true` dan `DeletedAt = DateTime.UtcNow`
   (`Services/TaskService.cs` baris 350–369).
3. Semua query task memakai extension `WhereActive()` (`Extensions/TaskQueryExtensions.cs`) yang
   menambahkan `WHERE IsDeleted = false`, sehingga task yang "dihapus" tidak muncul lagi.

---

## Existing Features

Semua fitur di bawah benar-benar ada di kode (bukan hanya di README).

| # | Fitur | Endpoint | File/Class utama | Status | Dependency penting |
| --- | --- | --- | --- | --- | --- |
| 1 | Register user | `POST /api/auth/register` | `AuthController`, `AuthService.RegisterAsync` | Selesai | `PasswordHasher<User>` |
| 2 | Login + JWT | `POST /api/auth/login` | `AuthController`, `AuthService.LoginAsync`, `GenerateJwtToken` | Selesai | JwtBearer, `Jwt:Key/Issuer/Audience` |
| 3 | Refresh token (rotation) | `POST /api/auth/refresh` | `AuthService.RefreshAsync`, `Models/RefreshToken.cs` | Selesai | tabel `refresh_tokens`, hashing SHA256 |
| 4 | Logout (revoke refresh token) | `POST /api/auth/logout` | `AuthService.LogoutAsync` | Selesai | tabel `refresh_tokens` |
| 5 | Password hashing | — | `AuthService`, `UserService`, `UserSeeder` | Selesai | `PasswordHasher<User>` |
| 6 | List task milik sendiri + pagination + search + filter + sort | `GET /api/tasks` | `TasksController.GetTasks`, `TaskService.GetTasksAsync` | Selesai | EF `ILike` (Postgres), `PaginatedResponse<T>` |
| 7 | Get task by id (scoped ke owner) | `GET /api/tasks/{id}` | `TasksController.GetTaskById` | Selesai | soft delete |
| 8 | Create task | `POST /api/tasks` | `TasksController.CreateTask` | Selesai | validasi DataAnnotations |
| 9 | Update task (full) | `PUT /api/tasks/{id}` | `TasksController.UpdateTask` | Selesai | — |
| 10 | Update completion (partial) | `PATCH /api/tasks/{id}/completion` | `TasksController.UpdateTaskCompletion` | Selesai | — |
| 11 | Soft delete task | `DELETE /api/tasks/{id}` | `TasksController.DeleteTask`, `TaskService.DeleteTaskAsync` | Selesai | `WhereActive()` |
| 12 | Admin: lihat semua task + owner | `GET /api/tasks/admin/all` | `TaskService.GetAllTasksWithOwnersAsync` | Selesai | policy `AdminOnly` |
| 13 | Admin: ringkasan task per user | `GET /api/tasks/admin/summary` | `TaskService.GetTaskSummaryByUserAsync` | Selesai | LINQ `GroupBy` |
| 14 | Admin: top task owners | `GET /api/tasks/admin/top-users` | `TaskService.GetTopTaskOwnersAsync` | Selesai | LINQ `GroupBy` + `Take` |
| 15 | Assign & lihat kategori task (many-to-many) | `GET`/`PUT /api/tasks/{id}/categories` | `TaskService.AssignTaskCategoriesAsync`, `Models/TaskCategory.cs` | Selesai | junction table `task_categories` |
| 16 | CRUD user (admin) | `GET/POST/PUT/DELETE /api/users` | `UsersController`, `UserService` | Selesai | policy `AdminOnly` |
| 17 | Users tanpa task aktif | `GET /api/users/without-tasks` | `UserService.GetUsersWithoutActiveTasksAsync` | Selesai | LINQ subquery `Any` |
| 18 | Profil user (1-to-1, upsert) | `GET`/`PUT /api/users/{id}/profile` | `UserService.UpsertUserProfileAsync`, `Models/UserProfile.cs` | Selesai | relasi one-to-one |
| 19 | CRUD kategori (admin) | `GET/POST/PUT/DELETE /api/categories` | `CategoriesController`, `CategoryService` | Selesai | unique index `Name` |
| 20 | Role-based access | — | `Program.cs` policy `AdminOnly`, `[Authorize]` | Selesai | claim `ClaimTypes.Role` |
| 21 | Global exception handler | — | `Errors/ApiExceptionHandler.cs`, `Errors/DuplicateResourceException.cs` | Selesai (bug mapping string sudah diperbaiki) | `IExceptionHandler` |
| 22 | Health check DB | `GET /health` | `Health/DatabaseHealthCheck.cs` | Selesai | `CanConnectAsync` |
| 23 | OpenAPI document + Bearer scheme | `GET /openapi/v1.json` | `Program.cs` document transformer | Selesai | hanya Development/Testing |
| 24 | Demo seeding | startup (Development) | `Data/Seeders/*` | Selesai | `Database:RefreshOnStartup` |
| 25 | CORS per-environment | — | `Program.cs`, `appsettings*.json` | Selesai | `Cors:AllowedOrigins` |

### Incomplete / Suspicious Features

- **Tidak ditemukan** `TODO`, `FIXME`, `HACK`, `TEMP`, atau `XXX` di seluruh source code
  (hasil pencarian di semua `*.cs`). Ini bagus.

Namun ada beberapa hal yang terlihat "setengah jadi" atau tidak terpakai:

1. **`OwnerNameSnapshot` dan `OwnerEmailSnapshot` ditulis tetapi tidak pernah dibaca.**
   - Ditulis di `Services/TaskService.cs` (baris 210–211) dan `Data/Seeders/TaskSeeder.cs`.
   - Tapi `TaskWithOwnerResponse` mengambil nama/email **live** dari `task.User.Name` / `task.User.Email`
     (`Services/TaskService.cs` baris 104–107), bukan dari snapshot.
   - Efeknya: kolom snapshot ada di database tapi tidak dipakai untuk apa pun saat ini.
     Evidence: pencarian `OwnerNameSnapshot` hanya menemukan penulisan, tidak ada pembacaan di response.

2. **`RefreshToken.ReplacedByTokenHash` ditulis tetapi tidak pernah dibaca.**
   - Ditulis di `Services/AuthService.cs` baris 142, tetapi tidak ada logika "deteksi pemakaian ulang token"
     yang memakainya. Fitur reuse-detection belum ada.

3. **`AssignTaskCategoriesRequest` tidak punya validasi.** (`DTOs/AssignTaskCategoriesRequest.cs`)
   `List<int> CategoryIds` tidak diberi `[Required]`, sehingga body `{ "categoryIds": null }`
   berpotensi menyebabkan error runtime (lihat Code Audit). — ✅ **Sudah diperbaiki (Step 2).**

4. **Query parameter `page` dan `limit` di `GET /api/tasks` tidak punya nilai default.**
   (`Controllers/TasksController.cs` baris 22–24). Jika klien tidak mengirim `page`/`limit`,
   nilainya menjadi `0` dan endpoint mengembalikan 400 "Page must be greater than 0".
   — ✅ **Sudah diperbaiki (Step 2): kini default 1 dan 10, plus batas atas 100.**

5. **`AuthService` tidak memiliki interface**, padahal `TaskService`, `UserService`, `CategoryService`
   punya interface (`ITaskService`, dll). Ini inkonsistensi pola, bukan bug.

6. **`appsettings.json` sengaja dikosongkan** (`ConnectionStrings:DefaultConnection` = `""`, `Jwt:Key` = `""`).
   Ini bukan bug — secret memang harus lewat User Secrets / environment variables.
   Tapi artinya **tanpa User Secrets, app akan gagal start** karena `Program.cs` baris 64–67
   melempar `InvalidOperationException` jika `Jwt:Key` kosong.

---

## Code Audit

### 🔴 Must Fix

#### 1. Exception `InvalidOperationException` dari kategori dipetakan ke HTTP 500, bukan 400 — ✅ SUDAH DIPERBAIKI

> **Status: Fixed.** Lihat catatan di akhir item ini untuk ringkasan perbaikannya.

**Issue:**
`ApiExceptionHandler` memetakan `InvalidOperationException` ke 400 **hanya jika pesannya persis**
`"Email is already registered"`. Sementara `CategoryService` melempar
`InvalidOperationException("Category name is already registered")`. Karena pesannya berbeda,
exception tersebut **jatuh ke default `_` → HTTP 500 Internal Server Error**.
Artinya: saat admin membuat/mengubah kategori dengan nama duplikat, API mengembalikan 500
(bukan 400) dan pesan generik "An unexpected error occurred".

**Location:**
`TaskManagement.Api/Errors/ApiExceptionHandler.cs` baris 24–26 (kondisi `when exception.Message == ...`),
`TaskManagement.Api/Services/CategoryService.cs` baris 47 dan 77.

**Why it matters:**
- Ini bug nyata: status code salah (500 = "server error", padahal ini kesalahan input klien → harusnya 400).
- 500 biasanya dianggap "server bermasalah" dan bisa memicu alert/monitoring palsu.
- Pola pemetaan berbasis **string pesan** sangat rapuh: setiap pesan exception baru harus "didaftarkan"
  manual, mudah lupa.
- Bukti bahwa bug ini lolos: test `TaskManagement.Api.Tests/Controllers/CategoriesControllerTests.cs`
  dan `Services/CategoryServiceTests.cs` menguji di level service/controller, bukan lewat
  `ApiExceptionHandler`, sehingga pemetaan 500 ini tidak terdeteksi.

**Suggested direction:**
Hindari mencocokkan string pesan. Buat tipe exception sendiri (misalnya `DuplicateEmailException`,
`DuplicateCategoryNameException`) atau satu exception khusus "validation/conflict", lalu petakan
tipe exception tersebut ke 400/409 di `ApiExceptionHandler`. Cara paling sederhana untuk beginner:
tambahkan cabang berdasarkan **tipe** exception, bukan isi `Message`.

**Resolution (sudah dikerjakan):**
- Ditambahkan tipe exception baru `TaskManagement.Api/Errors/DuplicateResourceException.cs`.
- `CategoryService` sekarang melempar `DuplicateResourceException` untuk nama kategori duplikat
  (bukan lagi `InvalidOperationException`).
- `UserService` dan `AuthService` juga melempar `DuplicateResourceException` untuk email duplikat
  (agar seluruh kasus "duplikat" memakai satu tipe yang sama dan konsisten).
- `ApiExceptionHandler` memetakan `DuplicateResourceException` → **HTTP 400**, dan kondisi
  `when exception.Message == "Email is already registered"` **dihapus** (tidak ada lagi pemetaan
  berbasis string pesan).
- Ditambahkan regression test: `ApiExceptionHandlerTests` (kategori duplikat → 400) dan
  `ApiIntegrationTests` (`POST`/`PUT` kategori duplikat → 400 lewat HTTP).

---

#### 2. `CategoryIds` bisa `null` → `NullReferenceException` → HTTP 500 — ✅ SUDAH DIPERBAIKI

> **Status: Fixed.** Lihat catatan di akhir item ini untuk ringkasan perbaikannya.

**Issue:**
`AssignTaskCategoriesRequest.CategoryIds` bertipe `List<int>` (non-nullable) tanpa `[Required]`.
`TaskService.AssignTaskCategoriesAsync` langsung memanggil `request.CategoryIds.Distinct()`.
Jika klien mengirim JSON `{ "categoryIds": null }`, properti menjadi `null` dan
`Distinct()` melempar `NullReferenceException`.

**Location:**
`TaskManagement.Api/DTOs/AssignTaskCategoriesRequest.cs`,
`TaskManagement.Api/Services/TaskService.cs` baris 325 (`request.CategoryIds.Distinct()`).

**Why it matters:**
- Menghasilkan 500 untuk input yang salah — seharusnya 400.
- `NullReferenceException` adalah jenis error yang paling sering bikin bingung saat debugging.

**Suggested direction:**
Tambahkan validasi (misalnya `[Required]` pada `CategoryIds`, atau cek `if (request.CategoryIds is null)`
lalu lempar `ArgumentException`). Untuk list kosong, tentukan perilakunya dengan sengaja
(apakah berarti "hapus semua kategori"?).

**Resolution (sudah dikerjakan):**
- `AssignTaskCategoriesRequest.CategoryIds` kini diberi `[Required]`, sehingga `categoryIds: null`
  ditolak di level model binding → **HTTP 400** dengan pesan "CategoryIds is required".
- Ditambah pengaman di `TaskService.AssignTaskCategoriesAsync` (`if (request.CategoryIds is null)
  throw new ArgumentException("CategoryIds is required")`) sebagai lapisan kedua.
- Ditambah test: unit test service (`AssignTaskCategoriesAsync_NullCategoryIds_ThrowsArgumentException`)
  dan integration test HTTP (`AssignTaskCategories_NullCategoryIds_ReturnsBadRequest`).

---

#### 3. Hapus user menghapus task miliknya secara permanen (hard delete), tidak konsisten dengan soft delete — ✅ SUDAH DIPERBAIKI

> **Status: Fixed.** Lihat catatan di akhir item ini untuk ringkasan perbaikannya.

**Issue:**
`UserService.DeleteUserAsync` memakai `_dbContext.Users.Remove(user)` (hard delete).
Di `AppDbContext` relasi `User → Tasks` memakai default cascade delete (terlihat di
`Migrations/*` `onDelete: ReferentialAction.Cascade`). Jadi menghapus seorang user akan
**menghapus permanen** semua task miliknya dari database, termasuk task yang sebelumnya
sudah "soft deleted". Ini berbeda dari model soft delete yang dipakai untuk task
(`TaskItem.IsDeleted` + `WhereActive()`).

**Location:**
`TaskManagement.Api/Services/UserService.cs` baris 180–193,
`TaskManagement.Api/Data/AppDbContext.cs` baris 46–49,
migration `20260912143356_AddUserTaskRelationship.cs` (Cascade).

**Why it matters:**
- Risiko kehilangan data: satu request `DELETE /api/users/{id}` bisa menghapus banyak task sekaligus
  tanpa konfirmasi dan tanpa jejak (tidak ada `DeletedAt`).
- Tidak konsisten: task pakai soft delete, user pakai hard delete. Seorang developer perlu tahu
  konsekuensi cascade ini.
- Tidak ada proteksi khusus: endpoint bisa dipakai untuk menghapus akun demo admin (`admin@mail.com`).

**Suggested direction:**
Tentukan kebijakan dengan sadar. Pilihan sederhana: ikuti model soft delete (tambahkan
`IsDeleted`/`DeletedAt` pada `User` dan filter dengan `WhereActive`), atau minimal
ubah `DeleteBehavior` relasi agar tidak otomatis cascade, lalu tangani task milik user secara eksplisit.
Ini keputusan desain, bukan sekadar bug — tapi harus disadari.

**Resolution (sudah dikerjakan — opsi "larang hapus bila masih punya task"):**
- `UserService.DeleteUserAsync` kini menolak penghapusan bila user masih punya task **apa pun**
  (aktif maupun soft-deleted) dengan `ArgumentException("User cannot be deleted because the user
  has tasks")` → **HTTP 400** lewat `ApiExceptionHandler`. Pengecekan memakai
  `IgnoreQueryFilters()` agar baris soft-deleted ikut dihitung.
- Relasi `User → Tasks` di `AppDbContext` diubah dari Cascade menjadi
  `DeleteBehavior.Restrict`, plus migration `RestrictUserTaskDelete`, sebagai backstop di level
  database bila ada jalur lain yang mencoba menghapus user bertask.
- Soft delete penuh pada `User` sengaja **tidak** dipilih karena efek ikutannya besar untuk
  level ini (konflik unique index `Email`, refresh token user terhapus yang masih valid,
  `FindAsync` yang mengabaikan query filter). Kebijakan "tolak bila masih punya task" menutup
  risiko kehilangan data dengan perubahan minimal.
- Diuji di `UserServiceTests` (task aktif → tolak; hanya soft-deleted → tolak; user & task tetap ada)
  dan `ApiIntegrationTests` (hapus user bertask → 400; hapus user tanpa task → 204).

---

### 🟡 Should Improve

#### 1. Pemetaan exception berbasis perbandingan string pesan — ✅ SUDAH DIPERBAIKI

**Issue:** `ApiExceptionHandler` mencocokkan `exception.Message == "Email is already registered"`.
**Location:** `TaskManagement.Api/Errors/ApiExceptionHandler.cs` baris 24.
**Why it matters:** Rapuh dan tidak scalable (penyebab langsung bug #1 di atas).
**Suggested direction:** Ganti ke pencocokan berdasarkan **tipe** exception, atau gunakan exception khusus
untuk kasus "konflik data".
**Resolution:** Sudah diganti ke pencocokan berdasarkan tipe `DuplicateResourceException`
(kondisi string dihapus). Lihat Must Fix #1.

#### 2. Soft delete bergantung pada pemanggilan manual `WhereActive()` di setiap query — ✅ SUDAH DIPERBAIKI

**Issue:** Tidak ada **global query filter**. Setiap query harus ingat menambahkan `.WhereActive()`.
Bukti risiko: `UserService.GetUsersWithoutActiveTasksAsync` menulis ulang kondisi
`!task.IsDeleted` secara manual (`Services/UserService.cs` baris 37–39), artinya aturan "task aktif"
tersebar di beberapa tempat.
**Location:** `Extensions/TaskQueryExtensions.cs`, `Services/TaskService.cs`, `Services/UserService.cs`.
**Why it matters:** Kalau ada satu query baru yang lupa `WhereActive()`, task yang sudah dihapus bisa
muncul lagi — bug yang sulit terdeteksi.
**Suggested direction:** Pelajari **EF Core global query filters** (`HasQueryFilter`) di `AppDbContext`
untuk task yang soft-deleted, sehingga filter otomatis berlaku.
**Resolution:** Sudah ditambahkan global query filter di `AppDbContext`
(`modelBuilder.Entity<TaskItem>().HasQueryFilter(task => !task.IsDeleted)`), plus filter pencocokan
pada `TaskCategory` (`HasQueryFilter(tc => !tc.TaskItem.IsDeleted)`) sesuai rekomendasi EF Core untuk
relasi required. Semua pemanggilan `.WhereActive()` manual dihapus, dan kondisi `!task.IsDeleted`
manual di `UserService` disederhanakan. Diuji di `Tests/Data/SoftDeleteQueryFilterTests.cs`.

#### 3. Duplikasi logika hashing password

**Issue:** Pembuatan `PasswordHasher<User>` dan pemanggilan `HashPassword` diulang di
`AuthService`, `UserService`, dan `UserSeeder`.
**Location:** `Services/AuthService.cs` baris 58–59, `Services/UserService.cs` baris 137–138 & 170–171,
`Data/Seeders/UserSeeder.cs`.
**Why it matters:** Kalau nanti mau ganti algoritma hashing, harus diubah di banyak tempat.
**Suggested direction:** Daftarkan `IPasswordHasher<User>` lewat DI dan inject ke service (bukan `new` manual).

#### 4. `AuthService` tidak punya interface, service lain punya

**Issue:** `ITaskService`, `IUserService`, `ICategoryService` ada, tapi `AuthService` tidak punya `IAuthService`.
**Location:** `Services/AuthService.cs` vs `Services/ITaskService.cs` dll.
**Why it matters:** Inkonsistensi pola; controller `AuthController` meng-`new`-kan lewat DI class konkret.
**Suggested direction:** Pilih satu konvensi. Untuk konsistensi, tambahkan `IAuthService`.

#### 5. Validasi id `id <= 0` mengembalikan 404, bukan 400

**Issue:** Di banyak endpoint, `if (id <= 0) return NotFound(...)`. Padahal `id = 0` atau negatif adalah
request yang tidak valid (400), bukan "resource tidak ditemukan" (404).
**Location:** `Controllers/TasksController.cs`, `Controllers/UsersController.cs`, `Controllers/CategoriesController.cs`.
**Why it matters:** Semantik HTTP/REST kurang tepat; klien tidak bisa membedakan "input salah" vs "tidak ada".
**Suggested direction:** Pelajari perbedaan 400 vs 404, lalu kembalikan `BadRequest` untuk id tidak valid.

#### 6. Tidak ada batas maksimum `limit` pada pagination — ✅ SUDAH DIPERBAIKI

**Issue:** `GET /api/tasks?limit=1000000` akan diterima dan mengambil data sebanyak itu.
**Location:** `Controllers/TasksController.cs` baris 36–39, `Services/TaskService.cs` baris 68–69.
**Why it matters:** Bisa memberatkan database dan server (mirip DoS ringan).
**Suggested direction:** Tambahkan batas atas, misalnya `limit` maksimal 100.
**Resolution:** Sudah ditambahkan konstanta `MaxPageSize = 100` di `TasksController`;
`limit > 100` → **HTTP 400** ("Limit must not exceed 100"). Diuji di unit test dan integration test.

#### 7. `page`/`limit` tanpa default value — ✅ SUDAH DIPERBAIKI

**Issue:** Endpoint task list tidak punya default, sehingga request tanpa parameter mengembalikan 400.
**Location:** `Controllers/TasksController.cs` baris 22–24.
**Why it matters:** UX API kurang ramah; klien harus selalu mengirim parameter.
**Suggested direction:** Beri default, misalnya `int page = 1, int limit = 10`.
**Resolution:** `GetTasks` kini memakai `int page = 1, int limit = 10`; request tanpa parameter
mengembalikan **HTTP 200** dengan `page = 1`, `limit = 10`. Diuji di unit test dan integration test.

#### 8. Beberapa DTO belum punya atribut validasi — 🟡 SEBAGIAN DIPERBAIKI

**Issue:** `UpsertUserProfileRequest` (Bio/Location) dan `UpdateTaskCompletionRequest` tidak divalidasi;
`AssignTaskCategoriesRequest` tidak punya `[Required]`.
**Location:** `DTOs/UpsertUserProfileRequest.cs`, `DTOs/UpdateTaskCompletionRequest.cs`, `DTOs/AssignTaskCategoriesRequest.cs`.
**Why it matters:** Input tak terduga bisa lolos ke service.
**Suggested direction:** Tambahkan DataAnnotations yang sesuai (`[MaxLength]`, `[Required]`).
**Resolution:** `AssignTaskCategoriesRequest` kini `[Required]`; `UpsertUserProfileRequest` kini punya
`[MaxLength]` (Bio 500, Location 200). `UpdateTaskCompletionRequest` masih berupa `bool` (tidak butuh
validasi tambahan), jadi tidak diubah.

#### 9. `int.Parse` pada claim bisa melempar `FormatException` → 500

**Issue:** `GetUserIdFromClaims` memakai `int.Parse(userIdClaim.Value)` tanpa `TryParse`.
**Location:** `Controllers/TasksController.cs` baris 194–203.
**Why it matters:** Jika claim `NameIdentifier` tidak valid, muncul 500 (harusnya 401).
**Suggested direction:** Gunakan `int.TryParse` dan lempar `UnauthorizedAccessException` jika gagal.

#### 10. `CancellationToken` tidak konsisten

**Issue:** `TaskService` dan `CategoryService` menerima `CancellationToken`, tapi `IUserService`/`UserService`
tidak menerimanya sama sekali.
**Location:** `Services/IUserService.cs`, `Services/UserService.cs`.
**Why it matters:** Konsistensi dan efisiensi; pembatalan request tidak diteruskan ke query user.
**Suggested direction:** Tambahkan `CancellationToken` di method `UserService` dan teruskan ke `...Async`.

#### 11. Duplikasi validasi manual yang berulang

**Issue:** Validasi `string.IsNullOrWhiteSpace` untuk Name/Email/Password ditulis ulang di
`AuthService` dan `UserService`.
**Location:** `Services/AuthService.cs` baris 29–42, `Services/UserService.cs` baris 195–206.
**Why it matters:** Duplikasi logika validasi.
**Suggested direction:** Pertimbangkan memindahkan validasi ke atribut DataAnnotations di DTO
atau satu helper bersama.

#### 12. Format response kustom (`ApiResponse<T>`) alih-alih ProblemDetails

**Issue:** Project memakai envelope `ApiResponse<T>` sendiri, bukan format standar ASP.NET Core
(ProblemDetails / RFC 7807).
**Location:** `DTOs/ApiResponse.cs`, `Program.cs` baris 50–57 & 136–147.
**Why it matters:** Bukan bug, tapi format non-standar bisa menyulitkan integrasi klien.
**Suggested direction:** Ini pilihan desain. Cukup pahami trade-off-nya; tidak wajib diubah.

#### 13. Tidak ada `.editorconfig`

**Issue:** Tidak ditemukan `.editorconfig` di repository.
**Location:** root repository.
**Why it matters:** Konsistensi style antar file bergantung pada IDE masing-masing.
**Suggested direction:** Opsional: tambahkan `.editorconfig` standar .NET.

---

### ⚪ Not Necessary Yet

Hal-hal berikut secara teori "bagus" tetapi **belum perlu** untuk project latihan ini.
Jangan dikerjakan dulu; fokus ke fundamental.

- **CQRS / MediatR** — service layer sekarang sudah cukup jelas dan mudah dibaca.
- **Repository Pattern + Unit of Work terpisah** — `AppDbContext` sudah berperan sebagai Unit of Work.
  Menambah repository hanya menambah lapisan tanpa manfaat nyata di skala ini.
- **Clean Architecture penuh (multi-project Domain/Application/Infrastructure)** — overkill untuk 1 API.
- **Microservices / message broker / event-driven** — sama sekali tidak relevan.
- **AutoMapper** — pemetaan manual entity→DTO saat ini sudah ringkas dan eksplisit (bagus untuk belajar).
- **FluentValidation** — DataAnnotations sudah dipakai; mengganti library validasi belum perlu sekarang.
- **Deteksi pemakaian ulang refresh token (token family revocation)** — fitur keamanan lanjutan;
  `ReplacedByTokenHash` sudah disimpan sebagai persiapan, tapi implementasinya bisa nanti.
- **Distributed cache / Redis** — belum ada kebutuhan.
- **Testcontainers** — integration test sekarang sudah jalan dengan PostgreSQL nyata via CI;
  Testcontainers bisa dipelajari nanti.
- **Rate limiting / API versioning** — belum diperlukan untuk latihan.

---

## .NET Skills Observed

| Skill | Status | Evidence | Notes |
| --- | --- | --- | --- |
| C# Fundamentals | ✅ Practiced | `record`, pattern matching (`switch` expression di `ApiExceptionHandler`), nullable reference types, generics (`ApiResponse<T>`, `PaginatedResponse<T>`), collection expression `[]` | Level C# yang dipakai sudah modern (.NET 10) |
| OOP | ✅ Practiced | Class + interface (`ITaskService`/`TaskService`), encapsulation lewat constructor injection, entity class | — |
| Dependency Injection | ✅ Practiced | Registrasi `AddScoped<ITaskService, TaskService>()` dll di `Program.cs`; constructor injection di semua controller/service | Lifetime Scoped dipakai konsisten |
| ASP.NET Core | ✅ Practiced | `Program.cs` (minimal hosting), Controllers, middleware pipeline, `AddOpenApi`, health checks, CORS | Pemahaman pipeline cukup baik |
| REST API | ✅ Practiced | Resource `tasks`/`users`/`categories`, method GET/POST/PUT/PATCH/DELETE, status 200/201/204/400/401/403/404 | Ada beberapa semantik status code yang perlu diperbaiki (lihat Code Audit) |
| Entity Framework Core | ✅ Practiced | Migrations (25 file), relasi one-to-many & one-to-one & many-to-many, unique index, `AsNoTracking`, projection `.Select`, `Include`, soft delete | Cukup dalam untuk level latihan |
| LINQ | ✅ Practiced | `Where`, `GroupBy`, `OrderBy`/`ThenBy`, `Select`, `Count`, `Any`, `Contains` di `TaskService`/`UserService` | Termasuk query yang di-translate ke SQL |
| DTO | ✅ Practiced | Folder `DTOs/` terpisah dari `Models/`; request & response terpisah | Tidak membocorkan entity ke API |
| Validation | ✅ Practiced | DataAnnotations (`[Required]`, `[EmailAddress]`, `[MaxLength]`) + validasi manual di service + validasi query parameter (default & batas pagination) | Ditingkatkan di Step 2 |
| Async/Await | ✅ Practiced | `async Task<...>` di seluruh service & controller, `await SaveChangesAsync`, `CancellationToken` di `TaskService`/`CategoryService` | Belum konsisten di `UserService` |
| Exception Handling | ✅ Practiced | `IExceptionHandler` global (`ApiExceptionHandler`), custom exception `DuplicateResourceException`, mapping berdasarkan **tipe** exception | Mapping berbasis string pesan sudah dihapus (lihat Step 1) |
| Logging | ✅ Practiced | `ILogger<T>` di semua service, pesan terstruktur (`"Task {TaskId} created by user {UserId}"`), diuji tidak membocorkan email/password/token | Test `TestLogger` memverifikasi log tidak berisi data sensitif |
| Configuration | ✅ Practiced | `appsettings*.json` per-environment, User Secrets, environment variables, `builder.Configuration[...]` | Fail-fast jika `Jwt:Key` kosong |
| Authentication | ✅ Practiced | JWT Bearer (`AddJwtBearer`), generate token, refresh token rotation + hashing, logout revoke | Refresh token reuse-detection belum ada |
| Authorization | ✅ Practiced | `[Authorize]`, policy `AdminOnly` (`RequireRole("Admin")`), scoping task per owner | Diuji di integration test |
| Unit Testing | ✅ Practiced | xUnit + Moq + EF InMemory di `Tests/Controllers`, `Tests/Services`, `Tests/Data`, `Tests/Errors` | Cakupan test luas dan rapi |
| Integration Testing | ✅ Practiced | `WebApplicationFactory<Program>` + PostgreSQL nyata (`PostgresWebApplicationFactory`), dijalankan di CI | Termasuk uji CORS, health, OpenAPI, auth, role, CRUD HTTP |

---

## Learning Gaps

Berdasarkan project, konsep fundamental yang **paling relevan** untuk dipelajari berikutnya:

1. **Penanganan exception yang benar & konsisten (exception handling).**
   ~~Bug kategori→500 menunjukkan bahwa pemetaan exception ke HTTP status belum kokoh.~~
   Sudah dipraktikkan lewat Step 1 (custom exception + pemetaan berbasis tipe). Langkah lanjutan:
   menerapkan pola ini ke seluruh exception bisnis agar tidak ada lagi mapping berbasis string.

2. **Semantik HTTP / REST (status code yang tepat).**
   Perbedaan 400 vs 404 vs 409, kapan memakai masing-masing. Banyak endpoint masih memakai
   `NotFound` untuk id tidak valid.

3. **EF Core: global query filters untuk soft delete.**
   Saat ini filter `IsDeleted` diterapkan manual di setiap query — berisiko lupa.
   Ini cara EF Core untuk menerapkan aturan filter secara otomatis.

4. **Validasi menyeluruh (validation).**
   Sudah dipraktikkan lewat Step 2 (DataAnnotations lanjutan + validasi query parameter + batas
   pagination). Langkah lanjutan: konsistenkan validasi untuk semua endpoint baru ke depan.

5. **Transaksi & konsistensi data (EF Core transactions + `DeleteBehavior`).**
   Sudah dipraktikkan lewat Step 4 (FK Cascade → Restrict + guard di service + migration).
   `DatabaseSeeder.RefreshAsync` tetap menjadi contoh transaction yang baik untuk dipelajari.

6. **Async & CancellationToken secara konsisten.**
   `UserService` belum menerima `CancellationToken`, berbeda dengan service lain.

7. **Logging lebih lanjut (log scopes / correlation).**
   Logging sudah bagus; langkah berikutnya bisa mempelajari log scope dan level.

Catatan: konsep seperti CQRS, MediatR, microservices, dan Clean Architecture **tidak** masuk daftar gap
karena belum relevan untuk kondisi project ini.

---

## Recommended Roadmap

Urutan di bawah disusun berdasarkan dependency: Step 1 sebaiknya selesai sebelum Step 2, dst.
Semua berasal dari kondisi project nyata (bukan roadmap generik).

### Step 1 — Perbaiki pemetaan exception & HTTP status code — ✅ SELESAI

> **Status: DONE.** Step ini sudah dikerjakan (lihat Must Fix #1). Semua kotak Definition of Done
> di bawah sudah terpenuhi dan `dotnet test` hijau (126 test). Step berikutnya adalah Step 2.

**Goal**
Membuat `ApiExceptionHandler` memetakan exception ke status code yang benar tanpa bergantung
pada perbandingan string pesan, sehingga kasus kategori duplikat mengembalikan 400 (bukan 500).

**Why Now**
Ini bug nyata yang sudah teridentifikasi (kategori duplikat → 500). Memperbaikinya melatih
exception handling dan semantik HTTP sekaligus, dan berdampak ke seluruh API.

**Concepts Learned**
- Global exception handling (`IExceptionHandler`)
- Exception khusus (custom exception class) vs `InvalidOperationException` generik
- Mapping exception → HTTP status code (400 vs 404 vs 409 vs 500)
- Kenapa `catch`/mapping berbasis string itu rapuh

**Likely Files**
`TaskManagement.Api/Errors/ApiExceptionHandler.cs`,
`TaskManagement.Api/Errors/DuplicateResourceException.cs` (baru),
`TaskManagement.Api/Services/CategoryService.cs`,
`TaskManagement.Api/Services/AuthService.cs`,
`TaskManagement.Api/Services/UserService.cs`,
`TaskManagement.Api.Tests/Errors/ApiExceptionHandlerTests.cs`,
`TaskManagement.Api.Tests/Integration/ApiIntegrationTests.cs`

**Definition of Done**
- [x] Tidak ada lagi `when exception.Message == ...` di `ApiExceptionHandler`.
- [x] Membuat kategori dengan nama duplikat → HTTP **400** dengan pesan yang benar.
- [x] Membuat/update user dengan email duplikat tetap → **400**.
- [x] Test baru menutup kasus kategori duplikat (handler unit test + integration test).
- [x] `dotnet test` tetap hijau (126 test lulus).

**Difficulty:** Medium

---

### Step 2 — Tambahkan validasi yang hilang pada DTO & query parameter — ✅ SELESAI

> **Status: DONE.** Step ini sudah dikerjakan (lihat Must Fix #2 & Should Improve #6–8).
> Semua Definition of Done di bawah sudah terpenuhi dan `dotnet test` hijau (133 test).

**Goal**
Memastikan semua input tervalidasi: `AssignTaskCategoriesRequest`, `UpsertUserProfileRequest`,
dan batas atas `limit` pada endpoint task list.

**Why Now**
Setelah error handling rapi (Step 1), langkah berikutnya adalah mencegah input tidak valid
sampai ke service. Ini juga menutup potensi `NullReferenceException` dari `CategoryIds = null`.

**Concepts Learned**
- DataAnnotations lanjutan (`[Required]`, `[Range]`, `[MaxLength]`)
- Validasi query parameter (model binding)
- Perbedaan validasi di DTO vs di service

**Likely Files**
`TaskManagement.Api/DTOs/AssignTaskCategoriesRequest.cs`,
`TaskManagement.Api/DTOs/UpsertUserProfileRequest.cs`,
`TaskManagement.Api/Controllers/TasksController.cs`,
`TaskManagement.Api/Services/TaskService.cs`

**Definition of Done**
- [x] `{ "categoryIds": null }` → **400**, bukan 500.
- [x] `limit` melebihi batas (> 100) ditolak → **400**.
- [x] `page`/`limit` punya default (1 dan 10) bila tidak dikirim.
- [x] Test untuk masing-masing kasus (unit + integration).

**Difficulty:** Easy–Medium

---

### Step 3 — Konsistenkan soft delete dengan global query filter (EF Core) — ✅ SELESAI

> **Status: DONE.** Step ini sudah dikerjakan. Semua Definition of Done di bawah sudah terpenuhi
> dan `dotnet test` hijau (136 test). Tidak ada perubahan skema database (global query filter
> bukan bagian dari schema), sehingga **tidak perlu migration**.

**Goal**
Membuat filter task yang sudah dihapus berlaku otomatis, tanpa harus memanggil `WhereActive()`
di setiap query.

**Why Now**
Model soft delete sudah ada, tetapi penerapannya masih manual dan tersebar.
Ini kesempatan belajar fitur EF Core yang penting dan mengurangi risiko bug di masa depan.

**Concepts Learned**
- EF Core **global query filters** (`HasQueryFilter`)
- Cara mengabaikan filter saat memang perlu (`IgnoreQueryFilters`)
- Trade-off filter otomatis
- Peringatan EF Core saat entity ber-filter berada di relasi **required** (perlu filter pencocokan)

**Likely Files**
`TaskManagement.Api/Data/AppDbContext.cs`,
`TaskManagement.Api/Extensions/TaskQueryExtensions.cs`,
`TaskManagement.Api/Services/TaskService.cs`,
`TaskManagement.Api/Services/UserService.cs`,
`TaskManagement.Api.Tests/Data/SoftDeleteQueryFilterTests.cs` (baru)

**Definition of Done**
- [x] Query task yang sudah dihapus tidak perlu lagi menambahkan `.WhereActive()` manual.
- [x] Semua test lama tetap hijau (termasuk yang menguji task soft-deleted).
- [x] Perilaku endpoint tidak berubah dari sudut pandang API.

**Difficulty:** Medium

---

### Step 4 — Putuskan kebijakan hapus user & relasinya (cascade vs soft delete) — ✅ SELESAI

> **Status: DONE.** Step ini sudah dikerjakan dengan opsi **"larang hapus bila masih punya task"**.
> Semua Definition of Done di bawah sudah terpenuhi dan `dotnet test` hijau (140 test).
> Perubahan skema (FK Cascade → Restrict) dituangkan dalam migration `RestrictUserTaskDelete`.

**Goal**
Menghilangkan risiko kehilangan data permanen saat user dihapus; menyelaraskan dengan model soft delete.

**Why Now**
Bergantung pada Step 3: setelah soft delete konsisten, kebijakan user bisa dibuat selaras.
Ini juga melatih pemahaman relasi & `DeleteBehavior` di EF Core.

**Concepts Learned**
- Relasi & `DeleteBehavior` (Cascade/Restrict/SetNull)
- Soft delete pada entity induk (dipelajari, lalu sengaja tidak dipilih — lihat alasan di bawah)
- Konsekuensi operasi delete berantai
- Membuat EF Core migration untuk perubahan perilaku FK

**Likely Files**
`TaskManagement.Api/Models/User.cs` (tidak diubah — sengaja),
`TaskManagement.Api/Data/AppDbContext.cs`,
`TaskManagement.Api/Services/UserService.cs`,
`TaskManagement.Api/Migrations/20260918161716_RestrictUserTaskDelete.cs` (baru),
`TaskManagement.Api.Tests/Services/UserServiceTests.cs`,
`TaskManagement.Api.Tests/Integration/ApiIntegrationTests.cs`

**Definition of Done**
- [x] Kebijakan tertulis jelas: **tolak hapus user yang masih punya task** (400), FK jadi Restrict.
- [x] Menghapus user tidak lagi menghapus task secara tak terduga.
- [x] Test menutup skenario menghapus user yang punya task (unit + integration).

**Alasan tidak memilih soft delete user:** efek ikutannya besar untuk level ini — konflik unique
index `Email` (email user terhapus tidak bisa dipakai lagi tanpa logika khusus), refresh token milik
user terhapus yang tetap valid, serta `FindAsync` yang mengabaikan global query filter sehingga
`GetUserByIdAsync`/`UpdateUserAsync`/`DeleteUserAsync` harus ditulis ulang. Kebijakan penolakan
menutup risiko kehilangan data dengan perubahan minimal.

**Difficulty:** Medium

---

### Step 5 — Konsistenkan `CancellationToken` & rapikan duplikasi password hashing

**Goal**
Menambah `CancellationToken` di `UserService` dan menghapus duplikasi `PasswordHasher` dengan
memanfaatkan DI.

**Why Now**
Ini perapian setelah alur utama stabil. Keduanya peningkatan kualitas yang terukur dan tidak berisiko besar.

**Concepts Learned**
- Meneruskan `CancellationToken` ke EF Core async API
- Registrasi `IPasswordHasher<User>` di DI container
- Menghilangkan duplikasi (DRY)

**Likely Files**
`TaskManagement.Api/Services/UserService.cs`,
`TaskManagement.Api/Services/IUserService.cs`,
`TaskManagement.Api/Program.cs`,
`TaskManagement.Api/Services/AuthService.cs`

**Definition of Done**
- [ ] Semua method `UserService` menerima & meneruskan `CancellationToken`.
- [ ] `PasswordHasher` di-inject lewat DI, bukan `new` berulang.
- [ ] Semua test tetap hijau.

**Difficulty:** Easy

---

### Step 6 — Tambahkan test untuk bug yang baru diperbaiki

**Goal**
Memperkuat test suite agar bug yang diperbaiki di Step 1–2 tidak muncul lagi (regression test).

**Why Now**
Terakhir, supaya semua perbaikan "terkunci" oleh test. Ini melatih disiplin testing.

**Concepts Learned**
- Menulis regression test
- Menguji lewat integration test (WebApplicationFactory)
- Memahami coverage (`coverage.runsettings`)

**Likely Files**
`TaskManagement.Api.Tests/Errors/ApiExceptionHandlerTests.cs`,
`TaskManagement.Api.Tests/Integration/ApiIntegrationTests.cs`,
`TaskManagement.Api.Tests/Controllers/CategoriesControllerTests.cs`

**Definition of Done**
- [ ] Ada test untuk kategori duplikat → 400.
- [ ] Ada test untuk `categoryIds: null` → 400.
- [ ] `dotnet test` hijau, coverage kategori/error terpantau.

**Difficulty:** Easy–Medium

---

## 🎯 NEXT TASK

> Catatan: Step 1 (pemetaan exception), Step 2 (validasi), Step 3 (global query filter),
> dan Step 4 (kebijakan hapus user) **sudah selesai**. Berikut task berikutnya.

# Konsistenkan `CancellationToken` & rapikan duplikasi password hashing

## Objective

Dua perapian kecil yang saling terkait di layer service:

1. Menambah `CancellationToken` di semua method `IUserService`/`UserService` dan meneruskannya
   ke semua pemanggilan EF Core async — menyamakan dengan `TaskService`/`CategoryService`
   yang sudah menerimanya.
2. Menghapus duplikasi `new PasswordHasher<User>()` di `AuthService`, `UserService`, dan
   `UserSeeder` dengan mendaftarkan `IPasswordHasher<User>` di DI container dan meng-inject-nya.

## Why This Task

- Setelah alur utama stabil (Step 1–4), ini saat yang tepat untuk perapian berkualitas:
  terukur, tidak berisiko besar, dan melatih dua fundamental (async + DI) sekaligus.
- `CancellationToken` yang tidak diteruskan berarti pembatalan request klien tidak sampai
  ke query database user — inkonsistensi yang mudah diperbaiki sekarang.
- `new PasswordHasher<User>()` yang tersebar di 3+ tempat berarti penggantian algoritma
  hashing di masa depan harus diubah di banyak tempat (pelanggaran DRY).

## Files To Study First

1. `TaskManagement.Api/Services/ITaskService.cs` + `TaskService.cs` — contoh pola
   `CancellationToken` yang sudah benar (jadikan acuan).
2. `TaskManagement.Api/Services/IUserService.cs` + `UserService.cs` — yang akan diubah.
3. `TaskManagement.Api/Controllers/UsersController.cs` — pemanggil `IUserService`
   (perlu meneruskan token; controller punya `HttpContext.RequestAborted` bila mau eksplisit,
   atau biarkan default).
4. `TaskManagement.Api/Program.cs` — tempat mendaftarkan `IPasswordHasher<User>`.
5. `TaskManagement.Api/Services/AuthService.cs` + `Data/Seeders/UserSeeder.cs` — pemakai
   `new PasswordHasher<User>()` yang akan diganti inject.
6. Test yang memanggil `UserService` langsung (`UserServiceTests`, `UsersControllerTests`)
   — perlu dicek apakah perlu penyesuaian tanda tangan.

## Concepts To Understand

- **`CancellationToken` di ASP.NET Core**: bagaimana pembatalan request mengalir dari
  controller → service → EF Core, dan kenapa `...Async(..., cancellationToken)` penting.
- **Mendaftarkan service framework di DI**: `builder.Services.AddScoped<IPasswordHasher<User>,
  PasswordHasher<User>>()` dan constructor injection.
- Prinsip **DRY** (Don't Repeat Yourself) untuk logika infrastruktur seperti hashing.

## Implementation Direction

Jangan langsung menulis kode lengkap — lakukan bertahap:

1. **Pelajari** bagaimana `TaskService` menerima `CancellationToken cancellationToken = default`
   dan meneruskannya ke `CountAsync`/`ToListAsync`/`SaveChangesAsync`. Perhatikan bahwa parameter
   opsional (`= default`) membuat pemanggil lama tetap kompilasi.
2. **Tambahkan** `CancellationToken cancellationToken = default` ke semua method
   `IUserService`/`UserService`, teruskan ke setiap `...Async`, dan teruskan dari
   `UsersController` (cukup teruskan parameter `cancellationToken` yang disediakan framework
   di action, atau biarkan default).
3. **Daftarkan** `IPasswordHasher<User>` di `Program.cs`, lalu ganti semua
   `new PasswordHasher<User>()` dengan field yang di-inject via constructor di
   `AuthService`, `UserService`, dan `UserSeeder` (jangan lupa daftarkan juga bila seeder
   di-resolve via DI — cek `Program.cs` baris seeder).
4. **Handle** test yang terdampak: pemanggil dengan positional argument tetap kompilasi
   karena parameter baru opsional; yang memakai Moq `Setup` mungkin perlu
   `It.IsAny<CancellationToken>()`.
5. **Test**: jalankan `dotnet test` — semua harus hijau tanpa perubahan perilaku API.

## Expected Behavior

Setelah selesai:

- Perilaku API **tidak berubah sama sekali** dari sudut pandang klien (refactor murni).
- Pembatalan request diteruskan sampai ke query database untuk endpoint `/api/users`.
- Tidak ada lagi `new PasswordHasher<User>()` di codebase (cek dengan pencarian).

## How To Verify

1. **Pencarian kode:**
   - Cari `new PasswordHasher` di seluruh `*.cs` → tidak ada lagi hasil di `TaskManagement.Api/`.
   - Cari method `UserService` tanpa `CancellationToken` → tidak ada lagi.

2. **Automated test:**
   - `dotnet test TaskManagement.slnx` → semua hijau.
   - Perilaku login/register (hashing) tetap benar — dibuktikan test `AuthServiceTests`
     dan `UserServiceTests` yang sudah ada.

3. **Manual (opsional):**
   - Register + login via Bruno tetap berhasil (membuktikan hashing via DI bekerja).

---

## Final Summary

### Current State

Project ini jauh melampaui "beginner biasa": sebuah **ASP.NET Core Web API (.NET 10)** yang lengkap dengan
PostgreSQL + EF Core (26 migration), JWT authentication **plus refresh token rotation**, role-based
authorization, soft delete task, relasi many-to-many (task↔category), profil user one-to-one,
demo seeding, health check, OpenAPI, CORS per-environment, CI GitHub Actions, serta **test unit dan
integration yang luas**. Arsitekturnya rapi: **Controller → Service → DbContext → PostgreSQL**, tanpa
over-engineering. Tidak ditemukan TODO/FIXME. Empat area sudah dibereskan: bug kategori duplikat → 500
(Step 1), validasi yang hilang (Step 2), soft delete otomatis via global query filter (Step 3),
dan kebijakan hapus user yang aman (Step 4). Tersisa perapian kecil (konsistensi async, duplikasi
password hashing). Test suite saat ini **140 test hijau**.

### What I Have Practiced

Yang sudah jelas terpakai: C# modern (record, pattern matching, nullable), OOP & interface,
Dependency Injection (constructor injection + lifetime), ASP.NET Core (middleware pipeline, controllers,
health checks, OpenAPI), REST API, EF Core (migrations, relasi, index, projection, soft delete,
**global query filters**, **`DeleteBehavior`**), LINQ, DTO terpisah dari entity, async/await,
logging terstruktur, configuration per-environment, authentication (JWT + refresh token),
authorization (policy & role), unit testing (xUnit + Moq), dan integration testing
(WebApplicationFactory + PostgreSQL nyata).
Selain itu, baru dipraktikkan: **custom exception + pemetaan exception berbasis tipe** (Step 1),
**validasi DataAnnotations lanjutan & validasi query parameter** (Step 2),
**EF Core global query filter untuk soft delete** (Step 3), serta **kebijakan delete yang aman
dengan Restrict + guard di service + migration FK** (Step 4).

### Biggest Gaps

1. **Semantik HTTP/REST** — id tidak valid masih mengembalikan 404 (seharusnya 400).
2. **Konsistensi async** — `UserService` belum menerima `CancellationToken` seperti service lain.
3. **Duplikasi password hashing** — `PasswordHasher<User>` dibuat manual (`new`) di beberapa tempat.
4. **Keputusan desain lanjutan** — mis. proteksi hapus akun demo/admin (saat ini masih bisa dihapus
   bila tidak punya task); sengaja di luar cakupan Step 4.

### Immediate Priority

Merapikan **konsistensi `CancellationToken` di `UserService`** dan **menghapus duplikasi
`PasswordHasher`** lewat DI — dua perapian kecil yang melatih fundamental async + DI
setelah alur utama stabil.

### Next Task

**Konsistenkan `CancellationToken` & rapikan duplikasi password hashing**
(lihat bagian 🎯 NEXT TASK di atas).

### After That

Setelah NEXT TASK selesai, tutup dengan **Step 6** (regression test & coverage check),
lalu pertimbangkan perapian semantik HTTP (400 vs 404 untuk id tidak valid) sebagai latihan lanjutan.
