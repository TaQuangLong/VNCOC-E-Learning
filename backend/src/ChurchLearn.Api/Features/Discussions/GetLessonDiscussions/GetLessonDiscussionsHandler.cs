using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Discussions.GetLessonDiscussions;

public record DiscussionSummaryDto(
    int Id,
    string UserId,
    string AuthorName,
    string Content,
    int ReplyCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted);

public record GetLessonDiscussionsResponse(
    IReadOnlyList<DiscussionSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetLessonDiscussionsHandler(AppDbContext db)
{
    private const string DeletedPlaceholder = "[This post has been removed]";

    public async Task<Result<GetLessonDiscussionsResponse>> HandleAsync(
        int lessonId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var lessonExists = await db.Lessons
            .AsNoTracking()
            .AnyAsync(l => l.Id == lessonId, cancellationToken);

        if (!lessonExists)
            return Result<GetLessonDiscussionsResponse>.Failure(
                $"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var query = db.Discussions
            .AsNoTracking()
            .Where(d => d.LessonId == lessonId && d.ParentDiscussionId == null);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DiscussionSummaryDto(
                d.Id,
                d.UserId,
                d.IsDeleted ? string.Empty : (d.User.UserName ?? string.Empty),
                d.IsDeleted ? DeletedPlaceholder : d.Content,
                d.Replies.Count(r => !r.IsDeleted),
                d.CreatedAt,
                d.UpdatedAt,
                d.IsDeleted))
            .ToListAsync(cancellationToken);

        return Result<GetLessonDiscussionsResponse>.Success(
            new GetLessonDiscussionsResponse(items, totalCount, page, pageSize));
    }
}
