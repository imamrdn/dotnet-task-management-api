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
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetUsersAsync(cancellationToken);

        return Ok(ApiResponse<List<UserResponse>>.Ok("Users retrieved successfully", users));
    }

    [HttpGet("without-tasks")]
    public async Task<IActionResult> GetUsersWithoutActiveTasks(CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetUsersWithoutActiveTasksAsync(cancellationToken);

        return Ok(ApiResponse<List<UserResponse>>.Ok("Users without active tasks retrieved successfully", users));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User not found"));
        }

        var user = await _userService.GetUserByIdAsync(id, cancellationToken);

        return user is null
            ? NotFound(ApiResponse<object>.Error("User not found"))
            : Ok(ApiResponse<UserResponse>.Ok("User retrieved successfully", user));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userService.CreateUserAsync(request, cancellationToken);
        return Created($"/api/users/{user.Id}",
            ApiResponse<UserResponse>.Ok("User created successfully", user));
    }

    [HttpGet("{id:int}/profile")]
    public async Task<IActionResult> GetUserProfile(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User profile not found"));
        }

        var profile = await _userService.GetUserProfileAsync(id, cancellationToken);

        return profile is null
            ? NotFound(ApiResponse<object>.Error("User profile not found"))
            : Ok(ApiResponse<UserProfileResponse>.Ok("User profile retrieved successfully", profile));
    }

    [HttpPut("{id:int}/profile")]
    public async Task<IActionResult> UpsertUserProfile(int id, UpsertUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User not found"));
        }

        var profile = await _userService.UpsertUserProfileAsync(id, request, cancellationToken);

        return profile is null
            ? NotFound(ApiResponse<object>.Error("User not found"))
            : Ok(ApiResponse<UserProfileResponse>.Ok("User profile saved successfully", profile));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User not found"));
        }

        var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
        return user is null
            ? NotFound(ApiResponse<object>.Error("User not found"))
            : Ok(ApiResponse<UserResponse>.Ok("User updated successfully", user));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return NotFound(ApiResponse<object>.Error("User not found"));
        }

        var isDeleted = await _userService.DeleteUserAsync(id, cancellationToken);

        return isDeleted ? NoContent() : NotFound(ApiResponse<object>.Error("User not found"));
    }
}
