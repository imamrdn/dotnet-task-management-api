namespace TaskManagement.Api.DTOs;

public record UserProfileResponse(
    int Id,
    int UserId,
    string Bio,
    string Location
);
