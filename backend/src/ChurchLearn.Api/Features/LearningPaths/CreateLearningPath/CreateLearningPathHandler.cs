using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;

public class CreateLearningPathHandler(
    AppDbContext db,
    IValidator<CreateLearningPathRequest> validator)
{
    public async Task<Result<CreateLearningPathResponse>> HandleAsync(
        CreateLearningPathRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<CreateLearningPathResponse>.Failure(
                string.Join("; ", validation.Errors.Select(error => error.ErrorMessage)),
                ErrorCodes.Validation);

        var slugTaken = await db.LearningPaths
            .AnyAsync(path => path.Slug == request.Slug, cancellationToken);
        if (slugTaken)
            return Result<CreateLearningPathResponse>.Failure(
                $"A learning path with slug '{request.Slug}' already exists.",
                ErrorCodes.Conflict);

        var courseIds = request.Sections
            .SelectMany(section => section.Courses)
            .Select(course => course.CourseId)
            .ToList();
        var publishedCourseIds = await db.Courses
            .AsNoTracking()
            .Where(course => courseIds.Contains(course.Id) && course.Status == CourseStatus.Published)
            .Select(course => course.Id)
            .ToListAsync(cancellationToken);
        var invalidCourseIds = courseIds.Except(publishedCourseIds).Order().ToList();

        if (invalidCourseIds.Count > 0)
            return Result<CreateLearningPathResponse>.Failure(
                $"Courses must exist and be Published. Invalid course IDs: {string.Join(", ", invalidCourseIds)}.",
                ErrorCodes.Validation);

        var learningPath = BuildLearningPath(request);
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync(cancellationToken);

        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return Result<CreateLearningPathResponse>.Success(
            new CreateLearningPathResponse(
                learningPath.Id,
                learningPath.Title,
                learningPath.Slug,
                learningPath.Status.ToString()));
    }

    private static LearningPath BuildLearningPath(CreateLearningPathRequest request)
    {
        var learningPath = new LearningPath
        {
            Title = request.Title,
            Slug = request.Slug,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            EstimatedDurationLabel = request.EstimatedDurationLabel,
        };

        foreach (var sectionRequest in request.Sections)
        {
            var section = new LearningPathSection
            {
                LearningPath = learningPath,
                Title = sectionRequest.Title,
                Description = sectionRequest.Description,
                OrderIndex = sectionRequest.OrderIndex,
            };

            foreach (var courseRequest in sectionRequest.Courses)
            {
                var learningPathCourse = new LearningPathCourse
                {
                    LearningPath = learningPath,
                    LearningPathSection = section,
                    CourseId = courseRequest.CourseId,
                    OrderIndex = courseRequest.OrderIndex,
                };
                section.Courses.Add(learningPathCourse);
                learningPath.Courses.Add(learningPathCourse);
            }

            learningPath.Sections.Add(section);
        }

        return learningPath;
    }
}
