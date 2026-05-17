using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.CreateLesson;

public record CreateLessonRequest(
    string Title,
    string? Description,
    ContentType ContentType,
    string? YouTubeUrl,
    string? TextContent,
    string? PdfUrl,
    int DurationSeconds,
    int OrderIndex,
    bool IsPreview) : ILessonRequest;

public record CreateLessonResponse(int Id, string Title, int OrderIndex);

public class CreateLessonValidator : AbstractValidator<CreateLessonRequest>
{
    public CreateLessonValidator()
    {
        LessonValidationRules.ApplyCommonRules(this);
    }
}

public class CreateLessonHandler(AppDbContext db, IValidator<CreateLessonRequest> validator)
{
    public async Task<Result<CreateLessonResponse>> HandleAsync(int courseId, CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<CreateLessonResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId, cancellationToken);
        if (!courseExists)
            return Result<CreateLessonResponse>.Failure($"Course {courseId} not found.", ErrorCodes.NotFound);

        var lesson = new Lesson
        {
            CourseId = courseId,
            Title = request.Title,
            Description = request.Description,
            ContentType = request.ContentType,
            YouTubeUrl = request.YouTubeUrl,
            TextContent = request.TextContent,
            PdfUrl = request.PdfUrl,
            DurationSeconds = request.DurationSeconds,
            OrderIndex = request.OrderIndex,
            IsPreview = request.IsPreview,
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(cancellationToken);

        // Keep denormalized TotalLessonsCount in sync for existing enrollments
        var enrollments = await db.Enrollments
            .Where(e => e.CourseId == courseId)
            .ToListAsync(cancellationToken);
        foreach (var enrollment in enrollments)
            enrollment.TotalLessonsCount++;
        if (enrollments.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        return Result<CreateLessonResponse>.Success(
            new CreateLessonResponse(lesson.Id, lesson.Title, lesson.OrderIndex));
    }
}
