using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.Courses.DeleteCourse;

public class DeleteCourseHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var course = await db.Courses.FindAsync([id], cancellationToken);
        if (course is null)
            return Result.Failure($"Course {id} not found.", ErrorCodes.NotFound);

        course.Status = CourseStatus.Archived;
        course.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
