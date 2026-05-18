using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Quizzes.AddQuestion;
using ChurchLearn.Api.Features.Quizzes.CreateQuiz;
using ChurchLearn.Api.Features.Quizzes.DeleteQuestion;
using ChurchLearn.Api.Features.Quizzes.GetLessonQuiz;
using ChurchLearn.Api.Features.Quizzes.GetMyQuizAttempts;
using ChurchLearn.Api.Features.Quizzes.SubmitQuiz;
using ChurchLearn.Api.Features.Quizzes.UpdateQuestion;
using ChurchLearn.Api.Features.Quizzes.UpdateQuiz;

namespace ChurchLearn.Api.Features.Quizzes;

public static class QuizzesEndpoints
{
    public static IEndpointRouteBuilder MapQuizzesEndpoints(this IEndpointRouteBuilder app)
    {
        // Student — GET lesson quiz (no IsCorrect exposed)
        app.MapGet("/api/lessons/{lessonId:int}/quiz", async (
            int lessonId,
            GetLessonQuizHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(lessonId, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Quizzes")
        .RequireAuthorization();

        // Admin — create quiz for a lesson
        app.MapPost("/api/admin/lessons/{lessonId:int}/quiz", async (
            int lessonId,
            CreateQuizRequest request,
            CreateQuizHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(lessonId, request, ct);
            return result.ToHttpResult(value => Results.Created($"/api/lessons/{lessonId}/quiz", value));
        })
        .WithTags("Quizzes")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        // Admin — update quiz metadata
        app.MapPut("/api/admin/quizzes/{quizId:int}", async (
            int quizId,
            UpdateQuizRequest request,
            UpdateQuizHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(quizId, request, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Quizzes")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        // Admin — add question to quiz
        app.MapPost("/api/admin/quizzes/{quizId:int}/questions", async (
            int quizId,
            AddQuestionRequest request,
            AddQuestionHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(quizId, request, ct);
            return result.ToHttpResult(value => Results.Created($"/api/admin/quizzes/{quizId}/questions/{value.Id}", value));
        })
        .WithTags("Quizzes")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        // Admin — update question
        app.MapPut("/api/admin/questions/{questionId:int}", async (
            int questionId,
            UpdateQuestionRequest request,
            UpdateQuestionHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(questionId, request, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Quizzes")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        // Admin — delete question
        app.MapDelete("/api/admin/questions/{questionId:int}", async (
            int questionId,
            DeleteQuestionHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(questionId, ct);
            return result.ToHttpResult(_ => Results.NoContent());
        })
        .WithTags("Quizzes")
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        // Student — submit quiz attempt
        app.MapPost("/api/quizzes/{quizId:int}/submit", async (
            int quizId,
            SubmitQuizRequest request,
            SubmitQuizHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(quizId, request, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Quizzes")
        .RequireAuthorization();

        // Student — get own attempts
        app.MapGet("/api/quizzes/{quizId:int}/attempts/me", async (
            int quizId,
            GetMyQuizAttemptsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(quizId, ct);
            return result.ToHttpResult(Results.Ok);
        })
        .WithTags("Quizzes")
        .RequireAuthorization();

        return app;
    }
}
