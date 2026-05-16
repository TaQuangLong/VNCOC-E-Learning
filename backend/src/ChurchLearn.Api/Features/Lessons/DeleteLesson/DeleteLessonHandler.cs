using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.DeleteLesson;

public class DeleteLessonHandler(AppDbContext db)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Lesson {id} not found.");

        db.Lessons.Remove(lesson);
        await db.SaveChangesAsync(cancellationToken);
    }
}
