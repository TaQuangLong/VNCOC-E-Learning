using ChurchLearn.Api.Features.Enrollments.EnrollCourse;
using ChurchLearn.Api.Features.Enrollments.GetMyEnrolledCourses;
using ChurchLearn.Api.Features.Enrollments.GetMyEnrollmentStatus;

namespace ChurchLearn.Api.Features.Enrollments;

public static class EnrollmentsServiceRegistration
{
    public static IServiceCollection AddEnrollmentsFeature(this IServiceCollection services)
    {
        services.AddScoped<EnrollCourseHandler>();
        services.AddScoped<GetMyEnrolledCoursesHandler>();
        services.AddScoped<GetMyEnrollmentStatusHandler>();
        return services;
    }
}
