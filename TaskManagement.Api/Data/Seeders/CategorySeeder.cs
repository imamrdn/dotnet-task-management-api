using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data.Seeders;

public class CategorySeeder
{
    private readonly AppDbContext _dbContext;

    public CategorySeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Category>> SeedAsync()
    {
        var categoryNames = new[] { "Backend", "Database", "Security" };

        foreach (var categoryName in categoryNames)
        {
            var exists = await _dbContext.Categories
                .AnyAsync(category => category.Name == categoryName);
            if (!exists)
            {
                _dbContext.Categories.Add(new Category { Name = categoryName });
            }
        }

        await _dbContext.SaveChangesAsync();

        return await _dbContext.Categories
            .Where(category => categoryNames.Contains(category.Name))
            .OrderBy(category => category.Id)
            .ToListAsync();
    }
}
