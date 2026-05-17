using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.Lessons.DeleteResource;

public class DeleteResourceHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var resource = await db.Resources.FindAsync([id], cancellationToken);
        if (resource is null)
            return Result.Failure($"Resource {id} not found.", ErrorCodes.NotFound);

        db.Resources.Remove(resource);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
