namespace TaskManagement.Api.DTOs;

public record UserResponse(
    int Id,
    string Name,
    string Email
);
