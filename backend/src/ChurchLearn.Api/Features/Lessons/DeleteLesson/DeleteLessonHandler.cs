using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.DeleteLesson;

public class DeleteLessonHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync([id], cancellationToken);
        if (lesson is null)
            return Result.Failure($"Lesson {id} not found.", ErrorCodes.NotFound);

        var courseId = lesson.CourseId;
        db.Lessons.Remove(lesson);
        await db.SaveChangesAsync(cancellationToken);

        // Keep denormalized TotalLessonsCount in sync for existing enrollments
        var enrollments = await db.Enrollments
            .Where(e => e.CourseId == courseId && e.TotalLessonsCount > 0)
            .ToListAsync(cancellationToken);
        foreach (var enrollment in enrollments)
            enrollment.TotalLessonsCount--;
        if (enrollments.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
