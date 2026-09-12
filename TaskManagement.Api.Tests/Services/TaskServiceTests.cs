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
            new TaskItem { Id = 4, UserId = 2, Title = "Other", IsCompleted = true });
        await context.SaveChangesAsync();

        var result = await new TaskService(context)
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

        var result = await new TaskService(context)
            .GetTasksAsync(1, 1, 10, null, null, sortBy, direction);

        Assert.Equal(expectedFirstTitle, result.Items[0].Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_OnlyReturnsOwnedTask()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.Add(new TaskItem { Id = 1, UserId = 1, Title = "Owned" });
        await context.SaveChangesAsync();
        var service = new TaskService(context);

        Assert.NotNull(await service.GetTaskByIdAsync(1, 1));
        Assert.Null(await service.GetTaskByIdAsync(2, 1));
    }

    [Fact]
    public async Task CreateTaskAsync_AssignsOwnerAndDefaultsToIncomplete()
    {
        await using var context = TestDbContextFactory.Create();

        var result = await new TaskService(context)
            .CreateTaskAsync(7, new CreateTaskRequest("Title", "Description"));

        var task = Assert.Single(context.Tasks);
        Assert.Equal(7, task.UserId);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task UpdateTaskAsync_UpdatesOwnedTaskOrReturnsNull()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.Add(new TaskItem { Id = 1, UserId = 1, Title = "Old" });
        await context.SaveChangesAsync();
        var service = new TaskService(context);

        var result = await service.UpdateTaskAsync(1, 1,
            new UpdateTaskRequest("New", "Updated", true));

        Assert.Equal("New", result!.Title);
        Assert.Null(await service.UpdateTaskAsync(2, 1,
            new UpdateTaskRequest("Blocked", "Blocked", false)));
    }

    [Fact]
    public async Task DeleteTaskAsync_DeletesOwnedTaskOnly()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.Add(new TaskItem { Id = 1, UserId = 1 });
        await context.SaveChangesAsync();
        var service = new TaskService(context);

        Assert.False(await service.DeleteTaskAsync(2, 1));
        Assert.True(await service.DeleteTaskAsync(1, 1));
        Assert.Empty(context.Tasks);
    }
}
