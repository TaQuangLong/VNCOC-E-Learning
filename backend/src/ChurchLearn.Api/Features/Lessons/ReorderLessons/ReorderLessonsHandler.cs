using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Lessons.ReorderLessons;

public record ReorderLessonsRequest(List<int> LessonIds);

public class ReorderLessonsHandler(AppDbContext db)
{
    public async Task HandleAsync(int courseId, ReorderLessonsRequest request, CancellationToken cancellationToken)
    {
        var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId, cancellationToken);
        if (!courseExists)
            throw new KeyNotFoundException($"Course {courseId} not found.");

        var lessons = await db.Lessons
            .Where(l => l.CourseId == courseId)
            .ToListAsync(cancellationToken);

        var lessonMap = lessons.ToDictionary(l => l.Id);

        for (var i = 0; i < request.LessonIds.Count; i++)
        {
            var lessonId = request.LessonIds[i];
            if (!lessonMap.TryGetValue(lessonId, out var lesson))
                throw new KeyNotFoundException($"Lesson {lessonId} not found in course {courseId}.");

            lesson.OrderIndex = i;
            lesson.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
