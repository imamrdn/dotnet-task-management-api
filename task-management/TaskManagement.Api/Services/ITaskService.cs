using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponse>> GetTasksAsync();
    Task<TaskResponse?> GetTaskByIdAsync(int id);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request);
    Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request);
    Task<bool> DeleteTaskAsync(int id);
}
