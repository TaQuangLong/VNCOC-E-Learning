using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.LearningPaths.UpdateLearningPath;

public class UpdateLearningPathHandler(
    AppDbContext db,
    IValidator<UpdateLearningPathRequest> validator)
{
    public async Task<Result<UpdateLearningPathResponse>> HandleAsync(
        int id,
        UpdateLearningPathRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<UpdateLearningPathResponse>.Failure(
                string.Join("; ", validation.Errors.Select(error => error.ErrorMessage)),
                ErrorCodes.Validation);

        var learningPath = await db.LearningPaths
            .Include(path => path.Sections)
                .ThenInclude(section => section.Courses)
            .Include(path => path.Courses)
            .FirstOrDefaultAsync(path => path.Id == id, cancellationToken);

        if (learningPath is null)
            return Result<UpdateLearningPathResponse>.Failure(
                $"Learning path {id} not found.",
                ErrorCodes.NotFound);

        if (learningPath.Status == LearningPathStatus.Archived)
            return Result<UpdateLearningPathResponse>.Failure(
                "Archived learning paths cannot be updated.",
                ErrorCodes.Conflict);

        var slugTaken = await db.LearningPaths
            .AnyAsync(path => path.Slug == request.Slug && path.Id != id, cancellationToken);
        if (slugTaken)
            return Result<UpdateLearningPathResponse>.Failure(
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
            return Result<UpdateLearningPathResponse>.Failure(
                $"Courses must exist and be Published. Invalid course IDs: {string.Join(", ", invalidCourseIds)}.",
                ErrorCodes.Validation);

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        db.LearningPathCourses.RemoveRange(learningPath.Courses);
        db.LearningPathSections.RemoveRange(learningPath.Sections);
        await db.SaveChangesAsync(cancellationToken);

        learningPath.Courses.Clear();
        learningPath.Sections.Clear();
        UpdateFields(learningPath, request);
        AddSections(learningPath, request.Sections);
        await db.SaveChangesAsync(cancellationToken);

        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return Result<UpdateLearningPathResponse>.Success(
            new UpdateLearningPathResponse(
                learningPath.Id,
                learningPath.Title,
                learningPath.Slug,
                learningPath.Status.ToString()));
    }

    private static void UpdateFields(
        LearningPath learningPath,
        UpdateLearningPathRequest request)
    {
        learningPath.Title = request.Title;
        learningPath.Slug = request.Slug;
        learningPath.ShortDescription = request.ShortDescription;
        learningPath.Description = request.Description;
        learningPath.ThumbnailUrl = request.ThumbnailUrl;
        learningPath.EstimatedDurationLabel = request.EstimatedDurationLabel;
        learningPath.UpdatedAt = DateTime.UtcNow;
    }

    private static void AddSections(
        LearningPath learningPath,
        IReadOnlyList<UpdateLearningPathSectionRequest> sectionRequests)
    {
        foreach (var sectionRequest in sectionRequests)
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
    }
}
