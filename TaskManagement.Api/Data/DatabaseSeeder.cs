using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data;

public class DatabaseSeeder
{
    private readonly AppDbContext _dbContext;

    public DatabaseSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        if (await _dbContext.Users.AnyAsync(user => user.Email == "demo@example.com"))
        {
            return;
        }

        var user = new User
        {
            Name = "Demo User",
            Email = "demo@example.com"
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, "secret123");

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Learn user registration",
                Description = "Create the user registration endpoint",
                IsCompleted = true,
                UserId = user.Id
            },
            new TaskItem
            {
                Title = "Learn user login",
                Description = "Create the login endpoint and JWT token",
                IsCompleted = true,
                UserId = user.Id
            },
            new TaskItem
            {
                Title = "Learn user task ownership",
                Description = "Allow users to access only their own tasks",
                IsCompleted = false,
                UserId = user.Id
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync();
    }
}
