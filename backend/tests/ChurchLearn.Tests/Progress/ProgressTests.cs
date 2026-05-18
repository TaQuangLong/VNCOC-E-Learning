using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Progress.GetCourseProgress;
using ChurchLearn.Api.Features.Progress.MarkLessonComplete;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.Progress;

public class ProgressTests
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

    private static async Task<(Course course, Enrollment enrollment, Lesson lesson1, Lesson lesson2)>
        SeedCourseWithLessonsAsync(AppDbContext db, string userId = "user-1")
    {
        var author = new Author { Name = "Test Author" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var course = new Course
        {
            Title = "Faith 101",
            Slug = "faith-101",
            AuthorId = author.Id,
            Status = CourseStatus.Published,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var lesson1 = new Lesson
        {
            CourseId = course.Id,
            Title = "Lesson 1",
            ContentType = ContentType.Text,
            OrderIndex = 0,
        };
        var lesson2 = new Lesson
        {
            CourseId = course.Id,
            Title = "Lesson 2",
            ContentType = ContentType.Text,
            OrderIndex = 1,
        };
        db.Lessons.AddRange(lesson1, lesson2);
        await db.SaveChangesAsync();

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = course.Id,
            TotalLessonsCount = 2,
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        return (course, enrollment, lesson1, lesson2);
    }

    // ── MarkLessonComplete — happy path ──────────────────────────────────────

    [Fact]
    public async Task MarkLessonComplete_FirstLesson_UpdatesProgressTo50Percent()
    {
        await using var db = CreateDb();
        var (_, _, lesson1, _) = await SeedCourseWithLessonsAsync(db);
        var handler = new MarkLessonCompleteHandler(db, FakeUser());

        var result = await handler.HandleAsync(lesson1.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value!.ProgressPercent);
        Assert.Equal(1, result.Value.CompletedLessonsCount);
        Assert.Equal(2, result.Value.TotalLessonsCount);
    }

    [Fact]
    public async Task MarkLessonComplete_BothLessons_UpdatesProgressTo100Percent()
    {
        await using var db = CreateDb();
        var (_, _, lesson1, lesson2) = await SeedCourseWithLessonsAsync(db);
        var handler = new MarkLessonCompleteHandler(db, FakeUser());

        await handler.HandleAsync(lesson1.Id, CancellationToken.None);
        var result = await handler.HandleAsync(lesson2.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value!.ProgressPercent);
        Assert.Equal(2, result.Value.CompletedLessonsCount);
    }

    // ── MarkLessonComplete — idempotency ─────────────────────────────────────

    [Fact]
    public async Task MarkLessonComplete_CalledTwice_DoesNotDoubleCount()
    {
        await using var db = CreateDb();
        var (_, _, lesson1, _) = await SeedCourseWithLessonsAsync(db);
        var handler = new MarkLessonCompleteHandler(db, FakeUser());

        await handler.HandleAsync(lesson1.Id, CancellationToken.None);
        var result = await handler.HandleAsync(lesson1.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.CompletedLessonsCount);
        Assert.Equal(50, result.Value.ProgressPercent);
    }

    // ── MarkLessonComplete — not enrolled ────────────────────────────────────

    [Fact]
    public async Task MarkLessonComplete_WhenNotEnrolled_ReturnsForbidden()
    {
        await using var db = CreateDb();
        var (_, _, lesson1, _) = await SeedCourseWithLessonsAsync(db, "other-user");
        var handler = new MarkLessonCompleteHandler(db, FakeUser("user-1-not-enrolled"));

        var result = await handler.HandleAsync(lesson1.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Forbidden, result.ErrorCode);
    }

    // ── MarkLessonComplete — lesson not found ────────────────────────────────

    [Fact]
    public async Task MarkLessonComplete_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var handler = new MarkLessonCompleteHandler(db, FakeUser());

        var result = await handler.HandleAsync(9999, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

    // ── MarkLessonComplete — updates LastAccessedLessonId ────────────────────

    [Fact]
    public async Task MarkLessonComplete_UpdatesLastAccessedLessonId()
    {
        await using var db = CreateDb();
        var (course, _, lesson1, _) = await SeedCourseWithLessonsAsync(db);
        var handler = new MarkLessonCompleteHandler(db, FakeUser());

        await handler.HandleAsync(lesson1.Id, CancellationToken.None);

        var enrollment = await db.Enrollments
            .FirstAsync(e => e.UserId == "user-1" && e.CourseId == course.Id);
        Assert.Equal(lesson1.Id, enrollment.LastAccessedLessonId);
    }

    // ── GetCourseProgress ────────────────────────────────────────────────────

    [Fact]
    public async Task GetCourseProgress_ReturnsCompletedLessons()
    {
        await using var db = CreateDb();
        var (course, _, lesson1, _) = await SeedCourseWithLessonsAsync(db);
        var user = FakeUser();

        var markHandler = new MarkLessonCompleteHandler(db, user);
        await markHandler.HandleAsync(lesson1.Id, CancellationToken.None);

        var getHandler = new GetCourseProgressHandler(db, user);
        var result = await getHandler.HandleAsync(course.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var completedLesson = result.Value!.Lessons.Single(l => l.LessonId == lesson1.Id);
        Assert.True(completedLesson.IsCompleted);
        Assert.Equal(50, result.Value.ProgressPercent);
    }

    [Fact]
    public async Task GetCourseProgress_WhenNotEnrolled_ReturnsForbidden()
    {
        await using var db = CreateDb();
        var (course, _, _, _) = await SeedCourseWithLessonsAsync(db, "another-user");
        var handler = new GetCourseProgressHandler(db, FakeUser("user-without-enrollment"));

        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Forbidden, result.ErrorCode);
    }

    // ── Progress percentage rounding ─────────────────────────────────────────

    [Theory]
    [InlineData(1, 3, 33)]
    [InlineData(2, 3, 67)]
    [InlineData(3, 3, 100)]
    [InlineData(0, 5, 0)]
    public void CalculateProgressPercent_RoundsCorrectly(
        int completed, int total, int expected)
    {
        var percent = total > 0
            ? (int)Math.Round((double)completed / total * 100)
            : 0;

        Assert.Equal(expected, percent);
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
