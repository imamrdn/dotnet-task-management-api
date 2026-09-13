namespace TaskManagement.Api.DTOs;

public record TaskWithOwnerResponse(
    int Id,
    string Title,
    string Description,
    bool IsCompleted,
    UserResponse Owner
);
