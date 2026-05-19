using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Reports.GetAdminOverview;
using ChurchLearn.Api.Features.Reports.GetCourseLearners;
using ChurchLearn.Api.Features.Reports.GetUserProgress;

namespace ChurchLearn.Api.Features.Reports;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/admin/reports/overview
        app.MapGet("/api/admin/reports/overview", async (
            GetAdminOverviewHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Reports")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        // GET /api/admin/reports/courses/{courseId}/learners
        app.MapGet("/api/admin/reports/courses/{courseId:int}/learners", async (
            int courseId,
            int page,
            int pageSize,
            GetCourseLearnersHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, page, pageSize, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Reports")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        // GET /api/admin/reports/users/{userId}/progress
        app.MapGet("/api/admin/reports/users/{userId}/progress", async (
            string userId,
            GetUserProgressHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(userId, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Reports")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        return app;
    }
}
