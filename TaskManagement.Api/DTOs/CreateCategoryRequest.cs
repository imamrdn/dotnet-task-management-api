using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record CreateCategoryRequest(
    [Required(ErrorMessage = "Name is required")]
    string Name
);
