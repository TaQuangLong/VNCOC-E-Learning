using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.Courses.UnpublishCourse;

public class UnpublishCourseHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var course = await db.Courses.FindAsync([id], cancellationToken);
        if (course is null)
            return Result.Failure($"Course {id} not found.", ErrorCodes.NotFound);

        if (course.Status == CourseStatus.Archived)
            return Result.Failure("An archived course cannot be unpublished.", ErrorCodes.Conflict);

        course.Status = CourseStatus.Draft;
        course.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
