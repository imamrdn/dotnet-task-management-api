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
        string? sortDirection,
        CancellationToken cancellationToken = default);
    Task<List<TaskWithOwnerResponse>> GetAllTasksWithOwnersAsync(CancellationToken cancellationToken = default);
    Task<List<TaskSummaryByUserResponse>> GetTaskSummaryByUserAsync(int? minimumTasks, CancellationToken cancellationToken = default);
    Task<List<TopTaskOwnerResponse>> GetTopTaskOwnersAsync(int limit, CancellationToken cancellationToken = default);
    Task<TaskResponse?> GetTaskByIdAsync(int userId, int id, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateTaskAsync(int userId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse?> UpdateTaskAsync(int userId, int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse?> UpdateTaskCompletionAsync(int userId, int id, UpdateTaskCompletionRequest request, CancellationToken cancellationToken = default);
    Task<List<CategoryResponse>?> GetTaskCategoriesAsync(int userId, int id, CancellationToken cancellationToken = default);
    Task<List<CategoryResponse>?> AssignTaskCategoriesAsync(int userId, int id, AssignTaskCategoriesRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(int userId, int id, CancellationToken cancellationToken = default);
}
