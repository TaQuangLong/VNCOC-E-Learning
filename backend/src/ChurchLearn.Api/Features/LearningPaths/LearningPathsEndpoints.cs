using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.LearningPaths.ArchiveLearningPath;
using ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPath;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPaths;
using ChurchLearn.Api.Features.LearningPaths.GetLearningPathBySlug;
using ChurchLearn.Api.Features.LearningPaths.GetLearningPaths;
using ChurchLearn.Api.Features.LearningPaths.PublishLearningPath;
using ChurchLearn.Api.Features.LearningPaths.UnpublishLearningPath;
using ChurchLearn.Api.Features.LearningPaths.UpdateLearningPath;
using Microsoft.AspNetCore.Mvc;

namespace ChurchLearn.Api.Features.LearningPaths;

public static class LearningPathsEndpoints
{
    public static IEndpointRouteBuilder MapLearningPathsEndpoints(this IEndpointRouteBuilder app)
    {
        MapPublicEndpoints(app);
        MapAdminEndpoints(app);
        return app;
    }

    private static void MapPublicEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/learning-paths")
            .WithTags("Learning Paths");

        group.MapGet("/", async (
            GetLearningPathsHandler handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.HandleAsync(page, pageSize, cancellationToken);
            return Results.Ok(result);
        });

        group.MapGet("/{slug}", async (
            string slug,
            GetLearningPathBySlugHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(slug, cancellationToken);
            return result.ToHttpResult(Results.Ok);
        });
    }

    private static void MapAdminEndpoints(IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/admin/learning-paths")
            .WithTags("Admin - Learning Paths")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        adminGroup.MapGet("/", async (
            GetAdminLearningPathsHandler handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default) =>
        {
            var result = await handler.HandleAsync(page, pageSize, status, cancellationToken);
            return Results.Ok(result);
        });

        adminGroup.MapGet("/{id:int}", async (
            int id,
            GetAdminLearningPathHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToHttpResult(Results.Ok);
        });

        adminGroup.MapPost("/", async (
            [FromBody] CreateLearningPathRequest request,
            CreateLearningPathHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToHttpResult(response =>
                Results.Created($"/api/admin/learning-paths/{response.Id}", response));
        });

        adminGroup.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateLearningPathRequest request,
            UpdateLearningPathHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, request, cancellationToken);
            return result.ToHttpResult(Results.Ok);
        });

        adminGroup.MapPost("/{id:int}/publish", async (
            int id,
            PublishLearningPathHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToHttpResult(Results.Ok);
        });

        adminGroup.MapPost("/{id:int}/unpublish", async (
            int id,
            UnpublishLearningPathHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToHttpResult(Results.Ok);
        });

        adminGroup.MapDelete("/{id:int}", async (
            int id,
            ArchiveLearningPathHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToHttpResult(() => Results.NoContent());
        });
    }
}
