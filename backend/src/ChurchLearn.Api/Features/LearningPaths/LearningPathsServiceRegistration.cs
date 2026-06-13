using ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPath;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPaths;
using FluentValidation;

namespace ChurchLearn.Api.Features.LearningPaths;

public static class LearningPathsServiceRegistration
{
    public static IServiceCollection AddLearningPathsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateLearningPathHandler>();
        services.AddScoped<GetAdminLearningPathsHandler>();
        services.AddScoped<GetAdminLearningPathHandler>();
        services.AddScoped<IValidator<CreateLearningPathRequest>, CreateLearningPathValidator>();

        return services;
    }
}
