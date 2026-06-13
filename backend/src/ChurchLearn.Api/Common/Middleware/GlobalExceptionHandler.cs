using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChurchLearn.Api.Common.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IMiddleware
{
    private const string LearningPathSlugIndex = "IX_LearningPaths_Slug";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DbUpdateException ex) when (IsLearningPathSlugConflict(ex))
        {
            logger.LogWarning(ex, "Learning path slug conflict");
            await WriteConflictResponse(context, "A learning path with this slug already exists.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorResponse(context);
        }
    }

    private static bool IsLearningPathSlugConflict(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: LearningPathSlugIndex,
        };

    private static Task WriteConflictResponse(HttpContext context, string error)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error }));
    }

    private static Task WriteErrorResponse(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new
        {
            title = "An unexpected error occurred.",
            status = (int)HttpStatusCode.InternalServerError,
        });
        return context.Response.WriteAsync(body);
    }
}
