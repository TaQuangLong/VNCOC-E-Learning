using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Discussions.DeleteDiscussion;

public class DeleteDiscussionHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<bool>> HandleAsync(
        int discussionId,
        CancellationToken cancellationToken)
    {
        var discussion = await db.Discussions
            .FirstOrDefaultAsync(d => d.Id == discussionId, cancellationToken);

        if (discussion is null)
            return Result<bool>.Failure(
                $"Discussion {discussionId} not found.", ErrorCodes.NotFound);

        if (discussion.UserId != currentUser.UserId)
            return Result<bool>.Failure(
                "You can only delete your own posts.", ErrorCodes.Forbidden);

        discussion.IsDeleted = true;
        discussion.DeletedBy = currentUser.UserId;
        discussion.DeletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
