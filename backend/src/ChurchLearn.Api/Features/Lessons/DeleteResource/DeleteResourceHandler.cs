using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.Lessons.DeleteResource;

public class DeleteResourceHandler(AppDbContext db)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var resource = await db.Resources.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Resource {id} not found.");

        db.Resources.Remove(resource);
        await db.SaveChangesAsync(cancellationToken);
    }
}
