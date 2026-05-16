using ChurchLearn.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Features.Users.GetUser;

public record UserDetail(string Id, string Email, string DisplayName, bool IsActive, DateTime CreatedAt, string[] Roles);

public class GetUserHandler(UserManager<AppUser> userManager)
{
    public async Task<UserDetail> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found");

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return new UserDetail(user.Id, user.Email!, user.DisplayName, user.IsActive, user.CreatedAt, roles);
    }
}
