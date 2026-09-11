using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TaskResponse>> GetTasksAsync()
    {
        return await _dbContext.Tasks
            .Select(task => new TaskResponse(
                task.Id,
                task.Title,
                task.Description,
                task.IsCompleted
            ))
            .ToListAsync();
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(int id)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == id);
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
    
    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        );
    }

    public async Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == id);
        if (task is null)
        {
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;

        await _dbContext.SaveChangesAsync();

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted
        );
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == id);
        if (task is null)
        {
            return false;
        }

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
