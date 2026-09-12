namespace TaskManagement.Api.DTOs;

public record UpdateUserRequest(
    string Name,
    string Email,
    string? Password
);
