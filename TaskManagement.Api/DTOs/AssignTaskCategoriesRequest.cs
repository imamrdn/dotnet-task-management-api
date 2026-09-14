namespace TaskManagement.Api.DTOs;

public record AssignTaskCategoriesRequest(
    List<int> CategoryIds
);
