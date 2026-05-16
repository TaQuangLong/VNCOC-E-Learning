using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.Courses.DeleteCourse;

public class DeleteCourseHandler(AppDbContext db)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var course = await db.Courses.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        course.Status = CourseStatus.Archived;
        course.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
