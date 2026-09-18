using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record UpsertUserProfileRequest(
    [MaxLength(500, ErrorMessage = "Bio must not exceed 500 characters")]
    string Bio,
    [MaxLength(200, ErrorMessage = "Location must not exceed 200 characters")]
    string Location
);
