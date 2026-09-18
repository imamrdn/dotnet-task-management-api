using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Extensions;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        int page = 1,
        int limit = 10,
        string? search = null,
        bool? isCompleted = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            return BadRequest(ApiResponse<object>.Error("Page must be greater than 0"));
        }

        if (limit <= 0)
        {
            return BadRequest(ApiResponse<object>.Error("Limit must be greater than 0"));
        }

        if (limit > MaxPageSize)
        {
            return BadRequest(ApiResponse<object>.Error($"Limit must not exceed {MaxPageSize}"));
        }

        var userId = GetUserIdFromClaims();
        var taskItems = await _taskService.GetTasksAsync(userId, page, limit, search, isCompleted, sortBy, sortDirection, cancellationToken);

        return this.Reply(taskItems, "Tasks retrieved successfully");
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllTasksWithOwners(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskService.GetAllTasksWithOwnersAsync(cancellationToken);

        return this.Reply(tasks, "Tasks with owners retrieved successfully");
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin/summary")]
    public async Task<IActionResult> GetTaskSummaryByUser(int? minimumTasks, CancellationToken cancellationToken = default)
    {
        if (minimumTasks <= 0)
        {
            return BadRequest(ApiResponse<object>.Error("Minimum tasks must be greater than 0"));
        }

        var summaries = await _taskService.GetTaskSummaryByUserAsync(minimumTasks, cancellationToken);

        return this.Reply(summaries, "Task summary retrieved successfully");
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin/top-users")]
    public async Task<IActionResult> GetTopTaskOwners(int limit = 5, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            return BadRequest(ApiResponse<object>.Error("Limit must be greater than 0"));
        }

        var owners = await _taskService.GetTopTaskOwnersAsync(limit, cancellationToken);

        return this.Reply(owners, "Top task owners retrieved successfully");
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTaskById(int id, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid task id"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.GetTaskByIdAsync(userId, id, cancellationToken);

        return this.Reply(response, "Task retrieved successfully", "Task not found");
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromClaims();
        var response = await _taskService.CreateTaskAsync(userId, request, cancellationToken);

        return Created($"/api/tasks/{response.Id}",
            ApiResponse<TaskResponse>.Ok("Task created successfully", response));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid task id"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.UpdateTaskAsync(userId, id, request, cancellationToken);

        return this.Reply(response, "Task updated successfully", "Task not found");
    }

    [HttpPatch("{id:int}/completion")]
    public async Task<IActionResult> UpdateTaskCompletion(
        int id,
        UpdateTaskCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid task id"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.UpdateTaskCompletionAsync(userId, id, request, cancellationToken);

        return this.Reply(response, "Task completion updated successfully", "Task not found");
    }

    [HttpGet("{id:int}/categories")]
    public async Task<IActionResult> GetTaskCategories(int id, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid task id"));
        }

        var userId = GetUserIdFromClaims();
        var categories = await _taskService.GetTaskCategoriesAsync(userId, id, cancellationToken);

        return this.Reply(categories, "Task categories retrieved successfully", "Task not found");
    }

    [HttpPut("{id:int}/categories")]
    public async Task<IActionResult> AssignTaskCategories(
        int id,
        AssignTaskCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid task id"));
        }

        var userId = GetUserIdFromClaims();
        var categories = await _taskService.AssignTaskCategoriesAsync(userId, id, request, cancellationToken);

        return this.Reply(categories, "Task categories updated successfully", "Task not found");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid task id"));
        }

        var userId = GetUserIdFromClaims();
        var isDeleted = await _taskService.DeleteTaskAsync(userId, id, cancellationToken);

        return this.Reply(isDeleted, "Task not found");
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim is invalid");
        }

        return userId;
    }
}
