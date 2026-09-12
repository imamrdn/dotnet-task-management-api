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
        string? sortDirection)
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
        var taskItems = await _taskService.GetTasksAsync(userId, page, limit, search, isCompleted, sortBy, sortDirection);

        return Ok(ApiResponse<PaginatedResponse<TaskResponse>>.Ok("Tasks retrieved successfully", taskItems));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Task not found"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.GetTaskByIdAsync(userId, id);

        return response is null
            ? NotFound(ApiResponse<object>.Error("Task not found"))
            : Ok(ApiResponse<TaskResponse>.Ok("Task retrieved successfully", response));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request)
    {
        var userId = GetUserIdFromClaims();
        var response = await _taskService.CreateTaskAsync(userId, request);

        return Created($"/api/tasks/{response.Id}",
            ApiResponse<TaskResponse>.Ok("Task created successfully", response));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest request)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Task not found"));
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.UpdateTaskAsync(userId, id, request);

        return response is null
            ? NotFound(ApiResponse<object>.Error("Task not found"))
            : Ok(ApiResponse<TaskResponse>.Ok("Task updated successfully", response));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("Task not found"));
        }

        var userId = GetUserIdFromClaims();
        var isDeleted = await _taskService.DeleteTaskAsync(userId, id);

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
