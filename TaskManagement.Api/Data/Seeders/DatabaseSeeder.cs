using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Api.Data.Seeders;

public class DatabaseSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly UserSeeder _userSeeder;
    private readonly CategorySeeder _categorySeeder;
    private readonly TaskSeeder _taskSeeder;

    public DatabaseSeeder(
        AppDbContext dbContext,
        UserSeeder userSeeder,
        CategorySeeder categorySeeder,
        TaskSeeder taskSeeder)
    {
        _dbContext = dbContext;
        _userSeeder = userSeeder;
        _categorySeeder = categorySeeder;
        _taskSeeder = taskSeeder;
    }

    public async Task SeedAsync()
    {
        var admin = await _userSeeder.SeedAdminAsync();
        var user = await _userSeeder.SeedRegularUserAsync();
        var categories = await _categorySeeder.SeedAsync();

        await _taskSeeder.SeedAdminTasksAsync(admin, categories);
        await _taskSeeder.SeedUserTasksAsync(user, categories);
    }

    public async Task RefreshAsync()
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        await _dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE refresh_tokens, task_categories, tasks, categories, user_profiles, users RESTART IDENTITY CASCADE;");

        await SeedAsync();
        await transaction.CommitAsync();
    }
}
