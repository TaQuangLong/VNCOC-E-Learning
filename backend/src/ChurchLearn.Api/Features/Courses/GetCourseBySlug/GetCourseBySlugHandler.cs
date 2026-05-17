using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.GetCourseBySlug;

public record CourseDetail(
    int Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? Category,
    string? Level,
    string? Language,
    int AuthorId,
    string AuthorName,
    string? AuthorBio,
    string? AuthorAvatarUrl,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class GetCourseBySlugHandler(AppDbContext db)
{
    public async Task<Result<CourseDetail>> HandleAsync(string slug, CancellationToken cancellationToken)
    {
        var course = await db.Courses
            .Include(c => c.Author)
            .Where(c => c.Slug == slug && c.Status == CourseStatus.Published)
            .Select(c => new CourseDetail(
                c.Id, c.Title, c.Slug, c.ShortDescription, c.Description,
                c.ThumbnailUrl, c.Category, c.Level, c.Language,
                c.AuthorId, c.Author.Name, c.Author.Bio, c.Author.AvatarUrl,
                c.Status.ToString(), c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result<CourseDetail>.Failure($"Course '{slug}' not found.", ErrorCodes.NotFound);

        return Result<CourseDetail>.Success(course);
    }
}
