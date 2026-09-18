using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Errors;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<UserResponse>> GetUsersAsync()
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .Select(user => new UserResponse(
                user.Id,
                user.Name,
                user.Email
            ))
            .ToListAsync();
    }

    public async Task<List<UserResponse>> GetUsersWithoutActiveTasksAsync()
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => !_dbContext.Tasks.Any(task =>
                task.UserId == user.Id &&
                !task.IsDeleted))
            .OrderBy(user => user.Id)
            .Select(user => new UserResponse(
                user.Id,
                user.Name,
                user.Email
            ))
            .ToListAsync();
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return null;
        }

        return new UserResponse(user.Id, user.Name, user.Email);
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(int userId)
    {
        return await _dbContext.UserProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new UserProfileResponse(
                profile.Id,
                profile.UserId,
                profile.Bio,
                profile.Location
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<UserProfileResponse?> UpsertUserProfileAsync(
        int userId,
        UpsertUserProfileRequest request)
    {
        var userExists = await _dbContext.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
        {
            return null;
        }

        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (profile is null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                Bio = request.Bio,
                Location = request.Location,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.UserProfiles.Add(profile);
        }
        else
        {
            profile.Bio = request.Bio;
            profile.Location = request.Location;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Profile for user {UserId} upserted", userId);

        return new UserProfileResponse(
            profile.Id,
            profile.UserId,
            profile.Bio,
            profile.Location
        );
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        ValidateUserRequest(request.Name, request.Email);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        var emailExists = await _dbContext.Users.AnyAsync(user => user.Email == request.Email);
        if (emailExists)
        {
            throw new DuplicateResourceException("Email is already registered");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User {UserId} created", user.Id);

        return new UserResponse(user.Id, user.Name, user.Email);
    }

    public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        ValidateUserRequest(request.Name, request.Email);

        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return null;
        }

        var emailExists = await _dbContext.Users
            .AnyAsync(existingUser => existingUser.Email == request.Email && existingUser.Id != id);

        if (emailExists)
        {
            throw new DuplicateResourceException("Email is already registered");
        }

        user.Name = request.Name;
        user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User {UserId} updated", user.Id);

        return new UserResponse(user.Id, user.Name, user.Email);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User {UserId} deleted", id);

        return true;
    }

    private static void ValidateUserRequest(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required");
        }
    }
}
