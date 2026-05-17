using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Enrollments.GetMyEnrollmentStatus;

public record EnrollmentStatusResponse(
    bool IsEnrolled,
    int? EnrollmentId,
    int? ProgressPercent,
    int? LastAccessedLessonId);

public class GetMyEnrollmentStatusHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<EnrollmentStatusResponse>> HandleAsync(int courseId, CancellationToken cancellationToken)
    {
        var enrollment = await db.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == currentUser.UserId && e.CourseId == courseId, cancellationToken);

        if (enrollment is null)
            return Result<EnrollmentStatusResponse>.Success(
                new EnrollmentStatusResponse(false, null, null, null));

        return Result<EnrollmentStatusResponse>.Success(
            new EnrollmentStatusResponse(true, enrollment.Id, enrollment.ProgressPercent, enrollment.LastAccessedLessonId));
    }
}
