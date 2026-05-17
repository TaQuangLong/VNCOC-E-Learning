using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Enrollments.EnrollCourse;

public record EnrollCourseResponse(
    int EnrollmentId,
    int CourseId,
    DateTime EnrolledAt,
    int TotalLessonsCount);

public class EnrollCourseHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<EnrollCourseResponse>> HandleAsync(int courseId, CancellationToken cancellationToken)
    {
        var course = await db.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);

        if (course is null)
            return Result<EnrollCourseResponse>.Failure($"Course {courseId} not found.", ErrorCodes.NotFound);

        if (course.Status != CourseStatus.Published)
            return Result<EnrollCourseResponse>.Failure(
                "Enrollment is only available for published courses.", ErrorCodes.BadRequest);

        var alreadyEnrolled = await db.Enrollments
            .AnyAsync(e => e.UserId == currentUser.UserId && e.CourseId == courseId, cancellationToken);

        if (alreadyEnrolled)
            return Result<EnrollCourseResponse>.Failure(
                "You are already enrolled in this course.", ErrorCodes.Conflict);

        var totalLessons = await db.Lessons
            .CountAsync(l => l.CourseId == courseId, cancellationToken);

        var enrollment = new Enrollment
        {
            UserId = currentUser.UserId,
            CourseId = courseId,
            TotalLessonsCount = totalLessons,
        };

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync(cancellationToken);

        return Result<EnrollCourseResponse>.Success(
            new EnrollCourseResponse(enrollment.Id, enrollment.CourseId, enrollment.EnrolledAt, enrollment.TotalLessonsCount));
    }
}
