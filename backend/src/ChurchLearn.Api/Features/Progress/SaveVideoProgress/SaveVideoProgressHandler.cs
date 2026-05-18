using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Progress.SaveVideoProgress;

public record SaveVideoProgressRequest(int VideoProgressPercent, int VideoWatchedSeconds);

public record SaveVideoProgressResponse(int LessonId, int VideoProgressPercent, int VideoWatchedSeconds);

public class SaveVideoProgressHandler(AppDbContext db, ICurrentUser currentUser, IValidator<SaveVideoProgressRequest> validator)
{
    public async Task<Result<SaveVideoProgressResponse>> HandleAsync(
        int lessonId,
        SaveVideoProgressRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<SaveVideoProgressResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);
        var lesson = await db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken);

        if (lesson is null)
            return Result<SaveVideoProgressResponse>.Failure(
                $"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var enrolled = await db.Enrollments
            .AnyAsync(
                e => e.UserId == currentUser.UserId && e.CourseId == lesson.CourseId,
                cancellationToken);

        if (!enrolled)
            return Result<SaveVideoProgressResponse>.Failure(
                "You are not enrolled in this course.", ErrorCodes.Forbidden);

        var progress = await db.LessonProgresses
            .FirstOrDefaultAsync(
                p => p.UserId == currentUser.UserId && p.LessonId == lessonId,
                cancellationToken);

        if (progress is null)
        {
            progress = new LessonProgress
            {
                UserId = currentUser.UserId,
                CourseId = lesson.CourseId,
                LessonId = lessonId,
                VideoProgressPercent = request.VideoProgressPercent,
                VideoWatchedSeconds = request.VideoWatchedSeconds,
                LastWatchedAt = DateTime.UtcNow,
            };
            db.LessonProgresses.Add(progress);
        }
        else
        {
            progress.VideoProgressPercent = request.VideoProgressPercent;
            progress.VideoWatchedSeconds = request.VideoWatchedSeconds;
            progress.LastWatchedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        return Result<SaveVideoProgressResponse>.Success(
            new SaveVideoProgressResponse(lessonId, progress.VideoProgressPercent, progress.VideoWatchedSeconds));
    }
}
