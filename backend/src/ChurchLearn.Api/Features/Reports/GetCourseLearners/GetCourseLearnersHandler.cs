using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Reports.GetCourseLearners;

public class GetCourseLearnersHandler(AppDbContext db)
{
    public async Task<Result<GetCourseLearnersResponse>> HandleAsync(
        int courseId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var course = await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.Id, c.Title })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result<GetCourseLearnersResponse>.Failure(
                $"Course {courseId} not found.", ErrorCodes.NotFound);

        var query = db.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.EnrolledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                e.UserId,
                e.User.DisplayName,
                Email = e.User.Email ?? string.Empty,
                e.EnrolledAt,
                e.ProgressPercent,
                e.CompletedLessonsCount,
                e.TotalLessonsCount,
                e.CompletedAt,
            })
            .ToListAsync(cancellationToken);

        var userIds = items.Select(i => i.UserId).ToList();
        var quizPassedCounts = await db.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.CourseId == courseId && userIds.Contains(lp.UserId) && lp.QuizPassed)
            .GroupBy(lp => lp.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

        var dtos = items.Select(i => new CourseLearnerDto(
            i.UserId,
            i.DisplayName,
            i.Email,
            i.EnrolledAt,
            i.ProgressPercent,
            i.CompletedLessonsCount,
            i.TotalLessonsCount,
            quizPassedCounts.GetValueOrDefault(i.UserId, 0),
            i.CompletedAt)).ToList();

        return Result<GetCourseLearnersResponse>.Success(
            new GetCourseLearnersResponse(course.Id, course.Title, dtos, totalCount, page, pageSize));
    }
}
