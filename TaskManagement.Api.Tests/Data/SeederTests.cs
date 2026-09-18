using Microsoft.AspNetCore.Identity;
using TaskManagement.Api.Data.Seeders;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Tests.Data;

public class SeederTests
{
    [Fact]
    public async Task UserSeeder_CreatesExpectedUsersAndIsIdempotent()
    {
        await using var context = TestDbContextFactory.Create();
        var seeder = new UserSeeder(context, new PasswordHasher<User>());

        var admin = await seeder.SeedAdminAsync();
        var user = await seeder.SeedRegularUserAsync();
        var existingAdmin = await seeder.SeedAdminAsync();

        Assert.Equal("Admin", admin.Role);
        Assert.Equal("User", user.Role);
        Assert.Equal(admin.Id, existingAdmin.Id);
        Assert.Equal(2, context.Users.Count());
    }

    [Fact]
    public async Task TaskSeeder_CreatesTasksForEachOwnerAndIsIdempotent()
    {
        await using var context = TestDbContextFactory.Create();
        var userSeeder = new UserSeeder(context, new PasswordHasher<User>());
        var admin = await userSeeder.SeedAdminAsync();
        var user = await userSeeder.SeedRegularUserAsync();
        var categories = await new CategorySeeder(context).SeedAsync();
        var seeder = new TaskSeeder(context);

        await seeder.SeedAdminTasksAsync(admin, categories);
        await seeder.SeedUserTasksAsync(user, categories);
        await seeder.SeedAdminTasksAsync(admin, categories);
        await seeder.SeedUserTasksAsync(user, categories);

        Assert.Equal(3, context.Tasks.Count(task => task.UserId == admin.Id));
        Assert.Equal(3, context.Tasks.Count(task => task.UserId == user.Id));
        Assert.Equal(12, context.TaskCategories.Count());
        Assert.All(context.Tasks, task => Assert.True(task.CreatedAt > DateTime.MinValue));
    }

    [Fact]
    public async Task DatabaseSeeder_SeedsUsersAndTasks()
    {
        await using var context = TestDbContextFactory.Create();
        var seeder = new DatabaseSeeder(
            context,
            new UserSeeder(context, new PasswordHasher<User>()),
            new CategorySeeder(context),
            new TaskSeeder(context));

        await seeder.SeedAsync();

        Assert.Equal(2, context.Users.Count());
        Assert.Equal(6, context.Tasks.Count());
        Assert.Equal(3, context.Categories.Count());
        Assert.Equal(12, context.TaskCategories.Count());
    }
}
