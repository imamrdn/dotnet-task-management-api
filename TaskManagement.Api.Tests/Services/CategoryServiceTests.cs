using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Errors;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategoriesOrderedByName()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Database" },
            new Category { Id = 2, Name = "Backend" });
        await context.SaveChangesAsync();

        var result = await CreateService(context).GetCategoriesAsync();

        Assert.Equal(["Backend", "Database"], result.Select(category => category.Name));
    }

    [Fact]
    public async Task CreateCategoryAsync_ValidatesAndCreatesCategory()
    {
        await using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        var created = await service.CreateCategoryAsync(new CreateCategoryRequest("Backend"));

        Assert.Equal("Backend", created.Name);
        Assert.Equal("Backend", Assert.Single(context.Categories).Name);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateCategoryAsync(new CreateCategoryRequest("")));
        await Assert.ThrowsAsync<DuplicateResourceException>(() =>
            service.CreateCategoryAsync(new CreateCategoryRequest("Backend")));
    }

    [Fact]
    public async Task UpdateCategoryAsync_UpdatesCategoryOrReturnsNull()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Backend" },
            new Category { Id = 2, Name = "Database" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var updated = await service.UpdateCategoryAsync(1, new UpdateCategoryRequest("API"));

        Assert.Equal("API", updated!.Name);
        Assert.Null(await service.UpdateCategoryAsync(99, new UpdateCategoryRequest("Missing")));
        await Assert.ThrowsAsync<DuplicateResourceException>(() =>
            service.UpdateCategoryAsync(1, new UpdateCategoryRequest("Database")));
    }

    [Fact]
    public async Task DeleteCategoryAsync_ReturnsWhetherCategoryWasDeleted()
    {
        await using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { Id = 1, Name = "Backend" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        Assert.True(await service.DeleteCategoryAsync(1));
        Assert.False(await service.DeleteCategoryAsync(99));
    }

    [Fact]
    public async Task WriteOperations_LogCategoryIds()
    {
        await using var context = TestDbContextFactory.Create();
        var logger = new TestLogger<CategoryService>();
        var service = new CategoryService(context, logger);

        var created = await service.CreateCategoryAsync(new CreateCategoryRequest("Backend"));
        await service.UpdateCategoryAsync(created.Id, new UpdateCategoryRequest("API"));
        await service.DeleteCategoryAsync(created.Id);

        Assert.Equal(3, logger.Entries.Count);
        Assert.All(logger.Entries, entry => Assert.Equal(LogLevel.Information, entry.Level));
        Assert.All(logger.Entries, entry => Assert.Contains($"Category {created.Id}", entry.Message));
    }

    private static CategoryService CreateService(AppDbContext context) =>
        new(context, NullLogger<CategoryService>.Instance);
}
