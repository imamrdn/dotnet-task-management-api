using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Extensions;
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
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskItem> query = _dbContext.Tasks
            .AsNoTracking()
            .WhereActive()
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

        var totalItems = await query.CountAsync(cancellationToken);
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

        // Everything above stays as IQueryable so EF Core can translate it to SQL.
        IQueryable<TaskResponse> responseQuery = query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(task => new TaskResponse(
                task.Id,
                task.Title,
                task.Description,
                task.IsCompleted
            ));

        // ToListAsync materializes the query; the result is now an in-memory IEnumerable.
        IEnumerable<TaskResponse> items = await responseQuery.ToListAsync(cancellationToken);

        return new PaginatedResponse<TaskResponse>(
            items.ToList(),
            page,
            limit,
            totalItems,
            totalPages
        );
    }

    public async Task<List<TaskWithOwnerResponse>> GetAllTasksWithOwnersAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _dbContext.Tasks
            .AsNoTracking()
            .WhereActive()
            .Include(task => task.User)
            .OrderBy(task => task.Id)
            .ToListAsync(cancellationToken);

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

    public async Task<List<TaskSummaryByUserResponse>> GetTaskSummaryByUserAsync(
        int? minimumTasks,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tasks
            .AsNoTracking()
            .WhereActive()
            .GroupBy(task => new
            {
                task.UserId,
                task.User.Name,
                task.User.Email
            });

        if (minimumTasks is not null)
        {
            query = query.Where(group => group.Count() >= minimumTasks);
        }

        return await query
            .OrderBy(group => group.Key.UserId)
            .Select(group => new TaskSummaryByUserResponse(
                group.Key.UserId,
                group.Key.Name,
                group.Key.Email,
                group.Count(),
                group.Count(task => task.IsCompleted),
                group.Count(task => !task.IsCompleted)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TopTaskOwnerResponse>> GetTopTaskOwnersAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .AsNoTracking()
            .WhereActive()
            .GroupBy(task => new
            {
                task.UserId,
                task.User.Name,
                task.User.Email
            })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.UserId)
            .Take(limit)
            .Select(group => new TopTaskOwnerResponse(
                group.Key.UserId,
                group.Key.Name,
                group.Key.Email,
                group.Count()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(
        int userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .AsNoTracking()
            .WhereActive()
            .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId, cancellationToken);
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
    
    public async Task<TaskResponse> CreateTaskAsync(
        int userId,
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var owner = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
        if (owner is null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            OwnerNameSnapshot = owner.Name,
            OwnerEmailSnapshot = owner.Email,
            UserId = userId
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, userId);

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        );
    }

    public async Task<TaskResponse?> UpdateTaskAsync(
        int userId,
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .WhereActive()
            .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Task {TaskId} updated by user {UserId}", task.Id, userId);

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        );
    }

    public async Task<bool> DeleteTaskAsync(
        int userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .WhereActive()
            .FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId, cancellationToken);
        if (task is null)
        {
            return false;
        }

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, userId);

        return true;
    }
}
