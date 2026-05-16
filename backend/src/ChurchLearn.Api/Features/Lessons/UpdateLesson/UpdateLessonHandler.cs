using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;

namespace ChurchLearn.Api.Features.Lessons.UpdateLesson;

public record UpdateLessonRequest(
    string Title,
    string? Description,
    ContentType ContentType,
    string? YouTubeUrl,
    string? TextContent,
    string? PdfUrl,
    int DurationSeconds,
    int OrderIndex,
    bool IsPreview) : ILessonRequest;

public class UpdateLessonValidator : AbstractValidator<UpdateLessonRequest>
{
    public UpdateLessonValidator()
    {
        LessonValidationRules.ApplyCommonRules(this);
    }
}

public class UpdateLessonHandler(AppDbContext db, IValidator<UpdateLessonRequest> validator)
{
    public async Task HandleAsync(int id, UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var lesson = await db.Lessons.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Lesson {id} not found.");

        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.ContentType = request.ContentType;
        lesson.YouTubeUrl = request.YouTubeUrl;
        lesson.TextContent = request.TextContent;
        lesson.PdfUrl = request.PdfUrl;
        lesson.DurationSeconds = request.DurationSeconds;
        lesson.OrderIndex = request.OrderIndex;
        lesson.IsPreview = request.IsPreview;
        lesson.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
