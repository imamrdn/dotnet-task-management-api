using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Seeders;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public ApiIntegrationTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks?page=1&limit=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False((await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Success);
    }

    [Fact]
    public async Task Cors_PreflightFromAllowedOrigin_ReturnsCorsHeaders()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/tasks");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Contains("http://localhost:5173", origins);
    }

    [Fact]
    public async Task HealthEndpoint_WhenDatabaseIsAvailable_ReturnsHealthy()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task OpenApiDocument_IncludesMetadataPathsAndBearerSecurity()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Task Management API", root.GetProperty("info").GetProperty("title").GetString());
        Assert.Equal("v1", root.GetProperty("info").GetProperty("version").GetString());
        Assert.True(root.GetProperty("paths").TryGetProperty("/api/auth/login", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/api/tasks", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/api/users", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.True(root.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
        Assert.True(root.GetProperty("security").GetArrayLength() > 0);
    }

    [Fact]
    public async Task InvalidRegisterAndLogin_ReturnConsistentErrorBody()
    {
        using var client = _factory.CreateClient();

        var badRegister = await client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("", "bad@mail.com", "secret123"));
        var badLogin = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("user@mail.com", "wrong"));

        Assert.Equal(HttpStatusCode.BadRequest, badRegister.StatusCode);
        Assert.Equal("Name is required",
            (await badRegister.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, badLogin.StatusCode);
        Assert.False((await badLogin.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Success);
    }

    [Theory]
    [InlineData("", "Description", "Title is required")]
    [InlineData("Title", "", "Description is required")]
    public async Task CreateTask_InvalidFields_ReturnsValidationError(
        string title, string description, string expectedMessage)
    {
        using var client = await CreateAuthenticatedClientAsync("user@mail.com");

        var response = await client.PostAsJsonAsync(
            "/api/tasks", new CreateTaskRequest(title, description));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body!.Success);
        Assert.Equal(expectedMessage, body.Message);
    }

    [Fact]
    public async Task CreateTask_MissingTitle_ReturnsValidationError()
    {
        using var client = await CreateAuthenticatedClientAsync("user@mail.com");

        var response = await client.PostAsJsonAsync(
            "/api/tasks", new { description = "Description" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Title is required",
            (await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Message);
    }

    [Fact]
    public async Task UpdateTask_EmptyDescription_ReturnsValidationError()
    {
        using var client = await CreateAuthenticatedClientAsync("user@mail.com");

        var response = await client.PutAsJsonAsync(
            "/api/tasks/1", new UpdateTaskRequest("Title", "", false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Description is required",
            (await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Message);
    }

    [Fact]
    public async Task AuthAndUser_InvalidEmail_ReturnValidationError()
    {
        using var publicClient = _factory.CreateClient();
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var register = await publicClient.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("User", "invalid", "secret123"));
        var login = await publicClient.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("invalid", "secret123"));
        var createUser = await adminClient.PostAsJsonAsync(
            "/api/users", new CreateUserRequest("User", "invalid", "secret123"));

        foreach (var response in new[] { register, login, createUser })
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Email is invalid",
                (await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Message);
        }
    }

    [Fact]
    public async Task MissingTask_ReturnsWrappedNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync("user@mail.com");

        var response = await client.GetAsync("/api/tasks/999999");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Task not found", body!.Message);
        Assert.Null(body.Data);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsUsableJwt()
    {
        using var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("user@mail.com", "secret123"));
        var auth = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Data!.Token);
        var tasksResponse = await client.GetAsync("/api/tasks?page=1&limit=10");

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, tasksResponse.StatusCode);
    }

    [Fact]
    public async Task RefreshAndLogout_RotateAndRevokeRefreshToken()
    {
        using var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("user@mail.com", "secret123"));
        var login = (await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!;

        var refreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(login.RefreshToken));
        var refresh = (await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!;

        var reusedResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(login.RefreshToken));

        var logoutResponse = await client.PostAsJsonAsync(
            "/api/auth/logout",
            new LogoutRequest(refresh.RefreshToken));

        var revokedResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(refresh.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        Assert.NotEqual(login.RefreshToken, refresh.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, reusedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, revokedResponse.StatusCode);
    }

    [Fact]
    public async Task UsersEndpoint_EnforcesAdminRole()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var forbiddenResponse = await userClient.GetAsync("/api/users");
        var adminResponse = await adminClient.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal("Forbidden", (await forbiddenResponse.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Message);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    [Fact]
    public async Task UsersWithoutTasksEndpoint_EnforcesAdminRoleAndReturnsSubqueryResult()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/users", new CreateUserRequest("Empty User", "empty@mail.com", "secret123"));
        var forbiddenResponse = await userClient.GetAsync("/api/users/without-tasks");
        var adminResponse = await adminClient.GetAsync("/api/users/without-tasks");
        var result = await adminResponse.Content.ReadFromJsonAsync<ApiResponse<List<UserResponse>>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Contains(result!.Data!, user => user.Email == "empty@mail.com");
        Assert.DoesNotContain(result.Data!, user => user.Email == "admin@mail.com");
        Assert.DoesNotContain(result.Data!, user => user.Email == "user@mail.com");
    }

    [Fact]
    public async Task UserProfileEndpoint_EnforcesAdminRoleAndPersistsOneToOneProfile()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var forbiddenResponse = await userClient.PutAsJsonAsync(
            "/api/users/2/profile",
            new UpsertUserProfileRequest("User bio", "Bandung"));
        var updateResponse = await adminClient.PutAsJsonAsync(
            "/api/users/2/profile",
            new UpsertUserProfileRequest("User bio", "Bandung"));
        var getResponse = await adminClient.GetAsync("/api/users/2/profile");
        var result = await getResponse.Content.ReadFromJsonAsync<ApiResponse<UserProfileResponse>>();

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(2, result!.Data!.UserId);
        Assert.Equal("User bio", result.Data.Bio);
        Assert.Equal("Bandung", result.Data.Location);
    }

    [Fact]
    public async Task CategoryEndpoints_EnforceAdminRoleAndReturnCategories()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var forbiddenResponse = await userClient.GetAsync("/api/categories");
        var adminResponse = await adminClient.GetAsync("/api/categories");
        var result = await adminResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryResponse>>>();

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Contains(result!.Data!, category => category.Name == "Backend");
        Assert.Contains(result.Data!, category => category.Name == "Database");
    }

    [Fact]
    public async Task CreateCategory_DuplicateName_ReturnsBadRequestInsteadOfServerError()
    {
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var response = await adminClient.PostAsJsonAsync(
            "/api/categories", new CreateCategoryRequest("Backend"));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body!.Success);
        Assert.Equal("Category name is already registered", body.Message);
    }

    [Fact]
    public async Task CreateCategory_DuplicateNameOnUpdate_ReturnsBadRequestInsteadOfServerError()
    {
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/categories", new CreateCategoryRequest("Temporary Category"));
        var created = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryResponse>>())!.Data!;

        var response = await adminClient.PutAsJsonAsync(
            $"/api/categories/{created.Id}", new UpdateCategoryRequest("Backend"));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body!.Success);
        Assert.Equal("Category name is already registered", body.Message);
    }

    [Fact]
    public async Task TaskCategoryEndpoint_AssignsManyCategoriesToOwnedTask()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");

        var createResponse = await userClient.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest("Categorized task", "Testing many-to-many"));
        var created = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TaskResponse>>())!.Data!;

        var assignResponse = await userClient.PutAsJsonAsync(
            $"/api/tasks/{created.Id}/categories",
            new AssignTaskCategoriesRequest([1, 2]));
        var getResponse = await userClient.GetAsync($"/api/tasks/{created.Id}/categories");
        var result = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryResponse>>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(["Backend", "Database"], result!.Data!.Select(category => category.Name));
    }

    [Fact]
    public async Task AdminTaskOwnerEndpoint_EnforcesAdminRoleAndReturnsOwnerData()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var forbiddenResponse = await userClient.GetAsync("/api/tasks/admin/all");
        var adminResponse = await adminClient.GetAsync("/api/tasks/admin/all");
        var result = await adminResponse.Content.ReadFromJsonAsync<ApiResponse<List<TaskWithOwnerResponse>>>();

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.NotNull(result!.Data);
        Assert.Contains(result.Data, task => task.Owner.Email == "admin@mail.com");
        Assert.Contains(result.Data, task => task.Owner.Email == "user@mail.com");
    }

    [Fact]
    public async Task AdminTaskSummaryEndpoint_EnforcesAdminRoleAndReturnsGroupedCounts()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var forbiddenResponse = await userClient.GetAsync("/api/tasks/admin/summary");
        var adminResponse = await adminClient.GetAsync("/api/tasks/admin/summary?minimumTasks=3");
        var result = await adminResponse.Content.ReadFromJsonAsync<ApiResponse<List<TaskSummaryByUserResponse>>>();

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.NotNull(result!.Data);

        var adminSummary = Assert.Single(result.Data, summary => summary.Email == "admin@mail.com");
        Assert.Equal(3, adminSummary.TotalTasks);
        Assert.Equal(2, adminSummary.CompletedTasks);
        Assert.Equal(1, adminSummary.PendingTasks);

        var userSummary = Assert.Single(result.Data, summary => summary.Email == "user@mail.com");
        Assert.Equal(3, userSummary.TotalTasks);
        Assert.Equal(2, userSummary.CompletedTasks);
        Assert.Equal(1, userSummary.PendingTasks);
    }

    [Fact]
    public async Task AdminTaskSummaryEndpoint_RejectsInvalidMinimumTasks()
    {
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var response = await adminClient.GetAsync("/api/tasks/admin/summary?minimumTasks=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Minimum tasks must be greater than 0",
            (await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Message);
    }

    [Fact]
    public async Task AdminTopTaskOwnersEndpoint_EnforcesAdminRoleAndReturnsLimitedOwners()
    {
        using var userClient = await CreateAuthenticatedClientAsync("user@mail.com");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var forbiddenResponse = await userClient.GetAsync("/api/tasks/admin/top-users?limit=1");
        var adminResponse = await adminClient.GetAsync("/api/tasks/admin/top-users?limit=1");
        var result = await adminResponse.Content.ReadFromJsonAsync<ApiResponse<List<TopTaskOwnerResponse>>>();

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        var owner = Assert.Single(result!.Data!);
        Assert.Equal("admin@mail.com", owner.Email);
        Assert.Equal(3, owner.TotalTasks);
    }

    [Fact]
    public async Task AdminTopTaskOwnersEndpoint_RejectsInvalidLimit()
    {
        using var adminClient = await CreateAuthenticatedClientAsync("admin@mail.com");

        var response = await adminClient.GetAsync("/api/tasks/admin/top-users?limit=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Limit must be greater than 0",
            (await response.Content.ReadFromJsonAsync<ApiResponse<object>>())!.Message);
    }

    [Fact]
    public async Task TaskList_IsOwnerScopedAndSupportsPostgresSearch()
    {
        using var client = await CreateAuthenticatedClientAsync("user@mail.com");

        var response = await client.GetAsync(
            "/api/tasks?page=1&limit=10&search=PERSONAL&isCompleted=true&sortBy=title&sortDirection=asc");
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<TaskResponse>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = Assert.Single(result!.Data!.Items);
        Assert.Equal("Create a personal task", task.Title);
        Assert.DoesNotContain(result.Data.Items, item => item.Title == "Review registered users");
    }

    [Fact]
    public async Task TaskCrud_WorksThroughHttp()
    {
        using var client = await CreateAuthenticatedClientAsync("user@mail.com");

        var createResponse = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest("Integration task", "Created through HTTP"));
        var created = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TaskResponse>>())!.Data!;

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/tasks/{created!.Id}",
            new UpdateTaskRequest("Updated integration task", "Updated through HTTP", true));
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<TaskResponse>>())!.Data!;

        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/tasks/{created.Id}/completion",
            new UpdateTaskCompletionRequest(false));
        var patched = (await patchResponse.Content.ReadFromJsonAsync<ApiResponse<TaskResponse>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");
        var getResponse = await client.GetAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.True(updated!.IsCompleted);
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        Assert.False(patched!.IsCompleted);
        Assert.Equal("Updated integration task", patched.Title);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Empty(await deleteResponse.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task RefreshDatabase_TruncatesAndSeedsAgain()
    {
        await _factory.RefreshDatabaseAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(2, context.Users.Count());
        Assert.Equal(6, context.Tasks.Count());
        Assert.Equal(1, context.Users.Min(user => user.Id));
    }

    [Fact]
    public async Task RefreshDatabase_WhenSeedingFails_RollsBackTruncate()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Users.Add(new User
        {
            Name = "Preserved User",
            Email = "preserved@mail.com",
            PasswordHash = "test-only"
        });
        await context.SaveChangesAsync();
        var taskCountBeforeRefresh = await context.Tasks.CountAsync();

        await context.Database.ExecuteSqlRawAsync(
            "ALTER TABLE users ADD CONSTRAINT test_seed_failure " +
            "CHECK (\"Email\" <> 'admin@mail.com') NOT VALID;");

        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await Assert.ThrowsAsync<DbUpdateException>(() => seeder.RefreshAsync());

            await using var verifyScope = _factory.Services.CreateAsyncScope();
            var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.True(await verifyContext.Users.AnyAsync(user => user.Email == "preserved@mail.com"));
            Assert.Equal(taskCountBeforeRefresh, await verifyContext.Tasks.CountAsync());
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE users DROP CONSTRAINT test_seed_failure;");
        }
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, "secret123"));
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Data!.Token);
        return client;
    }
}
