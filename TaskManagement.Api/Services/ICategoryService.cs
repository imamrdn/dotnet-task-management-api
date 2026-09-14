using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryResponse?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
