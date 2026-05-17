namespace ChurchLearn.Api.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public int ProgressPercent { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int TotalLessonsCount { get; set; }
    public int? LastAccessedLessonId { get; set; }
    public DateTime? CompletedAt { get; set; }
}
