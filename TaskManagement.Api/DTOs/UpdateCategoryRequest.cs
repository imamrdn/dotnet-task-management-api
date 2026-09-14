using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record UpdateCategoryRequest(
    [Required(ErrorMessage = "Name is required")]
    string Name
);
