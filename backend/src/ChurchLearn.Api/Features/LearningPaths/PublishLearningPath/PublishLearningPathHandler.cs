using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.LearningPaths.PublishLearningPath;

public record PublishLearningPathResponse(int Id, string Status);

public class PublishLearningPathHandler(AppDbContext db)
{
    public async Task<Result<PublishLearningPathResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var learningPath = await db.LearningPaths
            .Include(path => path.Sections)
                .ThenInclude(section => section.Courses)
                    .ThenInclude(pathCourse => pathCourse.Course)
            .FirstOrDefaultAsync(path => path.Id == id, cancellationToken);

        if (learningPath is null)
            return Result<PublishLearningPathResponse>.Failure(
                $"Learning path {id} not found.",
                ErrorCodes.NotFound);

        if (learningPath.Status == LearningPathStatus.Archived)
            return Result<PublishLearningPathResponse>.Failure(
                "Archived learning paths cannot be published.",
                ErrorCodes.Conflict);

        var pathCourses = learningPath.Sections
            .SelectMany(section => section.Courses)
            .ToList();
        if (pathCourses.Count == 0)
            return Result<PublishLearningPathResponse>.Failure(
                "A learning path must contain at least one course before it can be published.",
                ErrorCodes.Validation);

        var invalidCourseIds = pathCourses
            .Where(pathCourse => pathCourse.Course.Status != CourseStatus.Published)
            .Select(pathCourse => pathCourse.CourseId)
            .Distinct()
            .Order()
            .ToList();
        if (invalidCourseIds.Count > 0)
            return Result<PublishLearningPathResponse>.Failure(
                $"All courses must be Published. Invalid course IDs: {string.Join(", ", invalidCourseIds)}.",
                ErrorCodes.Validation);

        learningPath.Status = LearningPathStatus.Published;
        learningPath.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Result<PublishLearningPathResponse>.Success(
            new PublishLearningPathResponse(learningPath.Id, learningPath.Status.ToString()));
    }
}
