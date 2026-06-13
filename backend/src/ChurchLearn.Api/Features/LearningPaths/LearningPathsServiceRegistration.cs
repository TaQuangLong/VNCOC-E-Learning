using ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;
using FluentValidation;

namespace ChurchLearn.Api.Features.LearningPaths;

public static class LearningPathsServiceRegistration
{
    public static IServiceCollection AddLearningPathsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateLearningPathHandler>();
        services.AddScoped<IValidator<CreateLearningPathRequest>, CreateLearningPathValidator>();

        return services;
    }
}
