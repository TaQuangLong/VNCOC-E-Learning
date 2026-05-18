using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Discussions.CreateReply;

public record CreateReplyRequest(string Content);

public record CreateReplyResponse(int Id, string Content, DateTime CreatedAt);

public class CreateReplyValidator : AbstractValidator<CreateReplyRequest>
{
    public CreateReplyValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public class CreateReplyHandler(
    AppDbContext db,
    ICurrentUser currentUser,
    IValidator<CreateReplyRequest> validator)
{
    public async Task<Result<CreateReplyResponse>> HandleAsync(
        int discussionId,
        CreateReplyRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<CreateReplyResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var parent = await db.Discussions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == discussionId, cancellationToken);

        if (parent is null)
            return Result<CreateReplyResponse>.Failure(
                $"Discussion {discussionId} not found.", ErrorCodes.NotFound);

        if (parent.ParentDiscussionId is not null)
            return Result<CreateReplyResponse>.Failure(
                "Replies to replies are not supported. Only one level of nesting is allowed.",
                ErrorCodes.BadRequest);

        var reply = new Discussion
        {
            LessonId = parent.LessonId,
            UserId = currentUser.UserId,
            ParentDiscussionId = discussionId,
            Content = request.Content,
        };

        db.Discussions.Add(reply);
        await db.SaveChangesAsync(cancellationToken);

        return Result<CreateReplyResponse>.Success(
            new CreateReplyResponse(reply.Id, reply.Content, reply.CreatedAt));
    }
}
