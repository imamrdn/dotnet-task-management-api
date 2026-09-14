using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryService.GetCategoriesAsync(cancellationToken);

        return Ok(ApiResponse<List<CategoryResponse>>.Ok("Categories retrieved successfully", categories));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Category not found"));
        }

        var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);

        return category is null
            ? NotFound(ApiResponse<object>.Error("Category not found"))
            : Ok(ApiResponse<CategoryResponse>.Ok("Category retrieved successfully", category));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryService.CreateCategoryAsync(request, cancellationToken);

        return Created($"/api/categories/{category.Id}",
            ApiResponse<CategoryResponse>.Ok("Category created successfully", category));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Category not found"));
        }

        var category = await _categoryService.UpdateCategoryAsync(id, request, cancellationToken);

        return category is null
            ? NotFound(ApiResponse<object>.Error("Category not found"))
            : Ok(ApiResponse<CategoryResponse>.Ok("Category updated successfully", category));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Category not found"));
        }

        var isDeleted = await _categoryService.DeleteCategoryAsync(id, cancellationToken);

        return isDeleted ? NoContent() : NotFound(ApiResponse<object>.Error("Category not found"));
    }
}
