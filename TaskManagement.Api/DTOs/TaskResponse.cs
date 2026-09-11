namespace TaskManagement.Api.DTOs;

public record TaskResponse(
    int Id,
    string Title,
    string Description,
    bool IsCompleted
);
