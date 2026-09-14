namespace TaskManagement.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsCompleted { get; set; }
    public bool IsDeleted { get; set; }
    public string OwnerNameSnapshot { get; set; } = "";
    public string OwnerEmailSnapshot { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<TaskCategory> TaskCategories { get; set; } = new();
}
