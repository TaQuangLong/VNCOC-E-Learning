using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.AddResource;

public record AddResourceRequest(string Title, string Url);

public record AddResourceResponse(int Id, string Title, string Url);

public class AddResourceValidator : AbstractValidator<AddResourceRequest>
{
    public AddResourceValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Url must be a valid HTTP/HTTPS URL.")
            .MaximumLength(2048);
    }
}

public class AddResourceHandler(AppDbContext db, IValidator<AddResourceRequest> validator)
{
    public async Task<Result<AddResourceResponse>> HandleAsync(int lessonId, AddResourceRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<AddResourceResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var lessonExists = await db.Lessons.AnyAsync(l => l.Id == lessonId, cancellationToken);
        if (!lessonExists)
            return Result<AddResourceResponse>.Failure($"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var resource = new Resource
        {
            LessonId = lessonId,
            Title = request.Title,
            Url = request.Url,
        };

        db.Resources.Add(resource);
        await db.SaveChangesAsync(cancellationToken);

        return Result<AddResourceResponse>.Success(
            new AddResourceResponse(resource.Id, resource.Title, resource.Url));
    }
}
