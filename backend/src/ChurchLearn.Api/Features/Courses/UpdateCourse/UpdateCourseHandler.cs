using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.UpdateCourse;

public record UpdateCourseRequest(
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? Category,
    string? Level,
    string? Language,
    int AuthorId);

public class UpdateCourseValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseValidator()
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

public class UpdateCourseHandler(AppDbContext db, IValidator<UpdateCourseRequest> validator)
{
    public async Task<Result> HandleAsync(int id, UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var course = await db.Courses.FindAsync([id], cancellationToken);
        if (course is null)
            return Result.Failure($"Course {id} not found.", ErrorCodes.NotFound);

        var authorExists = await db.Authors.AnyAsync(a => a.Id == request.AuthorId, cancellationToken);
        if (!authorExists)
            return Result.Failure($"Author {request.AuthorId} not found.", ErrorCodes.NotFound);

        if (course.Slug != request.Slug)
        {
            var slugTaken = await db.Courses.AnyAsync(c => c.Slug == request.Slug && c.Id != id, cancellationToken);
            if (slugTaken)
                return Result.Failure(
                    $"A course with slug '{request.Slug}' already exists.", ErrorCodes.Conflict);
        }

        course.Title = request.Title;
        course.Slug = request.Slug;
        course.ShortDescription = request.ShortDescription;
        course.Description = request.Description;
        course.ThumbnailUrl = request.ThumbnailUrl;
        course.Category = request.Category;
        course.Level = request.Level;
        course.Language = request.Language;
        course.AuthorId = request.AuthorId;
        course.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
