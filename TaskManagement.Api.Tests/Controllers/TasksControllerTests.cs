using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _service = new();

    [Theory]
    [InlineData(0, 10, "Page must be greater than 0")]
    [InlineData(1, 0, "Limit must be greater than 0")]
    public async Task GetTasks_InvalidPagination_ReturnsBadRequest(int page, int limit, string message)
    {
        var result = await CreateController().GetTasks(page, limit, null, null, null, null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(message, Assert.IsType<ApiResponse<object>>(badRequest.Value).Message);
    }

    [Fact]
    public async Task GetTasks_ValidRequest_ReturnsServiceResponse()
    {
        var response = new PaginatedResponse<TaskResponse>([], 1, 10, 0, 0);
        _service.Setup(service => service.GetTasksAsync(7, 1, 10, "term", true, "title", "desc"))
            .ReturnsAsync(response);

        var result = await CreateController().GetTasks(1, 10, "term", true, "title", "desc");

        Assert.Same(response, Assert.IsType<ApiResponse<PaginatedResponse<TaskResponse>>>(
            Assert.IsType<OkObjectResult>(result).Value).Data);
    }

    [Fact]
    public async Task GetAllTasksWithOwners_ReturnsServiceResponse()
    {
        var response = new List<TaskWithOwnerResponse>
        {
            new(1, "Title", "Description", false, new UserResponse(7, "User", "user@mail.com"))
        };
        _service.Setup(service => service.GetAllTasksWithOwnersAsync()).ReturnsAsync(response);

        var result = await CreateController().GetAllTasksWithOwners();

        Assert.Same(response, Assert.IsType<ApiResponse<List<TaskWithOwnerResponse>>>(
            Assert.IsType<OkObjectResult>(result).Value).Data);
    }

    [Fact]
    public async Task GetTaskSummaryByUser_ReturnsServiceResponse()
    {
        var response = new List<TaskSummaryByUserResponse>
        {
            new(7, "User", "user@mail.com", 3, 2, 1)
        };
        _service.Setup(service => service.GetTaskSummaryByUserAsync(2)).ReturnsAsync(response);

        var result = await CreateController().GetTaskSummaryByUser(2);

        Assert.Same(response, Assert.IsType<ApiResponse<List<TaskSummaryByUserResponse>>>(
            Assert.IsType<OkObjectResult>(result).Value).Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTaskSummaryByUser_InvalidMinimumTasks_ReturnsBadRequest(int minimumTasks)
    {
        var result = await CreateController().GetTaskSummaryByUser(minimumTasks);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Minimum tasks must be greater than 0",
            Assert.IsType<ApiResponse<object>>(badRequest.Value).Message);
    }

    [Fact]
    public async Task GetTopTaskOwners_ReturnsServiceResponse()
    {
        var response = new List<TopTaskOwnerResponse>
        {
            new(7, "User", "user@mail.com", 3)
        };
        _service.Setup(service => service.GetTopTaskOwnersAsync(3)).ReturnsAsync(response);

        var result = await CreateController().GetTopTaskOwners(3);

        Assert.Same(response, Assert.IsType<ApiResponse<List<TopTaskOwnerResponse>>>(
            Assert.IsType<OkObjectResult>(result).Value).Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTopTaskOwners_InvalidLimit_ReturnsBadRequest(int limit)
    {
        var result = await CreateController().GetTopTaskOwners(limit);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Limit must be greater than 0",
            Assert.IsType<ApiResponse<object>>(badRequest.Value).Message);
    }

    [Fact]
    public async Task GetTaskById_ReturnsExpectedStatus()
    {
        var task = new TaskResponse(1, "Title", "Description", false);
        _service.Setup(service => service.GetTaskByIdAsync(7, 1)).ReturnsAsync(task);
        _service.Setup(service => service.GetTaskByIdAsync(7, 2)).ReturnsAsync((TaskResponse?)null);
        var controller = CreateController();

        Assert.IsType<NotFoundObjectResult>(await controller.GetTaskById(0));
        Assert.IsType<OkObjectResult>(await controller.GetTaskById(1));
        Assert.IsType<NotFoundObjectResult>(await controller.GetTaskById(2));
    }

    [Fact]
    public async Task CreateTask_ValidRequest_ReturnsCreated()
    {
        var request = new CreateTaskRequest("Title", "Description");
        var response = new TaskResponse(3, request.Title, request.Description, false);
        _service.Setup(service => service.CreateTaskAsync(7, request)).ReturnsAsync(response);

        var result = Assert.IsType<CreatedResult>(await CreateController().CreateTask(request));

        Assert.Equal("/api/tasks/3", result.Location);
        Assert.Same(response, Assert.IsType<ApiResponse<TaskResponse>>(result.Value).Data);
    }

    [Fact]
    public async Task UpdateTask_CoversNotFoundAndSuccess()
    {
        var controller = CreateController();
        var validRequest = new UpdateTaskRequest("Title", "Description", true);
        var response = new TaskResponse(1, "Title", "Description", true);
        _service.Setup(service => service.UpdateTaskAsync(7, 1, validRequest)).ReturnsAsync(response);

        Assert.IsType<NotFoundObjectResult>(await controller.UpdateTask(0, validRequest));
        Assert.IsType<OkObjectResult>(await controller.UpdateTask(1, validRequest));

        _service.Setup(service => service.UpdateTaskAsync(7, 2, validRequest)).ReturnsAsync((TaskResponse?)null);
        Assert.IsType<NotFoundObjectResult>(await controller.UpdateTask(2, validRequest));
    }

    [Fact]
    public async Task DeleteTask_ReturnsExpectedStatus()
    {
        _service.Setup(service => service.DeleteTaskAsync(7, 1)).ReturnsAsync(true);
        _service.Setup(service => service.DeleteTaskAsync(7, 2)).ReturnsAsync(false);
        var controller = CreateController();

        Assert.IsType<NotFoundObjectResult>(await controller.DeleteTask(0));
        Assert.IsType<NoContentResult>(await controller.DeleteTask(1));
        Assert.IsType<NotFoundObjectResult>(await controller.DeleteTask(2));
    }

    [Fact]
    public async Task GetTasks_MissingUserClaim_ThrowsUnauthorized()
    {
        var controller = CreateController(includeClaim: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            controller.GetTasks(1, 10, null, null, null, null));
    }

    private TasksController CreateController(bool includeClaim = true)
    {
        var claims = includeClaim
            ? new[] { new Claim(ClaimTypes.NameIdentifier, "7") }
            : [];
        var controller = new TasksController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            }
        };

        return controller;
    }
}
