using ChurchLearn.Api.Features.Lessons.AddResource;
using ChurchLearn.Api.Features.Lessons.CreateLesson;
using ChurchLearn.Api.Features.Lessons.DeleteLesson;
using ChurchLearn.Api.Features.Lessons.DeleteResource;
using ChurchLearn.Api.Features.Lessons.GetCourseLessons;
using ChurchLearn.Api.Features.Lessons.GetLesson;
using ChurchLearn.Api.Features.Lessons.GetLessonResources;
using ChurchLearn.Api.Features.Lessons.ReorderLessons;
using ChurchLearn.Api.Features.Lessons.UpdateLesson;
using FluentValidation;

namespace ChurchLearn.Api.Features.Lessons;

public static class LessonsServiceRegistration
{
    public static IServiceCollection AddLessonsFeature(this IServiceCollection services)
    {
        services.AddScoped<GetCourseLessonsHandler>();
        services.AddScoped<GetLessonHandler>();
        services.AddScoped<CreateLessonHandler>();
        services.AddScoped<UpdateLessonHandler>();
        services.AddScoped<DeleteLessonHandler>();
        services.AddScoped<ReorderLessonsHandler>();
        services.AddScoped<GetLessonResourcesHandler>();
        services.AddScoped<AddResourceHandler>();
        services.AddScoped<DeleteResourceHandler>();

        services.AddScoped<IValidator<CreateLessonRequest>, CreateLessonValidator>();
        services.AddScoped<IValidator<UpdateLessonRequest>, UpdateLessonValidator>();
        services.AddScoped<IValidator<AddResourceRequest>, AddResourceValidator>();

        return services;
    }
}
