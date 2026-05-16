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
    int DurationSeconds,
    int OrderIndex,
    bool IsPreview,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ResourceDto> Resources);

public class GetLessonHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<LessonDetailResponse> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(l => l.Resources)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lesson {id} not found.");

        if (!lesson.IsPreview)
        {
            if (!currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Authentication required.");

            var isAdmin = currentUser.IsInRole(AppRoles.Admin) || currentUser.IsInRole(AppRoles.SuperAdmin);
            if (!isAdmin)
            {
                var isEnrolled = await db.Enrollments
                    .AnyAsync(e => e.UserId == currentUser.UserId && e.CourseId == lesson.CourseId, cancellationToken);
                if (!isEnrolled)
                    throw new UnauthorizedAccessException("Enrollment required to access this lesson.");
            }
        }

        return new LessonDetailResponse(
            lesson.Id,
            lesson.CourseId,
            lesson.Title,
            lesson.Description,
            lesson.ContentType.ToString(),
            lesson.YouTubeUrl,
            lesson.TextContent,
            lesson.PdfUrl,
            lesson.DurationSeconds,
            lesson.OrderIndex,
            lesson.IsPreview,
            lesson.CreatedAt,
            lesson.UpdatedAt,
            lesson.Resources.Select(r => new ResourceDto(r.Id, r.Title, r.Url)).ToList());
    }
}
