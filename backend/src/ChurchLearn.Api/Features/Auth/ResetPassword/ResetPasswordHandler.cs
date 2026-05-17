using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.ResetPassword;

public class ResetPasswordHandler(
    UserManager<AppUser> userManager,
    IValidator<ResetPasswordRequest> validator)
{
    public async Task<Result> HandleAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure("Invalid or expired token.", ErrorCodes.Validation);

        var identityResult = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!identityResult.Succeeded)
            return Result.Failure(
                string.Join("; ", identityResult.Errors.Select(e => e.Description)),
                ErrorCodes.Validation);

        return Result.Success();
    }
}
