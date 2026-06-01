using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.DeleteAuthor;

public class DeleteAuthorHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var author = await db.Authors.FindAsync([id], cancellationToken);
        if (author is null)
            return Result.Failure("Author not found.", ErrorCodes.NotFound);

        var hasCourses = await db.Courses.AnyAsync(c => c.AuthorId == id, cancellationToken);
        if (hasCourses)
            return Result.Failure(
                "This author has assigned courses. Reassign all courses before deleting.",
                ErrorCodes.Conflict);

        db.Authors.Remove(author);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
