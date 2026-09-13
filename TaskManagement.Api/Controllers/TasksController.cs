using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        int page,
        int limit,
        string? search,
        bool? isCompleted,
        string? sortBy,
        string? sortDirection,
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

        var userId = GetUserIdFromClaims();
        var taskItems = await _taskService.GetTasksAsync(userId, page, limit, search, isCompleted, sortBy, sortDirection, cancellationToken);

        return Ok(ApiResponse<PaginatedResponse<TaskResponse>>.Ok("Tasks retrieved successfully", taskItems));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllTasksWithOwners(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskService.GetAllTasksWithOwnersAsync(cancellationToken);

        return Ok(ApiResponse<List<TaskWithOwnerResponse>>.Ok("Tasks with owners retrieved successfully", tasks));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/summary")]
    public async Task<IActionResult> GetTaskSummaryByUser(int? minimumTasks, CancellationToken cancellationToken = default)
    {
        if (minimumTasks <= 0)
        {
            return BadRequest(ApiResponse<object>.Error("Minimum tasks must be greater than 0"));
        }

        var summaries = await _taskService.GetTaskSummaryByUserAsync(minimumTasks, cancellationToken);

        return Ok(ApiResponse<List<TaskSummaryByUserResponse>>.Ok("Task summary retrieved successfully", summaries));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/top-users")]
    public async Task<IActionResult> GetTopTaskOwners(int limit = 5, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            return BadRequest(ApiResponse<object>.Error("Limit must be greater than 0"));
        }

        var owners = await _taskService.GetTopTaskOwnersAsync(limit, cancellationToken);

        return Ok(ApiResponse<List<TopTaskOwnerResponse>>.Ok("Top task owners retrieved successfully", owners));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTaskById(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Task not found"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.GetTaskByIdAsync(userId, id, cancellationToken);

        return response is null
            ? NotFound(ApiResponse<object>.Error("Task not found"))
            : Ok(ApiResponse<TaskResponse>.Ok("Task retrieved successfully", response));
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
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Task not found"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.UpdateTaskAsync(userId, id, request, cancellationToken);

        return response is null
            ? NotFound(ApiResponse<object>.Error("Task not found"))
            : Ok(ApiResponse<TaskResponse>.Ok("Task updated successfully", response));
    }

    [HttpPatch("{id:int}/completion")]
    public async Task<IActionResult> UpdateTaskCompletion(
        int id,
        UpdateTaskCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Task not found"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.UpdateTaskCompletionAsync(userId, id, request, cancellationToken);

        return response is null
            ? NotFound(ApiResponse<object>.Error("Task not found"))
            : Ok(ApiResponse<TaskResponse>.Ok("Task completion updated successfully", response));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Task not found"));
        }

        var userId = GetUserIdFromClaims();
        var isDeleted = await _taskService.DeleteTaskAsync(userId, id, cancellationToken);

        return isDeleted ? NoContent() : NotFound(ApiResponse<object>.Error("Task not found"));
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User ID claim not found");
        }

        return int.Parse(userIdClaim.Value);
    }
}
