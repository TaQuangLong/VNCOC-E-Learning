using ChurchLearn.Api.Features.Auth.ForgotPassword;
using ChurchLearn.Api.Features.Auth.GetCurrentUser;
using ChurchLearn.Api.Features.Auth.Login;
using ChurchLearn.Api.Features.Auth.RefreshToken;
using ChurchLearn.Api.Features.Auth.Register;
using ChurchLearn.Api.Features.Auth.ResetPassword;
using ChurchLearn.Api.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ChurchLearn.Api.Features.Auth;

public static class AuthEndpoints
{
    private const string RefreshTokenCookieName = "refresh_token";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            RegisterHandler handler,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(request, ct);
            SetRefreshTokenCookie(httpContext);
            return Results.Ok(response);
        })
        .RequireRateLimiting("auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            LoginHandler handler,
            JwtTokenService jwtTokenService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(request, ct);
            var refreshToken = jwtTokenService.GenerateRefreshToken(response.Email);
            SetRefreshTokenCookie(httpContext, refreshToken);
            return Results.Ok(response);
        })
        .RequireRateLimiting("auth");

        group.MapPost("/logout", (HttpContext httpContext) =>
        {
            httpContext.Response.Cookies.Delete(RefreshTokenCookieName);
            return Results.NoContent();
        }).RequireAuthorization();

        group.MapPost("/refresh", async (
            HttpContext httpContext,
            RefreshTokenHandler handler,
            CancellationToken ct) =>
        {
            var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
            if (string.IsNullOrEmpty(refreshToken))
                return Results.Unauthorized();

            var accessToken = await handler.HandleAsync(refreshToken, ct);
            return Results.Ok(new { accessToken });
        });

        group.MapGet("/me", async (
            GetCurrentUserHandler handler,
            HttpContext httpContext,
            JwtTokenService jwtTokenService,
            CancellationToken ct) =>
        {
            // If no bearer token but refresh cookie exists, issue a new access token
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
                if (!string.IsNullOrEmpty(refreshToken))
                    return Results.Unauthorized(); // Let client use /refresh endpoint
                return Results.Unauthorized();
            }

            var user = await handler.HandleAsync(ct);
            return Results.Ok(user);
        }).RequireAuthorization();

        group.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            ForgotPasswordHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(request, ct);
            return Results.Ok(new { message = "If the email exists, a reset link has been sent." });
        })
        .RequireRateLimiting("auth");

        group.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            ResetPasswordHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(request, ct);
            return Results.Ok(new { message = "Password has been reset." });
        });

        return app;
    }

    private static void SetRefreshTokenCookie(HttpContext httpContext, string? token = null)
    {
        if (token is null) return;
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
        });
    }
}
