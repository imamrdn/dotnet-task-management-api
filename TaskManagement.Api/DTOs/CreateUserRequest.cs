namespace TaskManagement.Api.DTOs;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password
);
