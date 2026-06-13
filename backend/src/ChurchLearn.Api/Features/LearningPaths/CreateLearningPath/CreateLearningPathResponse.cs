namespace ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;

public record CreateLearningPathResponse(
    int Id,
    string Title,
    string Slug,
    string Status);
