using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record CreateUserRequest(
    [Required(ErrorMessage = "Name is required")]
    string Name,
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    string Email,
    [Required(ErrorMessage = "Password is required")]
    string Password
);
