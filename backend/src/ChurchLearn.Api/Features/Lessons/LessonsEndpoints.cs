using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Lessons.AddResource;
using ChurchLearn.Api.Features.Lessons.CreateLesson;
using ChurchLearn.Api.Features.Lessons.DeleteLesson;
using ChurchLearn.Api.Features.Lessons.DeleteResource;
using ChurchLearn.Api.Features.Lessons.GetCourseLessons;
using ChurchLearn.Api.Features.Lessons.GetLesson;
using ChurchLearn.Api.Features.Lessons.GetLessonResources;
using ChurchLearn.Api.Features.Lessons.ReorderLessons;
using ChurchLearn.Api.Features.Lessons.UpdateLesson;
using Microsoft.AspNetCore.Mvc;

namespace ChurchLearn.Api.Features.Lessons;

public static class LessonsEndpoints
{
    public static IEndpointRouteBuilder MapLessonsEndpoints(this IEndpointRouteBuilder app)
    {
        MapPublicEndpoints(app);
        MapStudentEndpoints(app);
        MapAdminEndpoints(app);
        return app;
    }

    private static void MapPublicEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Lessons");

        group.MapGet("/{courseId:int}/lessons", async (
            int courseId,
            GetCourseLessonsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, ct);
            return result.ToHttpResult(Results.Ok);
        });
    }

    private static void MapStudentEndpoints(IEndpointRouteBuilder app)
    {
        // No group-level auth — preview lessons are accessible without authentication.
        // The handler enforces enrollment for non-preview lessons.
        var group = app.MapGroup("/api/lessons").WithTags("Lessons");

        group.MapGet("/{id:int}", async (
            int id,
            GetLessonHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return result.ToHttpResult(Results.Ok);
        });
    }

    private static void MapAdminEndpoints(IEndpointRouteBuilder app)
    {
        var courseGroup = app.MapGroup("/api/admin/courses")
            .WithTags("Admin - Lessons")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        courseGroup.MapPost("/{courseId:int}/lessons", async (
            int courseId,
            [FromBody] CreateLessonRequest request,
            CreateLessonHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, request, ct);
            return result.ToHttpResult(r => Results.Created($"/api/lessons/{r.Id}", r));
        });

        courseGroup.MapPut("/{courseId:int}/lessons/order", async (
            int courseId,
            [FromBody] ReorderLessonsRequest request,
            ReorderLessonsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, request, ct);
            return result.ToHttpResult(() => Results.NoContent());
        });

        var lessonGroup = app.MapGroup("/api/admin/lessons")
            .WithTags("Admin - Lessons")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        lessonGroup.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateLessonRequest request,
            UpdateLessonHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, request, ct);
            return result.ToHttpResult(() => Results.NoContent());
        });

        lessonGroup.MapDelete("/{id:int}", async (
            int id,
            DeleteLessonHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return result.ToHttpResult(() => Results.NoContent());
        });

        lessonGroup.MapGet("/{id:int}/resources", async (
            int id,
            GetLessonResourcesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return result.ToHttpResult(Results.Ok);
        });

        lessonGroup.MapPost("/{id:int}/resources", async (
            int id,
            [FromBody] AddResourceRequest request,
            AddResourceHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, request, ct);
            return result.ToHttpResult(r => Results.Created($"/api/admin/resources/{r.Id}", r));
        });

        var resourceGroup = app.MapGroup("/api/admin/resources")
            .WithTags("Admin - Lessons")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        resourceGroup.MapDelete("/{id:int}", async (
            int id,
            DeleteResourceHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return result.ToHttpResult(() => Results.NoContent());
        });
    }
}
