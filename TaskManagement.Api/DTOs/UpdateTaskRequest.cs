namespace TaskManagement.Api.DTOs;

public record UpdateTaskRequest(
    string Title,
    string Description,
    bool IsCompleted
);
