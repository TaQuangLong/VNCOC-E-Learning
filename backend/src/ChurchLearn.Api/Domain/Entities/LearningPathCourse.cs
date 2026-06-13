using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Domain.Entities;

[Index(nameof(LearningPathId), nameof(CourseId), IsUnique = true)]
[Index(nameof(LearningPathSectionId), nameof(OrderIndex), IsUnique = true)]
[Index(nameof(CourseId))]
public class LearningPathCourse
{
    public int Id { get; set; }
    public int LearningPathId { get; set; }
    public LearningPath LearningPath { get; set; } = null!;
    public int LearningPathSectionId { get; set; }
    public LearningPathSection LearningPathSection { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int OrderIndex { get; set; }
}
