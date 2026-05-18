namespace ChurchLearn.Api.Domain.Entities;

public class LessonProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public int CourseId { get; set; }
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int VideoProgressPercent { get; set; }
    public int VideoWatchedSeconds { get; set; }
    public DateTime? LastWatchedAt { get; set; }
}
