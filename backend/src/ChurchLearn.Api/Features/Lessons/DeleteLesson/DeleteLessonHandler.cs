using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;

namespace ChurchLearn.Api.Features.Lessons.DeleteLesson;

public class DeleteLessonHandler(AppDbContext db)
{
    public async Task<Result> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync([id], cancellationToken);
        if (lesson is null)
            return Result.Failure($"Lesson {id} not found.", ErrorCodes.NotFound);

        db.Lessons.Remove(lesson);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
