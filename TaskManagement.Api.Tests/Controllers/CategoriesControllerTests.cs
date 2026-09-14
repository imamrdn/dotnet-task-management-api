using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _service = new();

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        _service.Setup(service => service.GetCategoriesAsync(default)).ReturnsAsync([]);

        Assert.IsType<OkObjectResult>(await CreateController().GetCategories());
    }

    [Fact]
    public async Task GetCategoryById_ReturnsExpectedStatus()
    {
        _service.Setup(service => service.GetCategoryByIdAsync(1, default))
            .ReturnsAsync(new CategoryResponse(1, "Backend"));

        Assert.IsType<NotFoundObjectResult>(await CreateController().GetCategoryById(0));
        Assert.IsType<OkObjectResult>(await CreateController().GetCategoryById(1));
        Assert.IsType<NotFoundObjectResult>(await CreateController().GetCategoryById(2));
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreated()
    {
        var request = new CreateCategoryRequest("Backend");
        _service.Setup(service => service.CreateCategoryAsync(request, default))
            .ReturnsAsync(new CategoryResponse(1, request.Name));

        var result = Assert.IsType<CreatedResult>(await CreateController().CreateCategory(request));

        Assert.Equal("/api/categories/1", result.Location);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsExpectedStatus()
    {
        var request = new UpdateCategoryRequest("Backend");
        _service.Setup(service => service.UpdateCategoryAsync(1, request, default))
            .ReturnsAsync(new CategoryResponse(1, request.Name));

        Assert.IsType<NotFoundObjectResult>(await CreateController().UpdateCategory(0, request));
        Assert.IsType<OkObjectResult>(await CreateController().UpdateCategory(1, request));
        Assert.IsType<NotFoundObjectResult>(await CreateController().UpdateCategory(2, request));
    }

    [Fact]
    public async Task DeleteCategory_ReturnsExpectedStatus()
    {
        _service.Setup(service => service.DeleteCategoryAsync(1, default)).ReturnsAsync(true);
        _service.Setup(service => service.DeleteCategoryAsync(2, default)).ReturnsAsync(false);

        Assert.IsType<NotFoundObjectResult>(await CreateController().DeleteCategory(0));
        Assert.IsType<NoContentResult>(await CreateController().DeleteCategory(1));
        Assert.IsType<NotFoundObjectResult>(await CreateController().DeleteCategory(2));
    }

    private CategoriesController CreateController() => new(_service.Object);
}
