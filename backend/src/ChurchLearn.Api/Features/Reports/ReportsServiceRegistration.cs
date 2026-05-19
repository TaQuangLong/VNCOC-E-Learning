using ChurchLearn.Api.Features.Reports.GetAdminOverview;
using ChurchLearn.Api.Features.Reports.GetCourseLearners;
using ChurchLearn.Api.Features.Reports.GetUserProgress;

namespace ChurchLearn.Api.Features.Reports;

public static class ReportsServiceRegistration
{
    public static IServiceCollection AddReportsFeature(this IServiceCollection services)
    {
        services.AddScoped<GetAdminOverviewHandler>();
        services.AddScoped<GetCourseLearnersHandler>();
        services.AddScoped<GetUserProgressHandler>();
        return services;
    }
}
