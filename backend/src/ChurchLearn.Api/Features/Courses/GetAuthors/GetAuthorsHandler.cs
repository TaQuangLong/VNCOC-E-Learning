using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.GetAuthors;

public record AuthorSummary(int Id, string Name, string? Bio, string? AvatarUrl);

public class GetAuthorsHandler(AppDbContext db)
{
    public async Task<List<AuthorSummary>> HandleAsync(CancellationToken cancellationToken)
    {
        return await db.Authors
            .OrderBy(a => a.Name)
            .Select(a => new AuthorSummary(a.Id, a.Name, a.Bio, a.AvatarUrl))
            .ToListAsync(cancellationToken);
    }
}
