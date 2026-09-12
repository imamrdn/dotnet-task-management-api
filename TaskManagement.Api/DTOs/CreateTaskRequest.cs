using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record CreateTaskRequest(
    [Required(ErrorMessage = "Title is required")]
    string Title,
    [Required(ErrorMessage = "Description is required")]
    string Description
);
