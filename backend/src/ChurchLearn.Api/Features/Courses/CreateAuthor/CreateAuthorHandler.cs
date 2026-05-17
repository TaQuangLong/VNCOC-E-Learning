using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;

namespace ChurchLearn.Api.Features.Courses.CreateAuthor;

public record CreateAuthorRequest(string Name, string? Bio, string? AvatarUrl, string? UserId);

public record CreateAuthorResponse(int Id, string Name);

public class CreateAuthorValidator : AbstractValidator<CreateAuthorRequest>
{
    public CreateAuthorValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Bio).MaximumLength(1000).When(x => x.Bio != null);
        RuleFor(x => x.AvatarUrl).MaximumLength(2048).When(x => x.AvatarUrl != null);
    }
}

public class CreateAuthorHandler(AppDbContext db, IValidator<CreateAuthorRequest> validator)
{
    public async Task<Result<CreateAuthorResponse>> HandleAsync(CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<CreateAuthorResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var author = new Author
        {
            Name = request.Name,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            UserId = request.UserId,
        };

        db.Authors.Add(author);
        await db.SaveChangesAsync(cancellationToken);

        return Result<CreateAuthorResponse>.Success(new CreateAuthorResponse(author.Id, author.Name));
    }
}
