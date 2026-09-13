using TaskManagement.Api.DTOs;

namespace TaskManagement.Api.Services;

public interface ITaskService
{
    Task<PaginatedResponse<TaskResponse>> GetTasksAsync(
        int userId,
        int page,
        int limit,
        string? search,
        bool? isCompleted,
        string? sortBy,
        string? sortDirection);
    Task<List<TaskWithOwnerResponse>> GetAllTasksWithOwnersAsync();
    Task<List<TaskSummaryByUserResponse>> GetTaskSummaryByUserAsync(int? minimumTasks);
    Task<List<TopTaskOwnerResponse>> GetTopTaskOwnersAsync(int limit);
    Task<TaskResponse?> GetTaskByIdAsync(int userId, int id);
    Task<TaskResponse> CreateTaskAsync(int userId, CreateTaskRequest request);
    Task<TaskResponse?> UpdateTaskAsync(int userId, int id, UpdateTaskRequest request);
    Task<bool> DeleteTaskAsync(int userId, int id);
}
