using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace TaskManagement.Api.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required");   
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            throw new ArgumentException("Email is already registered");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }
}
