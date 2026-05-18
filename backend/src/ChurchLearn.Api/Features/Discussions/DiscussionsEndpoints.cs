using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Discussions.AdminDeleteDiscussion;
using ChurchLearn.Api.Features.Discussions.CreateDiscussion;
using ChurchLearn.Api.Features.Discussions.CreateReply;
using ChurchLearn.Api.Features.Discussions.DeleteDiscussion;
using ChurchLearn.Api.Features.Discussions.GetDiscussionReplies;
using ChurchLearn.Api.Features.Discussions.GetLessonDiscussions;
using ChurchLearn.Api.Features.Discussions.UpdateDiscussion;

namespace ChurchLearn.Api.Features.Discussions;

public static class DiscussionsEndpoints
{
    public static IEndpointRouteBuilder MapDiscussionsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET top-level posts for a lesson (paginated)
        app.MapGet("/api/lessons/{lessonId:int}/discussions", async (
            int lessonId,
            int page,
            int pageSize,
            GetLessonDiscussionsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(lessonId, page, pageSize, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Discussions")
        .RequireAuthorization();

        // POST create top-level post
        app.MapPost("/api/lessons/{lessonId:int}/discussions", async (
            int lessonId,
            CreateDiscussionRequest request,
            CreateDiscussionHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(lessonId, request, ct);
            return result.ToHttpResult(value =>
                Results.Created($"/api/lessons/{lessonId}/discussions/{value.Id}", value));
        })
        .WithTags("Discussions")
        .RequireAuthorization();

        // GET replies for a post (paginated)
        app.MapGet("/api/discussions/{discussionId:int}/replies", async (
            int discussionId,
            int page,
            int pageSize,
            GetDiscussionRepliesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(discussionId, page, pageSize, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Discussions")
        .RequireAuthorization();

        // POST reply to a post
        app.MapPost("/api/discussions/{discussionId:int}/reply", async (
            int discussionId,
            CreateReplyRequest request,
            CreateReplyHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(discussionId, request, ct);
            return result.ToHttpResult(value =>
                Results.Created($"/api/discussions/{discussionId}/replies/{value.Id}", value));
        })
        .WithTags("Discussions")
        .RequireAuthorization();

        // PUT edit own post
        app.MapPut("/api/discussions/{discussionId:int}", async (
            int discussionId,
            UpdateDiscussionRequest request,
            UpdateDiscussionHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(discussionId, request, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Discussions")
        .RequireAuthorization();

        // DELETE own post (soft delete)
        app.MapDelete("/api/discussions/{discussionId:int}", async (
            int discussionId,
            DeleteDiscussionHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(discussionId, ct);
            return result.ToHttpResult(_ => Results.NoContent());
        })
        .WithTags("Discussions")
        .RequireAuthorization();

        // Admin — soft delete any post
        app.MapDelete("/api/admin/discussions/{discussionId:int}", async (
            int discussionId,
            AdminDeleteDiscussionHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(discussionId, ct);
            return result.ToHttpResult(_ => Results.NoContent());
        })
        .WithTags("Discussions")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        return app;
    }
}
