using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.Login;

public record LoginResponse(string AccessToken, string Email, string DisplayName, string[] Roles);

public class LoginHandler(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    JwtTokenService jwtTokenService,
    IValidator<LoginRequest> validator)
{
    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled");

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        return new LoginResponse(accessToken, user.Email!, user.DisplayName, roles);
    }
}
