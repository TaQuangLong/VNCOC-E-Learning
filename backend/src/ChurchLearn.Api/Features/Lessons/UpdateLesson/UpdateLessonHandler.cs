using ChurchLearn.Api.Common;
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
    int? DurationMinutes,
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
    public async Task<Result> HandleAsync(int id, UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var lesson = await db.Lessons.FindAsync([id], cancellationToken);
        if (lesson is null)
            return Result.Failure($"Lesson {id} not found.", ErrorCodes.NotFound);

        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.ContentType = request.ContentType;
        lesson.YouTubeUrl = request.YouTubeUrl;
        lesson.TextContent = request.TextContent;
        lesson.PdfUrl = request.PdfUrl;
        lesson.DurationMinutes = request.DurationMinutes;
        lesson.OrderIndex = request.OrderIndex;
        lesson.IsPreview = request.IsPreview;
        lesson.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
