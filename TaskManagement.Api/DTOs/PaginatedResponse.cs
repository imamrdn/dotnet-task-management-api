namespace TaskManagement.Api.DTOs;

public record PaginatedResponse<T>(
    List<T> Items,
    int Page,
    int Limit,
    int TotalItems,
    int TotalPages
);
