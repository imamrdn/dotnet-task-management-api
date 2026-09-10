namespace TaskManagement.Api.DTOs;

public record CreateTaskRequest(
    string Title,
    string Description
);
