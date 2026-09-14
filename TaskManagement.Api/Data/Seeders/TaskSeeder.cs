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

    public async Task SeedAdminTasksAsync(User admin, IReadOnlyList<Category> categories)
    {
        var hasTasks = await _dbContext.Tasks.AnyAsync(task => task.UserId == admin.Id);
        if (hasTasks)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Review registered users",
                Description = "Inspect the users management endpoint as an admin",
                IsCompleted = true,
                CreatedAt = now,
                OwnerNameSnapshot = admin.Name,
                OwnerEmailSnapshot = admin.Email,
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "Manage demo data",
                Description = "Verify seeded users and tasks",
                IsCompleted = true,
                CreatedAt = now,
                OwnerNameSnapshot = admin.Name,
                OwnerEmailSnapshot = admin.Email,
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "Prepare role testing",
                Description = "Use this admin account to test protected user endpoints",
                IsCompleted = false,
                CreatedAt = now,
                OwnerNameSnapshot = admin.Name,
                OwnerEmailSnapshot = admin.Email,
                UserId = admin.Id
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync();
        await AssignCategoriesAsync(tasks, categories);
    }

    public async Task SeedUserTasksAsync(User user, IReadOnlyList<Category> categories)
    {
        var hasTasks = await _dbContext.Tasks.AnyAsync(task => task.UserId == user.Id);
        if (hasTasks)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Create a personal task",
                Description = "Practice creating tasks as a regular user",
                IsCompleted = true,
                CreatedAt = now,
                OwnerNameSnapshot = user.Name,
                OwnerEmailSnapshot = user.Email,
                UserId = user.Id
            },
            new TaskItem
            {
                Title = "Check own task list",
                Description = "Confirm regular users only see their own tasks",
                IsCompleted = true,
                CreatedAt = now,
                OwnerNameSnapshot = user.Name,
                OwnerEmailSnapshot = user.Email,
                UserId = user.Id
            },
            new TaskItem
            {
                Title = "Try users endpoint",
                Description = "Confirm regular users receive forbidden access",
                IsCompleted = false,
                CreatedAt = now,
                OwnerNameSnapshot = user.Name,
                OwnerEmailSnapshot = user.Email,
                UserId = user.Id
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync();
        await AssignCategoriesAsync(tasks, categories);
    }

    private async Task AssignCategoriesAsync(IReadOnlyList<TaskItem> tasks, IReadOnlyList<Category> categories)
    {
        if (categories.Count == 0)
        {
            return;
        }

        foreach (var task in tasks)
        {
            foreach (var category in categories.Take(2))
            {
                _dbContext.TaskCategories.Add(new TaskCategory
                {
                    TaskItemId = task.Id,
                    CategoryId = category.Id
                });
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
