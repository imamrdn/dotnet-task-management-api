using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Services;

public class TaskServiceTests
{
    [Fact]
    public async Task GetTasksAsync_FiltersByOwnerCompletionAndPaginates()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "A", IsCompleted = false },
            new TaskItem { Id = 2, UserId = 1, Title = "B", IsCompleted = true },
            new TaskItem { Id = 3, UserId = 1, Title = "C", IsCompleted = true },
            new TaskItem { Id = 4, UserId = 2, Title = "Other", IsCompleted = true },
            new TaskItem { Id = 5, UserId = 1, Title = "Deleted", IsCompleted = true, IsDeleted = true });
        await context.SaveChangesAsync();

        var result = await CreateService(context)
            .GetTasksAsync(1, 1, 1, null, true, "title", "desc");

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("C", Assert.Single(result.Items).Title);
    }

    [Theory]
    [InlineData("title", "asc", "A")]
    [InlineData("iscompleted", "asc", "B")]
    [InlineData("id", "desc", "A")]
    [InlineData(null, null, "B")]
    public async Task GetTasksAsync_SortsUsingRequestedField(
        string? sortBy, string? direction, string expectedFirstTitle)
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "B", IsCompleted = false },
            new TaskItem { Id = 2, UserId = 1, Title = "A", IsCompleted = true });
        await context.SaveChangesAsync();

        var result = await CreateService(context)
            .GetTasksAsync(1, 1, 10, null, null, sortBy, direction);

        Assert.Equal(expectedFirstTitle, result.Items[0].Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_OnlyReturnsOwnedTask()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Owned" },
            new TaskItem { Id = 2, UserId = 1, Title = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        Assert.NotNull(await service.GetTaskByIdAsync(1, 1));
        Assert.Null(await service.GetTaskByIdAsync(2, 1));
        Assert.Null(await service.GetTaskByIdAsync(1, 2));
    }

    [Fact]
    public async Task GetAllTasksWithOwnersAsync_ReturnsTasksWithOwnerData()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            new User { Id = 1, Name = "Admin", Email = "admin@mail.com" },
            new User { Id = 2, Name = "User", Email = "user@mail.com" });
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Admin task", Description = "Admin description" },
            new TaskItem { Id = 2, UserId = 2, Title = "User task", Description = "User description" },
            new TaskItem { Id = 3, UserId = 2, Title = "Deleted task", Description = "Deleted description", IsDeleted = true });
        await context.SaveChangesAsync();

        var result = await CreateService(context).GetAllTasksWithOwnersAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Admin", result[0].Owner.Name);
        Assert.Equal("user@mail.com", result[1].Owner.Email);
        Assert.DoesNotContain(result, task => task.Title == "Deleted task");
    }

    [Fact]
    public async Task GetTaskSummaryByUserAsync_GroupsActiveTasksByOwner()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            new User { Id = 1, Name = "Admin", Email = "admin@mail.com" },
            new User { Id = 2, Name = "User", Email = "user@mail.com" });
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Admin done", IsCompleted = true },
            new TaskItem { Id = 2, UserId = 1, Title = "Admin todo", IsCompleted = false },
            new TaskItem { Id = 3, UserId = 1, Title = "Admin deleted", IsCompleted = true, IsDeleted = true },
            new TaskItem { Id = 4, UserId = 2, Title = "User done", IsCompleted = true });
        await context.SaveChangesAsync();

        var result = await CreateService(context).GetTaskSummaryByUserAsync(null);

        Assert.Equal(2, result.Count);
        Assert.Equal(new TaskSummaryByUserResponse(1, "Admin", "admin@mail.com", 2, 1, 1), result[0]);
        Assert.Equal(new TaskSummaryByUserResponse(2, "User", "user@mail.com", 1, 1, 0), result[1]);
    }

    [Fact]
    public async Task GetTaskSummaryByUserAsync_FiltersGroupsByMinimumTasks()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            new User { Id = 1, Name = "Admin", Email = "admin@mail.com" },
            new User { Id = 2, Name = "User", Email = "user@mail.com" });
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Admin done", IsCompleted = true },
            new TaskItem { Id = 2, UserId = 1, Title = "Admin todo", IsCompleted = false },
            new TaskItem { Id = 3, UserId = 2, Title = "User done", IsCompleted = true });
        await context.SaveChangesAsync();

        var result = await CreateService(context).GetTaskSummaryByUserAsync(2);

        var summary = Assert.Single(result);
        Assert.Equal("admin@mail.com", summary.Email);
        Assert.Equal(2, summary.TotalTasks);
    }

    [Fact]
    public async Task CreateTaskAsync_AssignsOwnerAndDefaultsToIncomplete()
    {
        await using var context = TestDbContextFactory.Create();

        var result = await CreateService(context)
            .CreateTaskAsync(7, new CreateTaskRequest("Title", "Description"));

        var task = Assert.Single(context.Tasks);
        Assert.Equal(7, task.UserId);
        Assert.True(task.CreatedAt > DateTime.MinValue);
        Assert.Null(task.UpdatedAt);
        Assert.Null(task.DeletedAt);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task UpdateTaskAsync_UpdatesOwnedTaskOrReturnsNull()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.Add(new TaskItem { Id = 1, UserId = 1, Title = "Old" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.UpdateTaskAsync(1, 1,
            new UpdateTaskRequest("New", "Updated", true));

        Assert.Equal("New", result!.Title);
        Assert.NotNull((await context.Tasks.FindAsync(1))!.UpdatedAt);
        Assert.Null(await service.UpdateTaskAsync(2, 1,
            new UpdateTaskRequest("Blocked", "Blocked", false)));
    }

    [Fact]
    public async Task DeleteTaskAsync_DeletesOwnedTaskOnly()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.Add(new TaskItem { Id = 1, UserId = 1 });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        Assert.False(await service.DeleteTaskAsync(2, 1));
        Assert.True(await service.DeleteTaskAsync(1, 1));
        Assert.True((await context.Tasks.FindAsync(1))!.IsDeleted);
        Assert.NotNull((await context.Tasks.FindAsync(1))!.DeletedAt);
        Assert.Null(await service.GetTaskByIdAsync(1, 1));
    }

    [Fact]
    public async Task WriteOperations_LogOnlySuccessfulChanges()
    {
        await using var context = TestDbContextFactory.Create();
        var logger = new TestLogger<TaskService>();
        var service = new TaskService(context, logger);

        var created = await service.CreateTaskAsync(7,
            new CreateTaskRequest("Title", "Description"));
        await service.UpdateTaskAsync(7, created.Id,
            new UpdateTaskRequest("Updated", "Description", true));
        await service.DeleteTaskAsync(8, created.Id);
        await service.DeleteTaskAsync(7, created.Id);

        Assert.Equal(3, logger.Entries.Count);
        Assert.All(logger.Entries, entry => Assert.Equal(LogLevel.Information, entry.Level));
        Assert.Contains(logger.Entries, entry => entry.Message.Contains($"Task {created.Id} created by user 7"));
        Assert.Contains(logger.Entries, entry => entry.Message.Contains($"Task {created.Id} updated by user 7"));
        Assert.Contains(logger.Entries, entry => entry.Message.Contains($"Task {created.Id} deleted by user 7"));
        Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("Description"));
    }

    private static TaskService CreateService(AppDbContext context) =>
        new(context, NullLogger<TaskService>.Instance);
}
