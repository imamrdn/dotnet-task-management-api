using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record LogoutRequest(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken
);
