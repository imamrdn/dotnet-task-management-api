using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

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
    public async Task<IActionResult> GetTasks(int page, int limit)
    {
        if (page <= 0)
        {
            return BadRequest("Page must be greater than 0");
        }

        if (limit <= 0)
        {
            return BadRequest("Limit must be greater than 0");
        }

        var taskItems = await _taskService.GetTasksAsync(page, limit);

        return Ok(taskItems);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var response = await _taskService.GetTaskByIdAsync(id);

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

        var response = await _taskService.CreateTaskAsync(request);

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

        var response = await _taskService.UpdateTaskAsync(id, request);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var isDeleted = await _taskService.DeleteTaskAsync(id);

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
}
