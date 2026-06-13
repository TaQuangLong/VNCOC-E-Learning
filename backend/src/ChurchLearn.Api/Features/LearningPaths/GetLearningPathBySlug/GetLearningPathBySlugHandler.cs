using System.Text.Json.Serialization;
using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.LearningPaths.GetLearningPathBySlug;

public record LearningPathProgress(
    int CompletedCoursesCount,
    int TotalCoursesCount,
    int ProgressPercent);

public record LearningPathCourseDetail(
    int Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? ThumbnailUrl,
    string? Level,
    int LessonCount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? IsEnrolled,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? ProgressPercent,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? IsCompleted);

public record LearningPathSectionDetail(
    int Id,
    string Title,
    string? Description,
    List<LearningPathCourseDetail> Courses);

public record GetLearningPathBySlugResponse(
    int Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? EstimatedDurationLabel,
    List<LearningPathSectionDetail> Sections,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    LearningPathProgress? Progress);

public class GetLearningPathBySlugHandler(
    AppDbContext db,
    ICurrentUser currentUser)
{
    public async Task<Result<GetLearningPathBySlugResponse>> HandleAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        var learningPath = await db.LearningPaths
            .AsNoTracking()
            .Where(path =>
                path.Slug == slug
                && path.Status == LearningPathStatus.Published
                && path.Courses.All(pathCourse =>
                    pathCourse.Course.Status == CourseStatus.Published))
            .Select(path => new GetLearningPathBySlugResponse(
                path.Id,
                path.Title,
                path.Slug,
                path.ShortDescription,
                path.Description,
                path.ThumbnailUrl,
                path.EstimatedDurationLabel,
                path.Sections
                    .OrderBy(section => section.OrderIndex)
                    .Select(section => new LearningPathSectionDetail(
                        section.Id,
                        section.Title,
                        section.Description,
                        section.Courses
                            .OrderBy(pathCourse => pathCourse.OrderIndex)
                            .Select(pathCourse => new LearningPathCourseDetail(
                                pathCourse.CourseId,
                                pathCourse.Course.Title,
                                pathCourse.Course.Slug,
                                pathCourse.Course.ShortDescription,
                                pathCourse.Course.ThumbnailUrl,
                                pathCourse.Course.Level,
                                pathCourse.Course.Lessons.Count,
                                null,
                                null,
                                null))
                            .ToList()))
                    .ToList(),
                null))
            .FirstOrDefaultAsync(cancellationToken);

        if (learningPath is null)
            return Result<GetLearningPathBySlugResponse>.Failure(
                $"Learning path '{slug}' not found.",
                ErrorCodes.NotFound);

        if (!currentUser.IsAuthenticated)
            return Result<GetLearningPathBySlugResponse>.Success(learningPath);

        var courseIds = learningPath.Sections
            .SelectMany(section => section.Courses)
            .Select(course => course.Id)
            .ToList();
        var enrollments = await db.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.UserId == currentUser.UserId
                && courseIds.Contains(enrollment.CourseId))
            .ToDictionaryAsync(enrollment => enrollment.CourseId, cancellationToken);

        var sections = learningPath.Sections
            .Select(section => section with
            {
                Courses = section.Courses
                    .Select(course => AddEnrollmentProgress(course, enrollments))
                    .ToList(),
            })
            .ToList();
        var completedCoursesCount = enrollments.Values.Count(IsCompleted);
        var totalCoursesCount = courseIds.Count;
        var progressPercent = totalCoursesCount > 0
            ? (int)Math.Round((double)completedCoursesCount / totalCoursesCount * 100)
            : 0;

        return Result<GetLearningPathBySlugResponse>.Success(learningPath with
        {
            Sections = sections,
            Progress = new LearningPathProgress(
                completedCoursesCount,
                totalCoursesCount,
                progressPercent),
        });
    }

    private static LearningPathCourseDetail AddEnrollmentProgress(
        LearningPathCourseDetail course,
        IReadOnlyDictionary<int, Enrollment> enrollments)
    {
        if (!enrollments.TryGetValue(course.Id, out var enrollment))
            return course with
            {
                IsEnrolled = false,
                ProgressPercent = 0,
                IsCompleted = false,
            };

        return course with
        {
            IsEnrolled = true,
            ProgressPercent = enrollment.ProgressPercent,
            IsCompleted = IsCompleted(enrollment),
        };
    }

    private static bool IsCompleted(Enrollment enrollment) =>
        enrollment.CompletedAt.HasValue || enrollment.ProgressPercent >= 100;
}
