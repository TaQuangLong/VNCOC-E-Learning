using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.LearningPaths.UnpublishLearningPath;

public record UnpublishLearningPathResponse(int Id, string Status);

public class UnpublishLearningPathHandler(AppDbContext db)
{
    public async Task<Result<UnpublishLearningPathResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var learningPath = await db.LearningPaths.FindAsync([id], cancellationToken);
        if (learningPath is null)
            return Result<UnpublishLearningPathResponse>.Failure(
                $"Learning path {id} not found.",
                ErrorCodes.NotFound);

        if (learningPath.Status == LearningPathStatus.Archived)
            return Result<UnpublishLearningPathResponse>.Failure(
                "Archived learning paths cannot be unpublished.",
                ErrorCodes.Conflict);

        learningPath.Status = LearningPathStatus.Draft;
        learningPath.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Result<UnpublishLearningPathResponse>.Success(
            new UnpublishLearningPathResponse(learningPath.Id, learningPath.Status.ToString()));
    }
}
