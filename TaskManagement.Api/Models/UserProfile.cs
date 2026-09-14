namespace TaskManagement.Api.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
