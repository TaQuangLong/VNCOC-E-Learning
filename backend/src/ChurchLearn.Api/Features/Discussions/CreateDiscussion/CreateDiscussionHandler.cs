using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Discussions.CreateDiscussion;

public record CreateDiscussionRequest(string Content);

public record CreateDiscussionResponse(int Id, string Content, DateTime CreatedAt);

public class CreateDiscussionValidator : AbstractValidator<CreateDiscussionRequest>
{
    public CreateDiscussionValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public class CreateDiscussionHandler(
    AppDbContext db,
    ICurrentUser currentUser,
    IValidator<CreateDiscussionRequest> validator)
{
    public async Task<Result<CreateDiscussionResponse>> HandleAsync(
        int lessonId,
        CreateDiscussionRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<CreateDiscussionResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var lessonExists = await db.Lessons
            .AsNoTracking()
            .AnyAsync(l => l.Id == lessonId, cancellationToken);

        if (!lessonExists)
            return Result<CreateDiscussionResponse>.Failure(
                $"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var discussion = new Discussion
        {
            LessonId = lessonId,
            UserId = currentUser.UserId,
            Content = request.Content,
        };

        db.Discussions.Add(discussion);
        await db.SaveChangesAsync(cancellationToken);

        return Result<CreateDiscussionResponse>.Success(
            new CreateDiscussionResponse(discussion.Id, discussion.Content, discussion.CreatedAt));
    }
}
