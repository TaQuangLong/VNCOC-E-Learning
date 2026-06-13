namespace ChurchLearn.Api.Features.LearningPaths.UpdateLearningPath;

public record UpdateLearningPathCourseRequest(int CourseId, int OrderIndex);

public record UpdateLearningPathSectionRequest(
    string Title,
    string? Description,
    int OrderIndex,
    IReadOnlyList<UpdateLearningPathCourseRequest> Courses);

public record UpdateLearningPathRequest(
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? EstimatedDurationLabel,
    IReadOnlyList<UpdateLearningPathSectionRequest> Sections);
