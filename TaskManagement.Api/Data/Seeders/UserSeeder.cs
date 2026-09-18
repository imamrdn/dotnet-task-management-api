using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data.Seeders;

public class UserSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserSeeder(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
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
            .Include(user => user.Profile)
            .FirstOrDefaultAsync(user => user.Email == email);

        if (existingUser is not null)
        {
            await EnsureProfileAsync(existingUser);
            return existingUser;
        }

        var user = new User
        {
            Name = name,
            Email = email,
            Role = role
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, "secret123");

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        await EnsureProfileAsync(user);

        return user;
    }

    private async Task EnsureProfileAsync(User user)
    {
        var hasProfile = user.Profile is not null ||
            await _dbContext.UserProfiles.AnyAsync(profile => profile.UserId == user.Id);
        if (hasProfile)
        {
            return;
        }

        _dbContext.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            Bio = $"{user.Role} account for Task Management learning",
            Location = "Learning Workspace",
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }
}
