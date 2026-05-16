using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.GetPublishedCourses;

public record PublishedCourseSummary(
    int Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? ThumbnailUrl,
    string? Category,
    string? Level,
    string? Language,
    string AuthorName,
    DateTime CreatedAt);

public record GetPublishedCoursesResponse(List<PublishedCourseSummary> Items, int TotalCount, int Page, int PageSize);

public class GetPublishedCoursesHandler(AppDbContext db)
{
    public async Task<GetPublishedCoursesResponse> HandleAsync(
        int page,
        int pageSize,
        string? category,
        string? level,
        string? title,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Courses
            .Include(c => c.Author)
            .Where(c => c.Status == CourseStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(c => EF.Functions.ILike(c.Title, $"%{title}%"));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(c => c.Category == category);

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(c => c.Level == level);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new PublishedCourseSummary(
                c.Id, c.Title, c.Slug, c.ShortDescription, c.ThumbnailUrl,
                c.Category, c.Level, c.Language, c.Author.Name, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetPublishedCoursesResponse(items, total, page, pageSize);
    }
}
