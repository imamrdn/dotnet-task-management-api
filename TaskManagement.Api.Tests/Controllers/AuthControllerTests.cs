using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsOkForValidRequest()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = CreateController(context);

        Assert.IsType<OkObjectResult>(await controller.Register(
            new RegisterRequest("User", "user@mail.com", "secret123")));
    }

    [Fact]
    public async Task Register_ThrowsForGlobalHandlerOnValidationAndDuplicateEmail()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = CreateController(context);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.Register(
            new RegisterRequest("", "user@mail.com", "secret123")));

        await controller.Register(new RegisterRequest("User", "user@mail.com", "secret123"));
        await Assert.ThrowsAsync<ArgumentException>(() => controller.Register(
            new RegisterRequest("User", "user@mail.com", "secret123")));
    }

    [Fact]
    public async Task Login_ReturnsOkOrThrowsForGlobalHandler()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = CreateController(context);
        await controller.Register(new RegisterRequest("User", "user@mail.com", "secret123"));

        Assert.IsType<OkObjectResult>(await controller.Login(
            new LoginRequest("user@mail.com", "secret123")));
        await Assert.ThrowsAsync<ArgumentException>(() => controller.Login(
            new LoginRequest("", "secret123")));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.Login(
            new LoginRequest("user@mail.com", "wrong")));
    }

    private static AuthController CreateController(TaskManagement.Api.Data.AppDbContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "a-test-secret-key-that-is-at-least-32-characters-long",
                ["Jwt:Issuer"] = "Tests",
                ["Jwt:Audience"] = "Tests"
            })
            .Build();

        return new AuthController(new AuthService(
            context, configuration, NullLogger<AuthService>.Instance));
    }
}
