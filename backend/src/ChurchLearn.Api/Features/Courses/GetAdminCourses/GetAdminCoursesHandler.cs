using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.GetAdminCourses;

public record AdminCourseSummary(
    int Id,
    string Title,
    string Slug,
    string? Category,
    string? Level,
    string AuthorName,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetAdminCoursesResponse(List<AdminCourseSummary> Items, int TotalCount, int Page, int PageSize);

public class GetAdminCoursesHandler(AppDbContext db)
{
    public async Task<GetAdminCoursesResponse> HandleAsync(
        int page,
        int pageSize,
        string? status,
        string? title,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Courses
            .Include(c => c.Author)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CourseStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(c => c.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(c => EF.Functions.ILike(c.Title, $"%{title}%"));

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminCourseSummary(
                c.Id, c.Title, c.Slug, c.Category, c.Level,
                c.Author.Name, c.Status.ToString(), c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new GetAdminCoursesResponse(items, total, page, pageSize);
    }
}
