using ChurchLearn.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Users.GetUsers;

public record UserSummary(string Id, string Email, string DisplayName, bool IsActive, string[] Roles);
public record GetUsersResponse(List<UserSummary> Items, int TotalCount, int Page, int PageSize);

public class GetUsersHandler(UserManager<AppUser> userManager)
{
    public async Task<GetUsersResponse> HandleAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = userManager.Users.OrderBy(u => u.Email);
        var total = await query.CountAsync(cancellationToken);
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var summaries = new List<UserSummary>();
        foreach (var user in users)
        {
            var roles = (await userManager.GetRolesAsync(user)).ToArray();
            summaries.Add(new UserSummary(user.Id, user.Email!, user.DisplayName, user.IsActive, roles));
        }

        return new GetUsersResponse(summaries, total, page, pageSize);
    }
}
