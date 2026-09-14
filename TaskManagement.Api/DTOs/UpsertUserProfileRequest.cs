namespace TaskManagement.Api.DTOs;

public record UpsertUserProfileRequest(
    string Bio,
    string Location
);
