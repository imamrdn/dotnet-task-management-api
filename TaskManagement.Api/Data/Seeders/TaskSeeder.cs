using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data.Seeders;

public class TaskSeeder
{
    private readonly AppDbContext _dbContext;

    public TaskSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAdminTasksAsync(User admin)
    {
        var hasTasks = await _dbContext.Tasks.AnyAsync(task => task.UserId == admin.Id);
        if (hasTasks)
        {
            return;
        }

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Review registered users",
                Description = "Inspect the users management endpoint as an admin",
                IsCompleted = true,
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "Manage demo data",
                Description = "Verify seeded users and tasks",
                IsCompleted = true,
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "Prepare role testing",
                Description = "Use this admin account to test protected user endpoints",
                IsCompleted = false,
                UserId = admin.Id
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SeedUserTasksAsync(User user)
    {
        var hasTasks = await _dbContext.Tasks.AnyAsync(task => task.UserId == user.Id);
        if (hasTasks)
        {
            return;
        }

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Create a personal task",
                Description = "Practice creating tasks as a regular user",
                IsCompleted = true,
                UserId = user.Id
            },
            new TaskItem
            {
                Title = "Check own task list",
                Description = "Confirm regular users only see their own tasks",
                IsCompleted = true,
                UserId = user.Id
            },
            new TaskItem
            {
                Title = "Try users endpoint",
                Description = "Confirm regular users receive forbidden access",
                IsCompleted = false,
                UserId = user.Id
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync();
    }
}
