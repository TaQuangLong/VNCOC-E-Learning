using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Courses.GetAdminCourse;

public record AdminCourseDetail(
    int Id,
    string Title,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? ThumbnailUrl,
    string? Category,
    string? Level,
    string? Language,
    int AuthorId,
    string AuthorName,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class GetAdminCourseHandler(AppDbContext db)
{
    public async Task<AdminCourseDetail> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var course = await db.Courses
            .Include(c => c.Author)
            .Where(c => c.Id == id)
            .Select(c => new AdminCourseDetail(
                c.Id, c.Title, c.Slug, c.ShortDescription, c.Description,
                c.ThumbnailUrl, c.Category, c.Level, c.Language,
                c.AuthorId, c.Author.Name, c.Status.ToString(), c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        return course;
    }
}
