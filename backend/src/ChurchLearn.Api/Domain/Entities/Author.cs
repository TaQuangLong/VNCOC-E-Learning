namespace ChurchLearn.Api.Domain.Entities;

public class Author
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Course> Courses { get; set; } = [];
}
