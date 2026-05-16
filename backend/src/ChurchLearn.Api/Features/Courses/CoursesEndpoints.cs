using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Courses.CreateAuthor;
using ChurchLearn.Api.Features.Courses.CreateCourse;
using ChurchLearn.Api.Features.Courses.DeleteCourse;
using ChurchLearn.Api.Features.Courses.GetAdminCourse;
using ChurchLearn.Api.Features.Courses.GetAdminCourses;
using ChurchLearn.Api.Features.Courses.GetAuthors;
using ChurchLearn.Api.Features.Courses.GetCourseBySlug;
using ChurchLearn.Api.Features.Courses.GetPublishedCourses;
using ChurchLearn.Api.Features.Courses.PublishCourse;
using ChurchLearn.Api.Features.Courses.UnpublishCourse;
using ChurchLearn.Api.Features.Courses.UpdateCourse;
using Microsoft.AspNetCore.Mvc;

namespace ChurchLearn.Api.Features.Courses;

public static class CoursesEndpoints
{
    public static IEndpointRouteBuilder MapCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        MapPublicEndpoints(app);
        MapAdminCourseEndpoints(app);
        MapAdminAuthorEndpoints(app);
        return app;
    }

    private static void MapPublicEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");

        group.MapGet("/", async (
            GetPublishedCoursesHandler handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? level = null,
            [FromQuery] string? title = null,
            CancellationToken ct = default) =>
        {
            var result = await handler.HandleAsync(page, pageSize, category, level, title, ct);
            return Results.Ok(result);
        });

        group.MapGet("/{slug}", async (
            string slug,
            GetCourseBySlugHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(slug, ct);
            return Results.Ok(result);
        });
    }

    private static void MapAdminCourseEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/courses")
            .WithTags("Admin - Courses")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        group.MapGet("/", async (
            GetAdminCoursesHandler handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            [FromQuery] string? title = null,
            CancellationToken ct = default) =>
        {
            var result = await handler.HandleAsync(page, pageSize, status, title, ct);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (
            int id,
            GetAdminCourseHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return Results.Ok(result);
        });

        group.MapPost("/", async (
            [FromBody] CreateCourseRequest request,
            CreateCourseHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/admin/courses/{result.Id}", result);
        });

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateCourseRequest request,
            UpdateCourseHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(id, request, ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (
            int id,
            DeleteCourseHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(id, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:int}/publish", async (
            int id,
            PublishCourseHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(id, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:int}/unpublish", async (
            int id,
            UnpublishCourseHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(id, ct);
            return Results.NoContent();
        });
    }

    private static void MapAdminAuthorEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/authors")
            .WithTags("Admin - Authors")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        group.MapGet("/", async (
            GetAuthorsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return Results.Ok(result);
        });

        group.MapPost("/", async (
            [FromBody] CreateAuthorRequest request,
            CreateAuthorHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/admin/authors/{result.Id}", result);
        });
    }
}
