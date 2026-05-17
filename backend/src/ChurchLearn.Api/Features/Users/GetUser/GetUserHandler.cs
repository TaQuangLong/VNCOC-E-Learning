using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Users.GetUser;

public record UserDetail(string Id, string Email, string DisplayName, bool IsActive, DateTime CreatedAt, string[] Roles);

public class GetUserHandler(UserManager<AppUser> userManager)
{
    public async Task<Result<UserDetail>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<UserDetail>.Failure($"User {userId} not found.", ErrorCodes.NotFound);

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return Result<UserDetail>.Success(
            new UserDetail(user.Id, user.Email!, user.DisplayName, user.IsActive, user.CreatedAt, roles));
    }
}
