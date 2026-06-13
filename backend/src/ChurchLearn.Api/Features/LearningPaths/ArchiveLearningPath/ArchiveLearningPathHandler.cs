using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.LearningPaths.ArchiveLearningPath;

public class ArchiveLearningPathHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var learningPath = await db.LearningPaths.FindAsync([id], cancellationToken);
        if (learningPath is null)
            return Result.Failure(
                $"Learning path {id} not found.",
                ErrorCodes.NotFound);

        learningPath.Status = LearningPathStatus.Archived;
        learningPath.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
