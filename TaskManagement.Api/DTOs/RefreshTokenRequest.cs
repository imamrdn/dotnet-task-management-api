using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record RefreshTokenRequest(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken
);
