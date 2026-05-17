using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.GetLessonResources;

public record ResourceSummary(int Id, string Title, string Url, DateTime CreatedAt);

public class GetLessonResourcesHandler(AppDbContext db)
{
    public async Task<Result<List<ResourceSummary>>> HandleAsync(int lessonId, CancellationToken cancellationToken)
    {
        var lessonExists = await db.Lessons.AnyAsync(l => l.Id == lessonId, cancellationToken);
        if (!lessonExists)
            return Result<List<ResourceSummary>>.Failure($"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var resources = await db.Resources
            .Where(r => r.LessonId == lessonId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ResourceSummary(r.Id, r.Title, r.Url, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<ResourceSummary>>.Success(resources);
    }
}
