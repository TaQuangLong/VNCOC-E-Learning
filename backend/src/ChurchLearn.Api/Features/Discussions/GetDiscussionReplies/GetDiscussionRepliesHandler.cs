using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Discussions.GetDiscussionReplies;

public record ReplyDto(
    int Id,
    string UserId,
    string AuthorName,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted);

public record GetDiscussionRepliesResponse(
    IReadOnlyList<ReplyDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetDiscussionRepliesHandler(AppDbContext db)
{
    private const string DeletedPlaceholder = "[This post has been removed]";

    public async Task<Result<GetDiscussionRepliesResponse>> HandleAsync(
        int discussionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var discussionExists = await db.Discussions
            .AsNoTracking()
            .AnyAsync(d => d.Id == discussionId && d.ParentDiscussionId == null, cancellationToken);

        if (!discussionExists)
            return Result<GetDiscussionRepliesResponse>.Failure(
                $"Discussion {discussionId} not found.", ErrorCodes.NotFound);

        var query = db.Discussions
            .AsNoTracking()
            .Where(d => d.ParentDiscussionId == discussionId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new ReplyDto(
                d.Id,
                d.UserId,
                d.IsDeleted ? string.Empty : (d.User.UserName ?? string.Empty),
                d.IsDeleted ? DeletedPlaceholder : d.Content,
                d.CreatedAt,
                d.UpdatedAt,
                d.IsDeleted))
            .ToListAsync(cancellationToken);

        return Result<GetDiscussionRepliesResponse>.Success(
            new GetDiscussionRepliesResponse(items, totalCount, page, pageSize));
    }
}
