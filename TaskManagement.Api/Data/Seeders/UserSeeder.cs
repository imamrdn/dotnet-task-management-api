using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data.Seeders;

public class UserSeeder
{
    private readonly AppDbContext _dbContext;

    public UserSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> SeedAdminAsync()
    {
        return await SeedUserAsync(
            name: "Demo Admin",
            email: "admin@mail.com",
            role: "Admin");
    }

    public async Task<User> SeedRegularUserAsync()
    {
        return await SeedUserAsync(
            name: "Demo User",
            email: "user@mail.com",
            role: "User");
    }

    private async Task<User> SeedUserAsync(string name, string email, string role)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == email);

        if (existingUser is not null)
        {
            return existingUser;
        }

        var passwordHasher = new PasswordHasher<User>();

        var user = new User
        {
            Name = name,
            Email = email,
            Role = role
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "secret123");

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }
}
