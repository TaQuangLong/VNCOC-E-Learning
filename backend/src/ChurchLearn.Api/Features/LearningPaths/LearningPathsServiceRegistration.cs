using ChurchLearn.Api.Features.LearningPaths.ArchiveLearningPath;
using ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPath;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPaths;
using ChurchLearn.Api.Features.LearningPaths.PublishLearningPath;
using ChurchLearn.Api.Features.LearningPaths.UnpublishLearningPath;
using ChurchLearn.Api.Features.LearningPaths.UpdateLearningPath;
using FluentValidation;

namespace ChurchLearn.Api.Features.LearningPaths;

public static class LearningPathsServiceRegistration
{
    public static IServiceCollection AddLearningPathsFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateLearningPathHandler>();
        services.AddScoped<GetAdminLearningPathsHandler>();
        services.AddScoped<GetAdminLearningPathHandler>();
        services.AddScoped<UpdateLearningPathHandler>();
        services.AddScoped<PublishLearningPathHandler>();
        services.AddScoped<UnpublishLearningPathHandler>();
        services.AddScoped<ArchiveLearningPathHandler>();
        services.AddScoped<IValidator<CreateLearningPathRequest>, CreateLearningPathValidator>();
        services.AddScoped<IValidator<UpdateLearningPathRequest>, UpdateLearningPathValidator>();

        return services;
    }
}
