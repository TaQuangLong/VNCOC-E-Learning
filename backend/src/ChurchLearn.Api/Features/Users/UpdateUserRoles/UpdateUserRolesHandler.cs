using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Users.UpdateUserRoles;

public record UpdateUserRolesRequest(string[] Roles);

public class UpdateUserRolesValidator : AbstractValidator<UpdateUserRolesRequest>
{
    private static readonly string[] AllowedRoles = [AppRoles.Student, AppRoles.Admin, AppRoles.SuperAdmin];

    public UpdateUserRolesValidator()
    {
        RuleFor(x => x.Roles).NotNull().NotEmpty();
        RuleForEach(x => x.Roles).Must(r => AllowedRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", AllowedRoles)}");
    }
}

public class UpdateUserRolesHandler(
    UserManager<AppUser> userManager,
    IValidator<UpdateUserRolesRequest> validator)
{
    public async Task<Result> HandleAsync(string userId, UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure($"User {userId} not found.", ErrorCodes.NotFound);

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRolesAsync(user, request.Roles);
        return Result.Success();
    }
}
