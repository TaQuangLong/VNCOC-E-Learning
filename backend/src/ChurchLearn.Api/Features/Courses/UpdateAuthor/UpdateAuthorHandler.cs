using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;

namespace ChurchLearn.Api.Features.Courses.UpdateAuthor;

public record UpdateAuthorRequest(string Name, string? Bio, string? AvatarUrl, string? UserId);

public record UpdateAuthorResponse(int Id, string Name, string? Bio, string? AvatarUrl, string? UserId);

public class UpdateAuthorValidator : AbstractValidator<UpdateAuthorRequest>
{
    public UpdateAuthorValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Bio).MaximumLength(1000).When(x => x.Bio != null);
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl))
            .WithMessage("AvatarUrl must be a valid http or https URL.");
    }
}

public class UpdateAuthorHandler(AppDbContext db, IValidator<UpdateAuthorRequest> validator)
{
    public async Task<Result<UpdateAuthorResponse>> HandleAsync(int id, UpdateAuthorRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<UpdateAuthorResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var author = await db.Authors.FindAsync([id], cancellationToken);
        if (author is null)
            return Result<UpdateAuthorResponse>.Failure("Author not found.", ErrorCodes.NotFound);

        author.Name = request.Name;
        author.Bio = request.Bio;
        author.AvatarUrl = request.AvatarUrl;
        author.UserId = request.UserId;
        author.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result<UpdateAuthorResponse>.Success(
            new UpdateAuthorResponse(author.Id, author.Name, author.Bio, author.AvatarUrl, author.UserId));
    }
}
