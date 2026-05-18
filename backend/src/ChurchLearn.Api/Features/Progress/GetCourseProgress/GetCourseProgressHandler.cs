using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Progress.GetCourseProgress;

public record LessonProgressItem(
    int LessonId,
    bool IsCompleted,
    DateTime? CompletedAt,
    int VideoProgressPercent);

public record CourseProgressResponse(
    int CourseId,
    int ProgressPercent,
    int CompletedLessonsCount,
    int TotalLessonsCount,
    IReadOnlyList<LessonProgressItem> Lessons);

public class GetCourseProgressHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<CourseProgressResponse>> HandleAsync(
        int courseId,
        CancellationToken cancellationToken)
    {
        var enrollment = await db.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.UserId == currentUser.UserId && e.CourseId == courseId,
                cancellationToken);

        if (enrollment is null)
            return Result<CourseProgressResponse>.Failure(
                "You are not enrolled in this course.", ErrorCodes.Forbidden);

        var lessonProgresses = await db.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == currentUser.UserId && lp.CourseId == courseId)
            .Select(lp => new LessonProgressItem(
                lp.LessonId,
                lp.IsCompleted,
                lp.CompletedAt,
                lp.VideoProgressPercent))
            .ToListAsync(cancellationToken);

        return Result<CourseProgressResponse>.Success(new CourseProgressResponse(
            CourseId: courseId,
            ProgressPercent: enrollment.ProgressPercent,
            CompletedLessonsCount: enrollment.CompletedLessonsCount,
            TotalLessonsCount: enrollment.TotalLessonsCount,
            Lessons: lessonProgresses));
    }
}
