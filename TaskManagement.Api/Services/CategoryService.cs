using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext dbContext, ILogger<CategoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponse(category.Id, category.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new CategoryResponse(category.Id, category.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateName(request.Name);

        var nameExists = await _dbContext.Categories
            .AnyAsync(category => category.Name == request.Name, cancellationToken);
        if (nameExists)
        {
            throw new InvalidOperationException("Category name is already registered");
        }

        var category = new Category { Name = request.Name };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {CategoryId} created", category.Id);

        return new CategoryResponse(category.Id, category.Name);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateName(request.Name);

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        if (category is null)
        {
            return null;
        }

        var nameExists = await _dbContext.Categories.AnyAsync(
            existingCategory => existingCategory.Name == request.Name && existingCategory.Id != id,
            cancellationToken);
        if (nameExists)
        {
            throw new InvalidOperationException("Category name is already registered");
        }

        category.Name = request.Name;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {CategoryId} updated", category.Id);

        return new CategoryResponse(category.Id, category.Name);
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {CategoryId} deleted", id);

        return true;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required");
        }
    }
}
