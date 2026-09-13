using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext dbContext, ILogger<TaskService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PaginatedResponse<TaskResponse>> GetTasksAsync(
        int userId,
        int page,
        int limit,
        string? search,
        bool? isCompleted,
        string? sortBy,
        string? sortDirection)
    {
        var query = _dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";

            query = query.Where(task =>
                EF.Functions.ILike(task.Title, searchPattern) ||
                EF.Functions.ILike(task.Description, searchPattern));
        }

        if (isCompleted is not null)
        {
            query = query.Where(task => task.IsCompleted == isCompleted);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)limit);
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "title" => isDescending
                ? query.OrderByDescending(task => task.Title)
                : query.OrderBy(task => task.Title),
            "iscompleted" => isDescending
                ? query.OrderByDescending(task => task.IsCompleted)
                : query.OrderBy(task => task.IsCompleted),
            _ => isDescending
                ? query.OrderByDescending(task => task.Id)
                : query.OrderBy(task => task.Id)
        };

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(task => new TaskResponse(
                task.Id,
                task.Title,
                task.Description,
                task.IsCompleted
            ))
            .ToListAsync();

        return new PaginatedResponse<TaskResponse>(
            items,
            page,
            limit,
            totalItems,
            totalPages
        );
    }

    public async Task<List<TaskWithOwnerResponse>> GetAllTasksWithOwnersAsync()
    {
        var tasks = await _dbContext.Tasks
            .AsNoTracking()
            .Include(task => task.User)
            .OrderBy(task => task.Id)
            .ToListAsync();

        return tasks
            .Select(task => new TaskWithOwnerResponse(
                task.Id,
                task.Title,
                task.Description,
                task.IsCompleted,
                new UserResponse(
                    task.User.Id,
                    task.User.Name,
                    task.User.Email
                )))
            .ToList();
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(int userId, int id)
    {
        var task = await _dbContext.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);
        if (task is null)
        {
            return null;
        }

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        );
    }
    
    public async Task<TaskResponse> CreateTaskAsync(int userId, CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            UserId = userId
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, userId);

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        );
    }

    public async Task<TaskResponse?> UpdateTaskAsync(int userId, int id, UpdateTaskRequest request)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);
        if (task is null)
        {
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} updated by user {UserId}", task.Id, userId);

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        );
    }

    public async Task<bool> DeleteTaskAsync(int userId, int id)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);
        if (task is null)
        {
            return false;
        }

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, userId);

        return true;
    }
}
