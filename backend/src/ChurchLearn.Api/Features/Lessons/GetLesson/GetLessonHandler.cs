using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.GetLesson;

public record ResourceDto(int Id, string Title, string Url);

public record LessonDetailResponse(
    int Id,
    int CourseId,
    string Title,
    string? Description,
    string ContentType,
    string? YouTubeUrl,
    string? TextContent,
    string? PdfUrl,
    int? DurationMinutes,
    int OrderIndex,
    bool IsPreview,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ResourceDto> Resources);

public class GetLessonHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<LessonDetailResponse>> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(l => l.Resources)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (lesson is null)
            return Result<LessonDetailResponse>.Failure($"Lesson {id} not found.", ErrorCodes.NotFound);

        if (!lesson.IsPreview)
        {
            if (!currentUser.IsAuthenticated)
                return Result<LessonDetailResponse>.Failure(
                    "Authentication required.", ErrorCodes.Unauthorized);

            var isAdmin = currentUser.IsInRole(AppRoles.Admin) || currentUser.IsInRole(AppRoles.SuperAdmin);
            if (!isAdmin)
            {
                var isEnrolled = await db.Enrollments
                    .AnyAsync(e => e.UserId == currentUser.UserId && e.CourseId == lesson.CourseId, cancellationToken);
                if (!isEnrolled)
                    return Result<LessonDetailResponse>.Failure(
                        "Enrollment required to access this lesson.", ErrorCodes.Forbidden);
            }
        }

        return Result<LessonDetailResponse>.Success(new LessonDetailResponse(
            lesson.Id,
            lesson.CourseId,
            lesson.Title,
            lesson.Description,
            lesson.ContentType.ToString(),
            lesson.YouTubeUrl,
            lesson.TextContent,
            lesson.PdfUrl,
            lesson.DurationMinutes,
            lesson.OrderIndex,
            lesson.IsPreview,
            lesson.CreatedAt,
            lesson.UpdatedAt,
            lesson.Resources.Select(r => new ResourceDto(r.Id, r.Title, r.Url)).ToList()));
    }
}
