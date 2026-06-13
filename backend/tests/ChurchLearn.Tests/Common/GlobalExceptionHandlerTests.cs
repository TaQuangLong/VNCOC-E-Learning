using System.Net;
using System.Text.Json;
using ChurchLearn.Api.Common.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace ChurchLearn.Tests.Common;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task InvokeAsync_WhenLearningPathSlugIsDuplicate_ReturnsConflict()
    {
        var context = CreateContext();
        var postgresException = new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation,
            constraintName: "IX_LearningPaths_Slug");
        var exception = new DbUpdateException("Save failed.", postgresException);
        var middleware = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        await middleware.InvokeAsync(context, _ => throw exception);

        Assert.Equal((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        var response = await ReadResponseAsync(context);
        Assert.Equal("A learning path with this slug already exists.", response.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionIsUnhandled_ReturnsInternalServerError()
    {
        var context = CreateContext();
        var middleware = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        await middleware.InvokeAsync(context, _ => throw new InvalidOperationException("Unexpected."));

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

        var response = await ReadResponseAsync(context);
        Assert.Equal(
            "An unexpected error occurred.",
            response.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task InvokeAsync_WhenDifferentUniqueConstraintFails_ReturnsInternalServerError()
    {
        var context = CreateContext();
        var postgresException = new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation,
            constraintName: "IX_Courses_Slug");
        var exception = new DbUpdateException("Save failed.", postgresException);
        var middleware = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        await middleware.InvokeAsync(context, _ => throw exception);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
