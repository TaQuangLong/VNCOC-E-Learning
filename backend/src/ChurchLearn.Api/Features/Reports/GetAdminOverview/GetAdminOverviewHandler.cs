using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Reports.GetAdminOverview;

public class GetAdminOverviewHandler(AppDbContext db)
{
    private const int TopCoursesCount = 5;
    private const int RecentUsersCount = 10;
    private const int RecentDays = 7;

    public async Task<Result<GetAdminOverviewResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-RecentDays);

        var totalUsers = await db.Users
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalPublishedCourses = await db.Courses
            .AsNoTracking()
            .CountAsync(c => c.Status == CourseStatus.Published, cancellationToken);

        var totalActiveEnrollments = await db.Enrollments
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalQuizAttempts = await db.QuizAttempts
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var recentCount = await db.Users
            .AsNoTracking()
            .CountAsync(u => u.CreatedAt >= cutoff, cancellationToken);

        var popularCourses = await db.Enrollments
            .AsNoTracking()
            .GroupBy(e => e.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(TopCoursesCount)
            .Join(
                db.Courses.AsNoTracking(),
                e => e.CourseId,
                c => c.Id,
                (e, c) => new PopularCourseDto(c.Id, c.Title, c.Slug, e.Count))
            .ToListAsync(cancellationToken);

        var recentUsers = await db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(RecentUsersCount)
            .Select(u => new RecentUserDto(
                u.Id,
                u.DisplayName,
                u.Email ?? string.Empty,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        var response = new GetAdminOverviewResponse(
            totalUsers,
            totalPublishedCourses,
            totalActiveEnrollments,
            totalQuizAttempts,
            recentCount,
            popularCourses,
            recentUsers);

        return Result<GetAdminOverviewResponse>.Success(response);
    }
}
