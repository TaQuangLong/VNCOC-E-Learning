using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Domain.Entities;

[Index(nameof(LearningPathId), nameof(OrderIndex), IsUnique = true)]
public class LearningPathSection
{
    public int Id { get; set; }
    public int LearningPathId { get; set; }
    public LearningPath LearningPath { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }

    public ICollection<LearningPathCourse> Courses { get; set; } = [];
}
