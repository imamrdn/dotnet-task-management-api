using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.DTOs;

public record UpdateTaskRequest(
    [Required(ErrorMessage = "Title is required")]
    string Title,
    [Required(ErrorMessage = "Description is required")]
    string Description,
    bool IsCompleted
);
