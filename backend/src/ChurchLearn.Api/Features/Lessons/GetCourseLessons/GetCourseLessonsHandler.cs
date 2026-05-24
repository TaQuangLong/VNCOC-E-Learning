using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.GetCourseLessons;

public record LessonSummary(
    int Id,
    string Title,
    string? Description,
    string ContentType,
    int OrderIndex,
    int? DurationMinutes,
    bool IsPreview);

public class GetCourseLessonsHandler(AppDbContext db)
{
    public async Task<Result<List<LessonSummary>>> HandleAsync(int courseId, CancellationToken cancellationToken)
    {
        var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId, cancellationToken);
        if (!courseExists)
            return Result<List<LessonSummary>>.Failure($"Course {courseId} not found.", ErrorCodes.NotFound);

        var lessons = await db.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonSummary(
                l.Id,
                l.Title,
                l.Description,
                l.ContentType.ToString(),
                l.OrderIndex,
                l.DurationMinutes,
                l.IsPreview))
            .ToListAsync(cancellationToken);

        return Result<List<LessonSummary>>.Success(lessons);
    }
}
