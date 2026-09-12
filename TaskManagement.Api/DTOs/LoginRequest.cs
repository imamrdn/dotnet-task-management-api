using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record LoginRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    string Email,
    [Required(ErrorMessage = "Password is required")]
    string Password
);
