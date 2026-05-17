using System.Net;
using System.Text.Json;

namespace ChurchLearn.Api.Common.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorResponse(context);
        }
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
