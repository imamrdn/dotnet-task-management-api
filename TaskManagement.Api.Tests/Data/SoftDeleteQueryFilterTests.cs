using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Tests.Data;

public class SoftDeleteQueryFilterTests
{
    [Fact]
    public async Task TasksQuery_ExcludesSoftDeletedTasksWithoutExplicitFilter()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Active" },
            new TaskItem { Id = 2, UserId = 1, Title = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();

        // No .WhereActive() / .Where(t => !t.IsDeleted) here on purpose:
        // the global query filter must exclude the soft-deleted task automatically.
        var tasks = await context.Tasks.ToListAsync();

        var task = Assert.Single(tasks);
        Assert.Equal("Active", task.Title);
    }

    [Fact]
    public async Task TasksQuery_CanIncludeSoftDeletedTasksWhenFilterIsIgnored()
    {
        await using var context = TestDbContextFactory.Create();
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Active" },
            new TaskItem { Id = 2, UserId = 1, Title = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();

        var tasks = await context.Tasks.IgnoreQueryFilters().ToListAsync();

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task TasksQuery_FilterAppliesInsideSubquery()
    {
        await using var context = TestDbContextFactory.Create();
        context.Users.AddRange(
            new User { Id = 1, Name = "Has Active", Email = "active@mail.com" },
            new User { Id = 2, Name = "Deleted Only", Email = "deleted@mail.com" });
        context.Tasks.AddRange(
            new TaskItem { Id = 1, UserId = 1, Title = "Active" },
            new TaskItem { Id = 2, UserId = 2, Title = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();

        var usersWithoutActiveTasks = await context.Users
            .Where(user => !context.Tasks.Any(task => task.UserId == user.Id))
            .Select(user => user.Id)
            .ToListAsync();

        Assert.Equal([2], usersWithoutActiveTasks);
    }
}
