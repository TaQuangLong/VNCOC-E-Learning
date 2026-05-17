using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Lessons.CreateLesson;
using ChurchLearn.Api.Features.Lessons.ReorderLessons;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.Lessons;

public class LessonTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Course> SeedCourseAsync(AppDbContext db)
    {
        var author = new Author { Name = "Test Author" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var course = new Course
        {
            Title = "Test Course",
            Slug = "test-course",
            AuthorId = author.Id,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course;
    }

    // ── CreateLesson ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateLesson_VideoWithValidYouTubeUrl_Succeeds()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);
        IValidator<CreateLessonRequest> validator = new CreateLessonValidator();
        var handler = new CreateLessonHandler(db, validator);

        var request = new CreateLessonRequest(
            "Intro Video", null, ContentType.Video,
            "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            null, null, 213, 0, true);

        var result = await handler.HandleAsync(course.Id, request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Id > 0);
        Assert.Equal("Intro Video", result.Value.Title);
    }

    [Fact]
    public async Task CreateLesson_VideoWithInvalidYouTubeUrl_ReturnsValidationFailure()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);
        IValidator<CreateLessonRequest> validator = new CreateLessonValidator();
        var handler = new CreateLessonHandler(db, validator);

        var request = new CreateLessonRequest(
            "Bad Video", null, ContentType.Video,
            "https://vimeo.com/123456",
            null, null, 100, 0, false);

        var result = await handler.HandleAsync(course.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
    }

    [Fact]
    public async Task CreateLesson_TextWithoutContent_ReturnsValidationFailure()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);
        IValidator<CreateLessonRequest> validator = new CreateLessonValidator();
        var handler = new CreateLessonHandler(db, validator);

        var request = new CreateLessonRequest(
            "Text Lesson", null, ContentType.Text,
            null, null, null, 0, 0, false);

        var result = await handler.HandleAsync(course.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
    }

    [Fact]
    public async Task CreateLesson_PdfWithInvalidUrl_ReturnsValidationFailure()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);
        IValidator<CreateLessonRequest> validator = new CreateLessonValidator();
        var handler = new CreateLessonHandler(db, validator);

        var request = new CreateLessonRequest(
            "PDF Lesson", null, ContentType.Pdf,
            null, null, "not-a-url", 0, 0, false);

        var result = await handler.HandleAsync(course.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
    }

    // ── ReorderLessons ───────────────────────────────────────────────────────

    [Fact]
    public async Task ReorderLessons_UpdatesOrderIndexByPosition()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);

        var lessons = new[]
        {
            new Lesson { CourseId = course.Id, Title = "L1", ContentType = ContentType.Text, TextContent = "a", OrderIndex = 0 },
            new Lesson { CourseId = course.Id, Title = "L2", ContentType = ContentType.Text, TextContent = "b", OrderIndex = 1 },
            new Lesson { CourseId = course.Id, Title = "L3", ContentType = ContentType.Text, TextContent = "c", OrderIndex = 2 },
        };
        db.Lessons.AddRange(lessons);
        await db.SaveChangesAsync();

        var handler = new ReorderLessonsHandler(db);
        // Reverse order: L3, L1, L2
        var request = new ReorderLessonsRequest([lessons[2].Id, lessons[0].Id, lessons[1].Id]);
        var result = await handler.HandleAsync(course.Id, request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Lessons.Where(l => l.CourseId == course.Id).ToListAsync();
        Assert.Equal(0, updated.Single(l => l.Title == "L3").OrderIndex);
        Assert.Equal(1, updated.Single(l => l.Title == "L1").OrderIndex);
        Assert.Equal(2, updated.Single(l => l.Title == "L2").OrderIndex);
    }

    [Fact]
    public async Task ReorderLessons_WithLessonFromDifferentCourse_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);
        var otherCourse = await SeedCourseAsync(db);

        var lesson1 = new Lesson { CourseId = course.Id, Title = "L1", ContentType = ContentType.Text, TextContent = "a", OrderIndex = 0 };
        var foreign = new Lesson { CourseId = otherCourse.Id, Title = "Foreign", ContentType = ContentType.Text, TextContent = "x", OrderIndex = 0 };
        db.Lessons.AddRange(lesson1, foreign);
        await db.SaveChangesAsync();

        var handler = new ReorderLessonsHandler(db);
        var request = new ReorderLessonsRequest([lesson1.Id, foreign.Id]);

        var result = await handler.HandleAsync(course.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task ReorderLessons_WithNonExistentCourse_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var handler = new ReorderLessonsHandler(db);
        var request = new ReorderLessonsRequest([1, 2]);

        var result = await handler.HandleAsync(999, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }
}
