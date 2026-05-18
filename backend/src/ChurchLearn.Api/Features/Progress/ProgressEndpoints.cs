using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Features.Progress.GetCourseProgress;
using ChurchLearn.Api.Features.Progress.MarkLessonComplete;
using ChurchLearn.Api.Features.Progress.SaveVideoProgress;

namespace ChurchLearn.Api.Features.Progress;

public static class ProgressEndpoints
{
    public static IEndpointRouteBuilder MapProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var lessonsGroup = app.MapGroup("/api/lessons")
            .WithTags("Progress")
            .RequireAuthorization();

        lessonsGroup.MapPost("/{lessonId:int}/complete", async (
            int lessonId,
            MarkLessonCompleteHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(lessonId, ct);
            return result.ToHttpResult(Results.Ok);
        });

        lessonsGroup.MapPost("/{lessonId:int}/video-progress", async (
            int lessonId,
            SaveVideoProgressRequest request,
            SaveVideoProgressHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(lessonId, request, ct);
            return result.ToHttpResult(Results.Ok);
        });

        var meGroup = app.MapGroup("/api/me/courses")
            .WithTags("Progress")
            .RequireAuthorization();

        meGroup.MapGet("/{courseId:int}/progress", async (
            int courseId,
            GetCourseProgressHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, ct);
            return result.ToHttpResult(Results.Ok);
        });

        return app;
    }
}
