namespace ChurchLearn.Api.Domain.Entities;

public class Discussion
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public int? ParentDiscussionId { get; set; }
    public Discussion? ParentDiscussion { get; set; }
    public ICollection<Discussion> Replies { get; set; } = new List<Discussion>();
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
