using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.Register;

public record AuthResponse(string AccessToken, string Email, string DisplayName, string[] Roles);

public class RegisterHandler(
    UserManager<AppUser> userManager,
    JwtTokenService jwtTokenService,
    IValidator<RegisterRequest> validator)
{
    public async Task<AuthResponse> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        if (await userManager.FindByEmailAsync(request.Email) is not null)
            throw new InvalidOperationException("User with this email already exists");

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = false,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ArgumentException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, AppRoles.Student);

        var roles = new[] { AppRoles.Student };
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        return new AuthResponse(accessToken, user.Email!, user.DisplayName, roles);
    }
}
