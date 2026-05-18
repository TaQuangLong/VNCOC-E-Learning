using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Progress.MarkLessonComplete;

public record MarkLessonCompleteResponse(
    int LessonId,
    int CourseId,
    bool IsCompleted,
    int ProgressPercent,
    int CompletedLessonsCount,
    int TotalLessonsCount);

public class MarkLessonCompleteHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<MarkLessonCompleteResponse>> HandleAsync(
        int lessonId,
        CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken);

        if (lesson is null)
            return Result<MarkLessonCompleteResponse>.Failure(
                $"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var enrollment = await db.Enrollments
            .FirstOrDefaultAsync(
                e => e.UserId == currentUser.UserId && e.CourseId == lesson.CourseId,
                cancellationToken);

        if (enrollment is null)
            return Result<MarkLessonCompleteResponse>.Failure(
                "You are not enrolled in this course.", ErrorCodes.Forbidden);

        var progress = await db.LessonProgresses
            .FirstOrDefaultAsync(
                p => p.UserId == currentUser.UserId && p.LessonId == lessonId,
                cancellationToken);

        var wasAlreadyCompleted = progress?.IsCompleted ?? false;

        if (progress is null)
        {
            progress = new LessonProgress
            {
                UserId = currentUser.UserId,
                CourseId = lesson.CourseId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
            };
            db.LessonProgresses.Add(progress);
        }
        else if (!progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        if (!wasAlreadyCompleted)
            enrollment.CompletedLessonsCount++;

        enrollment.LastAccessedLessonId = lessonId;
        enrollment.ProgressPercent = enrollment.TotalLessonsCount > 0
            ? (int)Math.Round((double)enrollment.CompletedLessonsCount / enrollment.TotalLessonsCount * 100)
            : 0;

        await db.SaveChangesAsync(cancellationToken);

        return Result<MarkLessonCompleteResponse>.Success(new MarkLessonCompleteResponse(
            LessonId: lessonId,
            CourseId: lesson.CourseId,
            IsCompleted: true,
            ProgressPercent: enrollment.ProgressPercent,
            CompletedLessonsCount: enrollment.CompletedLessonsCount,
            TotalLessonsCount: enrollment.TotalLessonsCount));
    }
}
