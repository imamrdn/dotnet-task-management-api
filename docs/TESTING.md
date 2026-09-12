# Testing Guide

Dokumen ini mencatat strategi, skenario, dan hasil automated testing pada Task Management API.

## Test Stack

| Tool | Fungsi |
|---|---|
| xUnit | Framework untuk menulis dan menjalankan test |
| Moq | Membuat dependency palsu pada unit test controller |
| EF Core InMemory | Menyediakan database terisolasi untuk unit test service dan seeder |
| Coverlet | Mengukur line coverage dan branch coverage |

## Test Structure

```text
TaskManagement.Api.Tests/
├── Controllers/
│   ├── AuthControllerTests.cs
│   ├── TasksControllerTests.cs
│   └── UsersControllerTests.cs
├── Data/
│   └── SeederTests.cs
├── Services/
│   ├── AuthServiceTests.cs
│   ├── TaskServiceTests.cs
│   └── UserServiceTests.cs
└── TestDbContextFactory.cs
```

## Unit Tests

Unit test memeriksa satu bagian aplikasi secara terisolasi. Controller menggunakan mock service, sedangkan service dan seeder menggunakan database in-memory yang berbeda untuk setiap test.

### Covered Scenarios

| Area | Skenario | Status |
|---|---|---|
| Auth service | Validasi register | Passed |
| Auth service | Register user dan hash password | Passed |
| Auth service | Menolak email yang sudah terdaftar | Passed |
| Auth service | Validasi login | Passed |
| Auth service | Login dan pembuatan JWT beserta claims | Passed |
| Auth service | Menolak email, password, dan password hash yang salah | Passed |
| Auth service | Menolak konfigurasi JWT tanpa key | Passed |
| Task service | Membatasi task berdasarkan pemilik | Passed |
| Task service | Pagination dan filter completion | Passed |
| Task service | Sorting berdasarkan id, title, dan completion | Passed |
| Task service | Create, read, update, dan delete task | Passed |
| User service | Create, read, update, dan delete user | Passed |
| User service | Validasi input dan email duplikat | Passed |
| Controllers | HTTP result untuk request sukses dan gagal | Passed |
| Tasks controller | Membaca user ID dari JWT claim | Passed |
| Seeders | Membuat admin, user, dan task demo | Passed |
| Seeders | Tidak menggandakan seed data | Passed |

## Latest Test Result

Hasil terakhir:

```text
Total tests : 66
Passed      : 66
Failed      : 0
Skipped     : 0
Line        : 100%
Branch      : 99.10%
```

Migration, `Program.cs`, dan source code OpenAPI hasil generator dikecualikan dari laporan coverage. Migration dan alur HTTP tetap diuji melalui integration test; persentase coverage di atas hanya berlaku untuk source code yang dihitung oleh `coverage.runsettings`.

## Running Unit Tests

Jalankan semua test:

```bash
dotnet test TaskManagement.slnx
```

Jalankan test beserta Coverlet:

```bash
dotnet test TaskManagement.slnx --settings coverage.runsettings --collect:"XPlat Code Coverage"
```

Coverlet menyimpan laporan Cobertura XML di:

```text
TaskManagement.Api.Tests/TestResults/<test-run-id>/coverage.cobertura.xml
```

Folder `TestResults` merupakan output sementara dan tidak perlu dimasukkan ke Git.

## Integration Tests

Integration test memeriksa alur aplikasi melalui HTTP host dan PostgreSQL nyata. Test membuat database dengan nama unik, menjalankan migration dan seeder, lalu menghapus database tersebut setelah suite selesai.

### Planned Scenarios

| Flow | Status |
|---|---|
| Register dan login melalui HTTP | Passed |
| Login menghasilkan JWT yang valid | Passed |
| Endpoint task menolak request tanpa token | Passed |
| User hanya dapat mengakses task miliknya | Passed |
| Admin dapat mengakses CRUD user | Passed |
| User biasa mendapat `403 Forbidden` pada endpoint user | Passed |
| Search PostgreSQL menggunakan `ILIKE` | Passed |
| Migration dapat diterapkan ke database kosong | Passed |
| Seeder membuat data awal | Passed |
| Database refresh menjalankan `TRUNCATE` dan seed ulang | Passed |
| Error response dari exception handler dan authorization middleware | Passed |

## Testing Progress

- [x] Membuat project xUnit
- [x] Memahami pola Arrange, Act, Assert
- [x] Menguji controller dengan Moq
- [x] Menguji service dengan EF Core InMemory
- [x] Menguji authentication dan JWT
- [x] Menguji database seeder
- [x] Mengukur coverage dengan Coverlet
- [x] Menyiapkan PostgreSQL khusus integration test
- [x] Menguji aplikasi melalui HTTP host
- [x] Menguji migration dan query PostgreSQL
- [ ] Menjalankan test otomatis melalui CI
