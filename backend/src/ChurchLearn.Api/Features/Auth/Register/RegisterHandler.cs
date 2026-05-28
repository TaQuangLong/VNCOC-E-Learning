using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.Register;

public record AuthResponse(string UserId, string AccessToken, string Email, string DisplayName, string[] Roles);

public class RegisterHandler(
    UserManager<AppUser> userManager,
    JwtTokenService jwtTokenService,
    IValidator<RegisterRequest> validator)
{
    public async Task<Result<AuthResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<AuthResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Result<AuthResponse>.Failure("User with this email already exists.", ErrorCodes.Conflict);

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = false,
        };

        var identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
            return Result<AuthResponse>.Failure(
                string.Join("; ", identityResult.Errors.Select(e => e.Description)),
                ErrorCodes.Validation);

        await userManager.AddToRoleAsync(user, AppRoles.Student);

        var roles = new[] { AppRoles.Student };
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        return Result<AuthResponse>.Success(new AuthResponse(user.Id, accessToken, user.Email!, user.DisplayName, roles));
    }
}
