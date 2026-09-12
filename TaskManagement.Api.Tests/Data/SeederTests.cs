using TaskManagement.Api.Data.Seeders;

namespace TaskManagement.Api.Tests.Data;

public class SeederTests
{
    [Fact]
    public async Task UserSeeder_CreatesExpectedUsersAndIsIdempotent()
    {
        await using var context = TestDbContextFactory.Create();
        var seeder = new UserSeeder(context);

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
        var userSeeder = new UserSeeder(context);
        var admin = await userSeeder.SeedAdminAsync();
        var user = await userSeeder.SeedRegularUserAsync();
        var seeder = new TaskSeeder(context);

        await seeder.SeedAdminTasksAsync(admin);
        await seeder.SeedUserTasksAsync(user);
        await seeder.SeedAdminTasksAsync(admin);
        await seeder.SeedUserTasksAsync(user);

        Assert.Equal(3, context.Tasks.Count(task => task.UserId == admin.Id));
        Assert.Equal(3, context.Tasks.Count(task => task.UserId == user.Id));
    }

    [Fact]
    public async Task DatabaseSeeder_SeedsUsersAndTasks()
    {
        await using var context = TestDbContextFactory.Create();
        var seeder = new DatabaseSeeder(
            context,
            new UserSeeder(context),
            new TaskSeeder(context));

        await seeder.SeedAsync();

        Assert.Equal(2, context.Users.Count());
        Assert.Equal(6, context.Tasks.Count());
    }
}
