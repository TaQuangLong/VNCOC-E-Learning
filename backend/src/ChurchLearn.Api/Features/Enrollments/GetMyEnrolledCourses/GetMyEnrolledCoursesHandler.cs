using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Enrollments.GetMyEnrolledCourses;

public record MyEnrolledCourseResponse(
    int EnrollmentId,
    int CourseId,
    string Title,
    string Slug,
    string? ThumbnailUrl,
    string? Category,
    int ProgressPercent,
    int CompletedLessonsCount,
    int TotalLessonsCount,
    int? LastAccessedLessonId,
    DateTime EnrolledAt);

public class GetMyEnrolledCoursesHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<List<MyEnrolledCourseResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        return await db.Enrollments
            .Where(e => e.UserId == currentUser.UserId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new MyEnrolledCourseResponse(
                e.Id,
                e.CourseId,
                e.Course.Title,
                e.Course.Slug,
                e.Course.ThumbnailUrl,
                e.Course.Category,
                e.ProgressPercent,
                e.CompletedLessonsCount,
                e.TotalLessonsCount,
                e.LastAccessedLessonId,
                e.EnrolledAt))
            .ToListAsync(cancellationToken);
    }
}
