using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Auth.GetCurrentUser;

public record CurrentUserResponse(string UserId, string Email, string DisplayName, string[] Roles);

public class GetCurrentUserHandler(
    UserManager<AppUser> userManager,
    ICurrentUser currentUser)
{
    public async Task<CurrentUserResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUser.UserId)
            ?? throw new UnauthorizedAccessException("User not found");

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return new CurrentUserResponse(user.Id, user.Email!, user.DisplayName, roles);
    }
}
