using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.GetAuthorById;

public record AuthorDetail(int Id, string Name, string? Bio, string? AvatarUrl, string? UserId, int CourseCount);

public class GetAuthorByIdHandler(AppDbContext db)
{
    public async Task<Result<AuthorDetail>> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var author = await db.Authors
            .Where(a => a.Id == id)
            .Select(a => new AuthorDetail(
                a.Id,
                a.Name,
                a.Bio,
                a.AvatarUrl,
                a.UserId,
                a.Courses.Count))
            .FirstOrDefaultAsync(cancellationToken);

        if (author is null)
            return Result<AuthorDetail>.Failure("Author not found.", ErrorCodes.NotFound);

        return Result<AuthorDetail>.Success(author);
    }
}
