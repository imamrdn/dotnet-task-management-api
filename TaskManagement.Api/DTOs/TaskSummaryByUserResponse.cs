namespace TaskManagement.Api.DTOs;

public record TaskSummaryByUserResponse(
    int UserId,
    string UserName,
    string Email,
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks
);
