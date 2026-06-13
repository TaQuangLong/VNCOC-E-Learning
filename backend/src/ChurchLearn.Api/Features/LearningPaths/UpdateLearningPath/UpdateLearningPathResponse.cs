namespace ChurchLearn.Api.Features.LearningPaths.UpdateLearningPath;

public record UpdateLearningPathResponse(
    int Id,
    string Title,
    string Slug,
    string Status);
