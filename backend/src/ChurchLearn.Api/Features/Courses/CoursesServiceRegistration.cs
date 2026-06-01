using ChurchLearn.Api.Features.Courses.CreateAuthor;
using ChurchLearn.Api.Features.Courses.CreateCourse;
using ChurchLearn.Api.Features.Courses.DeleteAuthor;
using ChurchLearn.Api.Features.Courses.DeleteCourse;
using ChurchLearn.Api.Features.Courses.GetAdminCourse;
using ChurchLearn.Api.Features.Courses.GetAdminCourses;
using ChurchLearn.Api.Features.Courses.GetAuthorById;
using ChurchLearn.Api.Features.Courses.GetAuthors;
using ChurchLearn.Api.Features.Courses.GetCourseBySlug;
using ChurchLearn.Api.Features.Courses.GetPublishedCourses;
using ChurchLearn.Api.Features.Courses.PublishCourse;
using ChurchLearn.Api.Features.Courses.UnpublishCourse;
using ChurchLearn.Api.Features.Courses.UpdateAuthor;
using ChurchLearn.Api.Features.Courses.UpdateCourse;
using FluentValidation;

namespace ChurchLearn.Api.Features.Courses;

public static class CoursesServiceRegistration
{
    public static IServiceCollection AddCoursesFeature(this IServiceCollection services)
    {
        services.AddScoped<GetPublishedCoursesHandler>();
        services.AddScoped<GetCourseBySlugHandler>();
        services.AddScoped<GetAdminCoursesHandler>();
        services.AddScoped<GetAdminCourseHandler>();
        services.AddScoped<CreateCourseHandler>();
        services.AddScoped<UpdateCourseHandler>();
        services.AddScoped<DeleteCourseHandler>();
        services.AddScoped<PublishCourseHandler>();
        services.AddScoped<UnpublishCourseHandler>();
        services.AddScoped<GetAuthorsHandler>();
        services.AddScoped<GetAuthorByIdHandler>();
        services.AddScoped<CreateAuthorHandler>();
        services.AddScoped<UpdateAuthorHandler>();
        services.AddScoped<DeleteAuthorHandler>();

        services.AddScoped<IValidator<CreateCourseRequest>, CreateCourseValidator>();
        services.AddScoped<IValidator<UpdateCourseRequest>, UpdateCourseValidator>();
        services.AddScoped<IValidator<CreateAuthorRequest>, CreateAuthorValidator>();
        services.AddScoped<IValidator<UpdateAuthorRequest>, UpdateAuthorValidator>();

        return services;
    }
}
