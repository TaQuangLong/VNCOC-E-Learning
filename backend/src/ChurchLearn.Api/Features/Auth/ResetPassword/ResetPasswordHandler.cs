using ChurchLearn.Api.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.ResetPassword;

public class ResetPasswordHandler(
    UserManager<AppUser> userManager,
    IValidator<ResetPasswordRequest> validator)
{
    public async Task HandleAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new KeyNotFoundException("User not found");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new ArgumentException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
