using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.DeleteCourse;

public class DeleteCourseHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var course = await db.Courses.FindAsync([id], cancellationToken);
        if (course is null)
            return Result.Failure($"Course {id} not found.", ErrorCodes.NotFound);

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        var affectedPaths = await db.LearningPaths
            .Where(path =>
                path.Status == LearningPathStatus.Published
                && path.Courses.Any(pathCourse => pathCourse.CourseId == id))
            .ToListAsync(cancellationToken);
        var updatedAt = DateTime.UtcNow;

        course.Status = CourseStatus.Archived;
        course.UpdatedAt = updatedAt;
        foreach (var learningPath in affectedPaths)
        {
            learningPath.Status = LearningPathStatus.Draft;
            learningPath.UpdatedAt = updatedAt;
        }

        await db.SaveChangesAsync(cancellationToken);

        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
