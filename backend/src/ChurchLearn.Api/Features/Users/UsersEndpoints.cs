using ChurchLearn.Api.Common.Extensions;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Users.GetUser;
using ChurchLearn.Api.Features.Users.GetUsers;
using ChurchLearn.Api.Features.Users.UpdateUserRoles;
using Microsoft.AspNetCore.Mvc;

namespace ChurchLearn.Api.Features.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));

        group.MapGet("/", async (
            GetUsersHandler handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await handler.HandleAsync(page, pageSize, ct);
            return Results.Ok(result);
        });

        group.MapGet("/{id}", async (
            string id,
            GetUserHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return result.ToHttpResult(Results.Ok);
        });

        group.MapPut("/{id}/roles", async (
            string id,
            [FromBody] UpdateUserRolesRequest request,
            UpdateUserRolesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, request, ct);
            return result.ToHttpResult(() => Results.NoContent());
        })
        .RequireAuthorization(policy => policy.RequireRole(AppRoles.SuperAdmin));

        return app;
    }
}
