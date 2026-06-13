using ChurchLearn.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Domain.Entities;

[Index(nameof(Slug), IsUnique = true)]
public class LearningPath
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? EstimatedDurationLabel { get; set; }
    public LearningPathStatus Status { get; set; } = LearningPathStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LearningPathSection> Sections { get; set; } = [];
    public ICollection<LearningPathCourse> Courses { get; set; } = [];
}
