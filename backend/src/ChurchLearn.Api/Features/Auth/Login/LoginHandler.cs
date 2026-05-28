using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.Login;

public record LoginResponse(string UserId, string AccessToken, string Email, string DisplayName, string[] Roles);

public class LoginHandler(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    JwtTokenService jwtTokenService,
    IValidator<LoginRequest> validator)
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<LoginResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<LoginResponse>.Failure("Invalid credentials.", ErrorCodes.Unauthorized);

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is disabled.", ErrorCodes.Unauthorized);

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
            return Result<LoginResponse>.Failure("Invalid credentials.", ErrorCodes.Unauthorized);

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        return Result<LoginResponse>.Success(new LoginResponse(user.Id, accessToken, user.Email!, user.DisplayName, roles));
    }
}
