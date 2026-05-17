using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Features.Enrollments.EnrollCourse;
using ChurchLearn.Api.Features.Enrollments.GetMyEnrolledCourses;
using ChurchLearn.Api.Features.Enrollments.GetMyEnrollmentStatus;

namespace ChurchLearn.Api.Features.Enrollments;

public static class EnrollmentsEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var courseGroup = app.MapGroup("/api/courses")
            .WithTags("Enrollment")
            .RequireAuthorization();

        courseGroup.MapPost("/{courseId:int}/enroll", async (
            int courseId,
            EnrollCourseHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, ct);
            return result.ToHttpResult(r => Results.Created($"/api/me/courses/{r.CourseId}", r));
        });

        courseGroup.MapGet("/{courseId:int}/enrollment-status", async (
            int courseId,
            GetMyEnrollmentStatusHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, ct);
            return result.ToHttpResult(Results.Ok);
        });

        var meGroup = app.MapGroup("/api/me/courses")
            .WithTags("Enrollment")
            .RequireAuthorization();

        meGroup.MapGet("/", async (
            GetMyEnrolledCoursesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return Results.Ok(result);
        });

        meGroup.MapGet("/{courseId:int}", async (
            int courseId,
            GetMyEnrollmentStatusHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(courseId, ct);
            return result.ToHttpResult(Results.Ok);
        });

        return app;
    }
}
