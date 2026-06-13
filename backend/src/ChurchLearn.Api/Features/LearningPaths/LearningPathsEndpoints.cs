using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;
using Microsoft.AspNetCore.Mvc;

namespace ChurchLearn.Api.Features.LearningPaths;

public static class LearningPathsEndpoints
{
    public static IEndpointRouteBuilder MapLearningPathsEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/admin/learning-paths")
            .WithTags("Admin - Learning Paths")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        adminGroup.MapPost("/", async (
            [FromBody] CreateLearningPathRequest request,
            CreateLearningPathHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToHttpResult(response =>
                Results.Created($"/api/admin/learning-paths/{response.Id}", response));
        });

        return app;
    }
}
