using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPaths;

public record AdminLearningPathSummary(
    int Id,
    string Title,
    string Slug,
    string Status,
    int SectionCount,
    int CourseCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetAdminLearningPathsResponse(
    List<AdminLearningPathSummary> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetAdminLearningPathsHandler(AppDbContext db)
{
    public async Task<GetAdminLearningPathsResponse> HandleAsync(
        int page,
        int pageSize,
        string? status,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.LearningPaths.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<LearningPathStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(path => path.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(path => path.CreatedAt)
            .ThenByDescending(path => path.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(path => new AdminLearningPathSummary(
                path.Id,
                path.Title,
                path.Slug,
                path.Status.ToString(),
                path.Sections.Count,
                path.Courses.Count,
                path.CreatedAt,
                path.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new GetAdminLearningPathsResponse(items, totalCount, page, pageSize);
    }
}
