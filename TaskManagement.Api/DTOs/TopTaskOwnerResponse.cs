namespace TaskManagement.Api.DTOs;

public record TopTaskOwnerResponse(
    int UserId,
    string UserName,
    string Email,
    int TotalTasks
);
