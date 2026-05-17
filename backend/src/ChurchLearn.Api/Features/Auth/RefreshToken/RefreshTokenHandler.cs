using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
    UserManager<AppUser> userManager,
    JwtTokenService jwtTokenService)
{
    public async Task<Result<string>> HandleAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var principal = jwtTokenService.ValidateRefreshToken(refreshToken);
        if (principal is null)
            return Result<string>.Failure("Invalid or expired refresh token.", ErrorCodes.Unauthorized);

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Result<string>.Failure("Invalid token claims.", ErrorCodes.Unauthorized);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<string>.Failure("User not found.", ErrorCodes.Unauthorized);

        if (!user.IsActive)
            return Result<string>.Failure("Account is disabled.", ErrorCodes.Unauthorized);

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return Result<string>.Success(jwtTokenService.GenerateAccessToken(user, roles));
    }
}
