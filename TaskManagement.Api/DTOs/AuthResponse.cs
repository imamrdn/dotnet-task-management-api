namespace TaskManagement.Api.DTOs;

public record AuthResponse(
    string Token,
    string RefreshToken
);
