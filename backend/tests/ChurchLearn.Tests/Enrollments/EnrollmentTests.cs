using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Enrollments.EnrollCourse;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.Enrollments;

public class EnrollmentTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static ICurrentUser FakeUser(string userId = "user-1") =>
        new FakeCurrentUser(userId);

    private static async Task<Course> SeedPublishedCourseAsync(AppDbContext db)
    {
        var author = new Author { Name = "Test Author" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var course = new Course
        {
            Title = "Faith Basics",
            Slug = "faith-basics",
            AuthorId = author.Id,
            Status = CourseStatus.Published,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course;
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task EnrollCourse_WhenCourseIsPublished_Succeeds()
    {
        await using var db = CreateDb();
        var course = await SeedPublishedCourseAsync(db);
        var handler = new EnrollCourseHandler(db, FakeUser());

        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(course.Id, result.Value!.CourseId);
    }

    // ── Duplicate enrollment ─────────────────────────────────────────────────

    [Fact]
    public async Task EnrollCourse_WhenAlreadyEnrolled_ReturnsConflict()
    {
        await using var db = CreateDb();
        var course = await SeedPublishedCourseAsync(db);
        var handler = new EnrollCourseHandler(db, FakeUser());

        await handler.HandleAsync(course.Id, CancellationToken.None);

        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
    }

    // ── Course not published ─────────────────────────────────────────────────

    [Fact]
    public async Task EnrollCourse_WhenCourseIsDraft_ReturnsBadRequest()
    {
        await using var db = CreateDb();
        var author = new Author { Name = "Author" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var course = new Course
        {
            Title = "Draft Course",
            Slug = "draft-course",
            AuthorId = author.Id,
            Status = CourseStatus.Draft,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var handler = new EnrollCourseHandler(db, FakeUser());

        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BadRequest, result.ErrorCode);
    }

    // ── Course not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task EnrollCourse_WhenCourseDoesNotExist_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var handler = new EnrollCourseHandler(db, FakeUser());

        var result = await handler.HandleAsync(99999, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

    // ── TotalLessonsCount is set at enrollment time ──────────────────────────

    [Fact]
    public async Task EnrollCourse_SetsTotalLessonsCount()
    {
        await using var db = CreateDb();
        var course = await SeedPublishedCourseAsync(db);

        db.Lessons.AddRange(
            new Lesson { CourseId = course.Id, Title = "L1", OrderIndex = 1 },
            new Lesson { CourseId = course.Id, Title = "L2", OrderIndex = 2 });
        await db.SaveChangesAsync();

        var handler = new EnrollCourseHandler(db, FakeUser());
        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalLessonsCount);
    }
}

// ── Test double ──────────────────────────────────────────────────────────────

file sealed class FakeCurrentUser(string userId) : ICurrentUser
{
    public string UserId => userId;
    public string Email => "test@example.com";
    public string DisplayName => "Test User";
    public bool IsAuthenticated => true;
    public bool IsInRole(string role) => false;
}
