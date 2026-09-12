using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserResponse>> GetUsersAsync()
    {
        return await _dbContext.Users
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
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (user is null)
        {
            return null;
        }

        return new UserResponse(user.Id, user.Name, user.Email);
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
            throw new InvalidOperationException("Email is already registered");
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

        return new UserResponse(user.Id, user.Name, user.Email);
    }

    public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        ValidateUserRequest(request.Name, request.Email);

        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (user is null)
        {
            return null;
        }

        var emailExists = await _dbContext.Users
            .AnyAsync(existingUser => existingUser.Email == request.Email && existingUser.Id != id);

        if (emailExists)
        {
            throw new InvalidOperationException("Email is already registered");
        }

        user.Name = request.Name;
        user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        }

        await _dbContext.SaveChangesAsync();

        return new UserResponse(user.Id, user.Name, user.Email);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (user is null)
        {
            return false;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

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
