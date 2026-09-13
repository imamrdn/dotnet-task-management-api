using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Services;

public class AuthServiceTests
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "a-test-secret-key-that-is-at-least-32-characters-long",
            ["Jwt:Issuer"] = "TaskManagement.Tests",
            ["Jwt:Audience"] = "TaskManagement.Tests"
        })
        .Build();

    [Theory]
    [InlineData("", "user@mail.com", "secret123", "Name is required")]
    [InlineData("User", "", "secret123", "Email is required")]
    [InlineData("User", "user@mail.com", "", "Password is required")]
    public async Task RegisterAsync_InvalidRequest_ThrowsArgumentException(
        string name, string email, string password, string message)
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(new RegisterRequest(name, email, password)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesRegularUserWithHashedPassword()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        await service.RegisterAsync(new RegisterRequest("User", "user@mail.com", "secret123"));

        var user = Assert.Single(context.Users);
        Assert.Equal("User", user.Role);
        Assert.NotEqual("secret123", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsArgumentException()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Email = "user@mail.com" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(new RegisterRequest("User", "user@mail.com", "secret123")));

        Assert.Equal("Email is already registered", exception.Message);
    }

    [Theory]
    [InlineData("", "secret123", "Email is required")]
    [InlineData("user@mail.com", "", "Password is required")]
    public async Task LoginAsync_InvalidRequest_ThrowsArgumentException(
        string email, string password, string message)
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.LoginAsync(new LoginRequest(email, password)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsJwtWithUserClaims()
    {
        await using var context = TestDbContextFactory.Create();
        var user = new User { Id = 7, Name = "Admin", Email = "admin@mail.com", Role = "Admin" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "secret123");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var response = await service.LoginAsync(new LoginRequest(user.Email, "secret123"));
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.Single(context.RefreshTokens);
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == user.Email);
    }

    [Fact]
    public async Task RefreshAsync_ValidRefreshToken_RotatesToken()
    {
        await using var context = TestDbContextFactory.Create();
        var user = new User { Id = 7, Name = "User", Email = "user@mail.com", Role = "User" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "secret123");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = CreateService(context);
        var login = await service.LoginAsync(new LoginRequest(user.Email, "secret123"));

        var refresh = await service.RefreshAsync(new RefreshTokenRequest(login.RefreshToken));

        Assert.NotEqual(login.RefreshToken, refresh.RefreshToken);
        Assert.Equal(2, context.RefreshTokens.Count());
        Assert.Equal(1, context.RefreshTokens.Count(token => token.RevokedAt != null));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshAsync(new RefreshTokenRequest(login.RefreshToken)));
    }

    [Fact]
    public async Task RefreshAsync_InvalidRefreshToken_ThrowsUnauthorized()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshAsync(new RefreshTokenRequest("invalid-refresh-token")));
    }

    [Fact]
    public async Task LogoutAsync_RevokesRefreshToken()
    {
        await using var context = TestDbContextFactory.Create();
        var user = new User { Id = 7, Name = "User", Email = "user@mail.com", Role = "User" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "secret123");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = CreateService(context);
        var login = await service.LoginAsync(new LoginRequest(user.Email, "secret123"));

        await service.LogoutAsync(new LogoutRequest(login.RefreshToken));

        Assert.NotNull(Assert.Single(context.RefreshTokens).RevokedAt);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshAsync(new RefreshTokenRequest(login.RefreshToken)));
    }

    [Theory]
    [InlineData("missing@mail.com", "secret123")]
    [InlineData("user@mail.com", "wrong-password")]
    public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorized(string email, string password)
    {
        await using var context = TestDbContextFactory.Create();
        var user = new User { Name = "User", Email = "user@mail.com" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "secret123");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest(email, password)));
    }

    [Fact]
    public async Task LoginAsync_MalformedPasswordHash_ThrowsUnauthorized()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Email = "user@mail.com", PasswordHash = "invalid-hash" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("user@mail.com", "secret123")));
    }

    [Fact]
    public async Task LoginAsync_MissingJwtKey_ThrowsInvalidOperationException()
    {
        await using var context = TestDbContextFactory.Create();
        var user = new User { Email = "user@mail.com" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "secret123");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = CreateService(context, new ConfigurationBuilder().Build());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LoginAsync(new LoginRequest(user.Email, "secret123")));
    }

    [Fact]
    public async Task Authentication_LogsOutcomeWithoutCredentialsOrToken()
    {
        await using var context = TestDbContextFactory.Create();
        var logger = new TestLogger<AuthService>();
        var service = new AuthService(context, Configuration, logger);

        await service.RegisterAsync(new RegisterRequest("User", "user@mail.com", "secret123"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("user@mail.com", "wrong-password")));
        var response = await service.LoginAsync(new LoginRequest("user@mail.com", "secret123"));

        Assert.Equal(3, logger.Entries.Count);
        Assert.Equal(LogLevel.Warning, logger.Entries[1].Level);
        Assert.Contains("Login failed", logger.Entries[1].Message);
        Assert.Equal(LogLevel.Information, logger.Entries[2].Level);
        Assert.Contains("logged in", logger.Entries[2].Message);
        Assert.All(logger.Entries, entry =>
        {
            Assert.DoesNotContain("@", entry.Message);
            Assert.DoesNotContain("secret123", entry.Message);
            Assert.DoesNotContain("wrong-password", entry.Message);
            Assert.DoesNotContain(response.Token, entry.Message);
            Assert.DoesNotContain(response.RefreshToken, entry.Message);
        });
    }

    private static AuthService CreateService(AppDbContext context, IConfiguration? configuration = null) =>
        new(context, configuration ?? Configuration, NullLogger<AuthService>.Instance);
}
