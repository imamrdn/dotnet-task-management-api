using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Errors;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetUsersAsync_ReturnsUsersOrderedById()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            new User { Id = 2, Name = "Second", Email = "second@mail.com" },
            new User { Id = 1, Name = "First", Email = "first@mail.com" });
        await context.SaveChangesAsync();

        var result = await CreateService(context).GetUsersAsync();

        Assert.Equal([1, 2], result.Select(user => user.Id));
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUserOrNull()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Id = 1, Name = "User", Email = "user@mail.com" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        Assert.NotNull(await service.GetUserByIdAsync(1));
        Assert.Null(await service.GetUserByIdAsync(99));
    }

    [Fact]
    public async Task GetUserProfileAsync_ReturnsProfileOrNull()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Id = 1, Name = "User", Email = "user@mail.com" });
        context.UserProfiles.Add(new UserProfile
        {
            Id = 1,
            UserId = 1,
            Bio = "Bio",
            Location = "Location"
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetUserProfileAsync(1);

        Assert.Equal("Bio", result!.Bio);
        Assert.Null(await service.GetUserProfileAsync(99));
    }

    [Fact]
    public async Task UpsertUserProfileAsync_CreatesUpdatesOrReturnsNull()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Id = 1, Name = "User", Email = "user@mail.com" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var created = await service.UpsertUserProfileAsync(
            1,
            new UpsertUserProfileRequest("Initial bio", "Initial location"));
        var updated = await service.UpsertUserProfileAsync(
            1,
            new UpsertUserProfileRequest("Updated bio", "Updated location"));

        Assert.Equal(created!.Id, updated!.Id);
        Assert.Equal("Updated bio", updated.Bio);
        Assert.Equal("Updated location", updated.Location);
        Assert.Equal(1, context.UserProfiles.Count());
        Assert.NotNull(Assert.Single(context.UserProfiles).UpdatedAt);
        Assert.Null(await service.UpsertUserProfileAsync(
            99,
            new UpsertUserProfileRequest("Missing", "Missing")));
    }

    [Fact]
    public async Task GetUsersWithoutActiveTasksAsync_ReturnsUsersMatchedBySubquery()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            new User { Id = 1, Name = "Active", Email = "active@mail.com" },
            new User { Id = 2, Name = "Deleted Only", Email = "deleted@mail.com" },
            new User { Id = 3, Name = "Empty", Email = "empty@mail.com" });
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Active task" },
            new TaskItem { Id = 2, UserId = 2, Title = "Deleted task", IsDeleted = true });
        await context.SaveChangesAsync();

        var result = await CreateService(context).GetUsersWithoutActiveTasksAsync();

        Assert.Equal([2, 3], result.Select(user => user.Id));
    }

    [Theory]
    [InlineData("", "user@mail.com", "secret123", "Name is required")]
    [InlineData("User", "", "secret123", "Email is required")]
    [InlineData("User", "user@mail.com", "", "Password is required")]
    public async Task CreateUserAsync_InvalidRequest_Throws(
        string name, string email, string password, string message)
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateUserAsync(new CreateUserRequest(name, email, password)));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public async Task CreateUserAsync_ValidRequest_CreatesUser()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var result = await service.CreateUserAsync(
            new CreateUserRequest("User", "user@mail.com", "secret123"));

        Assert.Equal("user@mail.com", result.Email);
        Assert.NotEqual("secret123", Assert.Single(context.Users).PasswordHash);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_Throws()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Email = "user@mail.com" });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<DuplicateResourceException>(() => CreateService(context)
            .CreateUserAsync(new CreateUserRequest("User", "user@mail.com", "secret123")));
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesFieldsAndOptionalPassword()
    {
        await using var context = TestDbContextFactory.Create();
        var user = new User { Id = 1, Name = "Old", Email = "old@mail.com", PasswordHash = "old-hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.UpdateUserAsync(1,
            new UpdateUserRequest("New", "new@mail.com", "new-password"));

        Assert.Equal("New", result!.Name);
        Assert.NotEqual("old-hash", user.PasswordHash);
        Assert.Null(await service.UpdateUserAsync(99,
            new UpdateUserRequest("Missing", "missing@mail.com", null)));
    }

    [Fact]
    public async Task UpdateUserAsync_WithoutPassword_KeepsExistingHash()
    {
        await using var context = TestDbContextFactory.Create();
        var user = new User { Id = 1, Name = "Old", Email = "old@mail.com", PasswordHash = "old-hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await CreateService(context).UpdateUserAsync(1,
            new UpdateUserRequest("New", "new@mail.com", null));

        Assert.Equal("old-hash", user.PasswordHash);
    }

    [Fact]
    public async Task UpdateUserAsync_DuplicateEmail_Throws()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            new User { Id = 1, Email = "first@mail.com" },
            new User { Id = 2, Email = "second@mail.com" });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<DuplicateResourceException>(() => CreateService(context)
            .UpdateUserAsync(1, new UpdateUserRequest("First", "second@mail.com", null)));
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsWhetherUserWasDeleted()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Id = 1 });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        Assert.True(await service.DeleteUserAsync(1));
        Assert.False(await service.DeleteUserAsync(99));
    }

    [Fact]
    public async Task DeleteUserAsync_UserWithActiveTasks_ThrowsAndKeepsUserAndTasks()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Id = 1, Name = "Owner", Email = "owner@mail.com" });
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Active" },
            new TaskItem { Id = 2, UserId = 1, Title = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteUserAsync(1));

        Assert.Equal("User cannot be deleted because the user has tasks", exception.Message);
        Assert.NotNull(await context.Users.FindAsync(1));
        Assert.Equal(2, await context.Tasks.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task DeleteUserAsync_UserWithOnlySoftDeletedTasks_ThrowsArgumentException()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Id = 1 });
        context.Tasks.Add(new TaskItem { Id = 1, UserId = 1, Title = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(context).DeleteUserAsync(1));
        Assert.NotNull(await context.Users.FindAsync(1));
    }

    [Fact]
    public async Task WriteOperations_LogUserIdsWithoutPersonalData()
    {
        await using var context = TestDbContextFactory.Create();
        var logger = new TestLogger<UserService>();
        var service = new UserService(context, new PasswordHasher<User>(), logger);

        var created = await service.CreateUserAsync(
            new CreateUserRequest("User", "user@mail.com", "secret123"));
        await service.UpdateUserAsync(created.Id,
            new UpdateUserRequest("Updated", "updated@mail.com", null));
        await service.DeleteUserAsync(created.Id);

        Assert.Equal(3, logger.Entries.Count);
        Assert.All(logger.Entries, entry =>
        {
            Assert.Equal(LogLevel.Information, entry.Level);
            Assert.Contains($"User {created.Id}", entry.Message);
            Assert.DoesNotContain("@", entry.Message);
            Assert.DoesNotContain("secret123", entry.Message);
        });
    }

    private static UserService CreateService(AppDbContext context) =>
        new(context, new PasswordHasher<User>(), NullLogger<UserService>.Instance);
}
