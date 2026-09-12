using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponse>> GetTasksAsync(int userId, int page, int limit);
    Task<TaskResponse?> GetTaskByIdAsync(int userId, int id);
    Task<TaskResponse> CreateTaskAsync(int userId, CreateTaskRequest request);
    Task<TaskResponse?> UpdateTaskAsync(int userId, int id, UpdateTaskRequest request);
    Task<bool> DeleteTaskAsync(int userId, int id);
}
