using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
    UserManager<AppUser> userManager,
    JwtTokenService jwtTokenService)
{
    public async Task<string> HandleAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var principal = jwtTokenService.ValidateRefreshToken(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Invalid token claims");

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled");

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return jwtTokenService.GenerateAccessToken(user, roles);
    }
}
