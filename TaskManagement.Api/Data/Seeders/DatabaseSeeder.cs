using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Api.Data.Seeders;

public class DatabaseSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly UserSeeder _userSeeder;
    private readonly TaskSeeder _taskSeeder;

    public DatabaseSeeder(
        AppDbContext dbContext,
        UserSeeder userSeeder,
        TaskSeeder taskSeeder)
    {
        _dbContext = dbContext;
        _userSeeder = userSeeder;
        _taskSeeder = taskSeeder;
    }

    public async Task SeedAsync()
    {
        var admin = await _userSeeder.SeedAdminAsync();
        var user = await _userSeeder.SeedRegularUserAsync();

        await _taskSeeder.SeedAdminTasksAsync(admin);
        await _taskSeeder.SeedUserTasksAsync(user);
    }

    public async Task RefreshAsync()
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        await _dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE tasks, users RESTART IDENTITY CASCADE;");

        await SeedAsync();
        await transaction.CommitAsync();
    }
}
