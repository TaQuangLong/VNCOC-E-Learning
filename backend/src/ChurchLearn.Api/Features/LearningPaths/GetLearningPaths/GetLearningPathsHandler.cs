using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.LearningPaths.GetLearningPaths;

public record LearningPathSummary(
    int Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? ThumbnailUrl,
    string? EstimatedDurationLabel,
    int CourseCount);

public record GetLearningPathsResponse(
    List<LearningPathSummary> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetLearningPathsHandler(AppDbContext db)
{
    public async Task<GetLearningPathsResponse> HandleAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.LearningPaths
            .AsNoTracking()
            .Where(path =>
                path.Status == LearningPathStatus.Published
                && path.Courses.All(pathCourse =>
                    pathCourse.Course.Status == CourseStatus.Published));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(path => path.CreatedAt)
            .ThenByDescending(path => path.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(path => new LearningPathSummary(
                path.Id,
                path.Title,
                path.Slug,
                path.ShortDescription,
                path.ThumbnailUrl,
                path.EstimatedDurationLabel,
                path.Courses.Count))
            .ToListAsync(cancellationToken);

        return new GetLearningPathsResponse(items, totalCount, page, pageSize);
    }
}
