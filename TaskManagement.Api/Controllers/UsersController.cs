using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsersAsync();

        return Ok(ApiResponse<List<UserResponse>>.Ok("Users retrieved successfully", users));
    }

    [HttpGet("without-tasks")]
    public async Task<IActionResult> GetUsersWithoutActiveTasks()
    {
        var users = await _userService.GetUsersWithoutActiveTasksAsync();

        return Ok(ApiResponse<List<UserResponse>>.Ok("Users without active tasks retrieved successfully", users));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User not found"));
        }

        var user = await _userService.GetUserByIdAsync(id);

        return user is null
            ? NotFound(ApiResponse<object>.Error("User not found"))
            : Ok(ApiResponse<UserResponse>.Ok("User retrieved successfully", user));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return Created($"/api/users/{user.Id}",
            ApiResponse<UserResponse>.Ok("User created successfully", user));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User not found"));
        }

        var user = await _userService.UpdateUserAsync(id, request);
        return user is null
            ? NotFound(ApiResponse<object>.Error("User not found"))
            : Ok(ApiResponse<UserResponse>.Ok("User updated successfully", user));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User not found"));
        }

        var isDeleted = await _userService.DeleteUserAsync(id);

        return isDeleted ? NoContent() : NotFound(ApiResponse<object>.Error("User not found"));
    }
}
