using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPath;

public record AdminLearningPathCourseDetail(
    int CourseId,
    string Title,
    string Slug,
    string Status,
    int OrderIndex);

public record AdminLearningPathSectionDetail(
    int Id,
    string Title,
    string? Description,
    int OrderIndex,
    List<AdminLearningPathCourseDetail> Courses);

public record AdminLearningPathDetail(
    int Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? EstimatedDurationLabel,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<AdminLearningPathSectionDetail> Sections);

public class GetAdminLearningPathHandler(AppDbContext db)
{
    public async Task<Result<AdminLearningPathDetail>> HandleAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var learningPath = await db.LearningPaths
            .AsNoTracking()
            .Where(path => path.Id == id)
            .Select(path => new AdminLearningPathDetail(
                path.Id,
                path.Title,
                path.Slug,
                path.ShortDescription,
                path.Description,
                path.ThumbnailUrl,
                path.EstimatedDurationLabel,
                path.Status.ToString(),
                path.CreatedAt,
                path.UpdatedAt,
                path.Sections
                    .OrderBy(section => section.OrderIndex)
                    .Select(section => new AdminLearningPathSectionDetail(
                        section.Id,
                        section.Title,
                        section.Description,
                        section.OrderIndex,
                        section.Courses
                            .OrderBy(course => course.OrderIndex)
                            .Select(course => new AdminLearningPathCourseDetail(
                                course.CourseId,
                                course.Course.Title,
                                course.Course.Slug,
                                course.Course.Status.ToString(),
                                course.OrderIndex))
                            .ToList()))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (learningPath is null)
        {
            return Result<AdminLearningPathDetail>.Failure(
                $"Learning path {id} not found.",
                ErrorCodes.NotFound);
        }

        return Result<AdminLearningPathDetail>.Success(learningPath);
    }
}
