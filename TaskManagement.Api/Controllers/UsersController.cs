using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Extensions;
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

        return this.Reply(users, "Users retrieved successfully");
    }

    [HttpGet("without-tasks")]
    public async Task<IActionResult> GetUsersWithoutActiveTasks(CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetUsersWithoutActiveTasksAsync(cancellationToken);

        return this.Reply(users, "Users without active tasks retrieved successfully");
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid user id"));
        }

        var user = await _userService.GetUserByIdAsync(id, cancellationToken);

        return this.Reply(user, "User retrieved successfully", "User not found");
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
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid user id"));
        }

        var profile = await _userService.GetUserProfileAsync(id, cancellationToken);

        return this.Reply(profile, "User profile retrieved successfully", "User profile not found");
    }

    [HttpPut("{id:int}/profile")]
    public async Task<IActionResult> UpsertUserProfile(int id, UpsertUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid user id"));
        }

        var profile = await _userService.UpsertUserProfileAsync(id, request, cancellationToken);

        return this.Reply(profile, "User profile saved successfully", "User not found");
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid user id"));
        }

        var user = await _userService.UpdateUserAsync(id, request, cancellationToken);

        return this.Reply(user, "User updated successfully", "User not found");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken = default)
    {
        if (!id.IsValidId())
        {
            return BadRequest(ApiResponse<object>.Error("Invalid user id"));
        }

        var isDeleted = await _userService.DeleteUserAsync(id, cancellationToken);

        return this.Reply(isDeleted, "User not found");
    }
}
