using ChurchLearn.Api.Features.Progress.GetCourseProgress;
using ChurchLearn.Api.Features.Progress.MarkLessonComplete;
using ChurchLearn.Api.Features.Progress.SaveVideoProgress;
using FluentValidation;

namespace ChurchLearn.Api.Features.Progress;

public static class ProgressServiceRegistration
{
    public static IServiceCollection AddProgressFeature(this IServiceCollection services)
    {
        services.AddScoped<MarkLessonCompleteHandler>();
        services.AddScoped<SaveVideoProgressHandler>();
        services.AddScoped<GetCourseProgressHandler>();
        services.AddScoped<IValidator<SaveVideoProgressRequest>, SaveVideoProgressValidator>();
        return services;
    }
}
