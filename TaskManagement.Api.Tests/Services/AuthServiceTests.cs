using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
        var service = new AuthService(context, Configuration);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(new RegisterRequest(name, email, password)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesRegularUserWithHashedPassword()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new AuthService(context, Configuration);

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
        var service = new AuthService(context, Configuration);

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
        var service = new AuthService(context, Configuration);

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
        var service = new AuthService(context, Configuration);

        var response = await service.LoginAsync(new LoginRequest(user.Email, "secret123"));
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == user.Email);
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
        var service = new AuthService(context, Configuration);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest(email, password)));
    }

    [Fact]
    public async Task LoginAsync_MalformedPasswordHash_ThrowsUnauthorized()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Email = "user@mail.com", PasswordHash = "invalid-hash" });
        await context.SaveChangesAsync();
        var service = new AuthService(context, Configuration);

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
        var service = new AuthService(context, new ConfigurationBuilder().Build());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LoginAsync(new LoginRequest(user.Email, "secret123")));
    }
}
