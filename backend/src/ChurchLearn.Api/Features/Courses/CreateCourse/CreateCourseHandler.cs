using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.CreateCourse;

public record CreateCourseRequest(
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? Category,
    string? Level,
    string? Language,
    int AuthorId);

public record CreateCourseResponse(int Id, string Title, string Slug, string Status);

public class CreateCourseValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase kebab-case (e.g. my-course-title).");
        RuleFor(x => x.AuthorId).GreaterThan(0);
        RuleFor(x => x.ShortDescription).MaximumLength(500).When(x => x.ShortDescription != null);
        RuleFor(x => x.ThumbnailUrl).MaximumLength(2048).When(x => x.ThumbnailUrl != null);
    }
}

public class CreateCourseHandler(AppDbContext db, IValidator<CreateCourseRequest> validator)
{
    public async Task<CreateCourseResponse> HandleAsync(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var authorExists = await db.Authors.AnyAsync(a => a.Id == request.AuthorId, cancellationToken);
        if (!authorExists)
            throw new KeyNotFoundException($"Author {request.AuthorId} not found.");

        var slugTaken = await db.Courses.AnyAsync(c => c.Slug == request.Slug, cancellationToken);
        if (slugTaken)
            throw new InvalidOperationException($"A course with slug '{request.Slug}' already exists.");

        var course = new Course
        {
            Title = request.Title,
            Slug = request.Slug,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Category = request.Category,
            Level = request.Level,
            Language = request.Language,
            AuthorId = request.AuthorId,
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateCourseResponse(course.Id, course.Title, course.Slug, course.Status.ToString());
    }
}
