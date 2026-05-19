using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Reports.GetUserProgress;

public class GetUserProgressHandler(AppDbContext db)
{
    public async Task<Result<GetUserProgressResponse>> HandleAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.DisplayName, Email = u.Email ?? string.Empty })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result<GetUserProgressResponse>.Failure(
                $"User {userId} not found.", ErrorCodes.NotFound);

        var enrollments = await db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => new
            {
                e.CourseId,
                e.Course.Title,
                CourseSlug = e.Course.Slug,
                e.EnrolledAt,
                e.ProgressPercent,
                e.CompletedLessonsCount,
                e.TotalLessonsCount,
                e.CompletedAt,
            })
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);

        var courseIds = enrollments.Select(e => e.CourseId).ToList();

        var quizResults = await db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.UserId == userId)
            .Select(qa => new
            {
                qa.QuizId,
                QuizTitle = qa.Quiz.Title,
                LessonId = qa.Quiz.LessonId,
                LessonTitle = qa.Quiz.Lesson.Title,
                CourseId = qa.Quiz.Lesson.CourseId,
                qa.Score,
                qa.Passed,
                qa.SubmittedAt,
            })
            .Where(qa => courseIds.Contains(qa.CourseId))
            .ToListAsync(cancellationToken);

        var quizByCourse = quizResults
            .GroupBy(q => q.CourseId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(q => new UserQuizResultDto(
                    q.QuizId,
                    q.QuizTitle,
                    q.LessonId,
                    q.LessonTitle,
                    q.Score,
                    q.Passed,
                    q.SubmittedAt)).ToList());

        var courses = enrollments.Select(e => new UserCourseProgressDto(
            e.CourseId,
            e.Title,
            e.CourseSlug,
            e.EnrolledAt,
            e.ProgressPercent,
            e.CompletedLessonsCount,
            e.TotalLessonsCount,
            e.CompletedAt.HasValue,
            e.CompletedAt,
            quizByCourse.GetValueOrDefault(e.CourseId, []))).ToList();

        return Result<GetUserProgressResponse>.Success(
            new GetUserProgressResponse(user.Id, user.DisplayName, user.Email, courses));
    }
}
