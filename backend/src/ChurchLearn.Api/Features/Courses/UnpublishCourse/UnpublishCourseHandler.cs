using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.Courses.UnpublishCourse;

public class UnpublishCourseHandler(AppDbContext db)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var course = await db.Courses.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        if (course.Status == CourseStatus.Archived)
            throw new InvalidOperationException("An archived course cannot be unpublished.");

        course.Status = CourseStatus.Draft;
        course.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
