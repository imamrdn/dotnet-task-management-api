using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        bool? isCompleted)
    {
        if (page <= 0)
        {
            return BadRequest("Page must be greater than 0");
        }

        if (limit <= 0)
        {
            return BadRequest("Limit must be greater than 0");
        }

        var userId = GetUserIdFromClaims();
        var taskItems = await _taskService.GetTasksAsync(userId, page, limit, search, isCompleted);

        return Ok(taskItems);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.GetTaskByIdAsync(userId, id);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request)
    {
        var validationResult = ValidateTaskRequest(request.Title, request.Description);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.CreateTaskAsync(userId, request);

        return Created($"/api/tasks/{response.Id}", response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest request)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var validationResult = ValidateTaskRequest(request.Title, request.Description);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var userId = GetUserIdFromClaims();
        var response = await _taskService.UpdateTaskAsync(userId, id, request);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var userId = GetUserIdFromClaims();
        var isDeleted = await _taskService.DeleteTaskAsync(userId, id);

        return isDeleted ? NoContent() : NotFound();
    }

    private IActionResult? ValidateTaskRequest(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest("Title is required");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return BadRequest("Description is required");
        }

        return null;
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
