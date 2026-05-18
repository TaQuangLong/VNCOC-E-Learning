using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Discussions.UpdateDiscussion;

public record UpdateDiscussionRequest(string Content);

public record UpdateDiscussionResponse(int Id, string Content, DateTime UpdatedAt);

public class UpdateDiscussionValidator : AbstractValidator<UpdateDiscussionRequest>
{
    public UpdateDiscussionValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public class UpdateDiscussionHandler(
    AppDbContext db,
    ICurrentUser currentUser,
    IValidator<UpdateDiscussionRequest> validator)
{
    public async Task<Result<UpdateDiscussionResponse>> HandleAsync(
        int discussionId,
        UpdateDiscussionRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<UpdateDiscussionResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var discussion = await db.Discussions
            .FirstOrDefaultAsync(d => d.Id == discussionId, cancellationToken);

        if (discussion is null)
            return Result<UpdateDiscussionResponse>.Failure(
                $"Discussion {discussionId} not found.", ErrorCodes.NotFound);

        if (discussion.IsDeleted)
            return Result<UpdateDiscussionResponse>.Failure(
                "Cannot edit a deleted post.", ErrorCodes.BadRequest);

        if (discussion.UserId != currentUser.UserId)
            return Result<UpdateDiscussionResponse>.Failure(
                "You can only edit your own posts.", ErrorCodes.Forbidden);

        discussion.Content = request.Content;
        discussion.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result<UpdateDiscussionResponse>.Success(
            new UpdateDiscussionResponse(discussion.Id, discussion.Content, discussion.UpdatedAt));
    }
}
