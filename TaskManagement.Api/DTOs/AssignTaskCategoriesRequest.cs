using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record AssignTaskCategoriesRequest(
    [Required(ErrorMessage = "CategoryIds is required")]
    List<int> CategoryIds
);
