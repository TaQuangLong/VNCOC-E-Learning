namespace ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;

public record CreateLearningPathCourseRequest(int CourseId, int OrderIndex);

public record CreateLearningPathSectionRequest(
    string Title,
    string? Description,
    int OrderIndex,
    IReadOnlyList<CreateLearningPathCourseRequest> Courses);

public record CreateLearningPathRequest(
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? EstimatedDurationLabel,
    IReadOnlyList<CreateLearningPathSectionRequest> Sections);
