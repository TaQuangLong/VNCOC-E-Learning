using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Features.Courses.CreateCourse;
using ChurchLearn.Api.Features.Courses.PublishCourse;
using ChurchLearn.Api.Features.Courses.UnpublishCourse;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.Courses;

public class CourseTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Author> SeedAuthorAsync(AppDbContext db)
    {
        var author = new Author { Name = "Test Author" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();
        return author;
    }

    // ── Slug uniqueness ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCourse_WhenSlugIsUnique_Succeeds()
    {
        await using var db = CreateDb();
        var author = await SeedAuthorAsync(db);
        IValidator<CreateCourseRequest> validator = new CreateCourseValidator();
        var handler = new CreateCourseHandler(db, validator);

        var request = new CreateCourseRequest(
            "Intro to Faith", "intro-to-faith", null, null, null, null, null, null, author.Id);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("intro-to-faith", result.Value!.Slug);
    }

    [Fact]
    public async Task CreateCourse_WhenSlugIsDuplicate_ReturnsConflict()
    {
        await using var db = CreateDb();
        var author = await SeedAuthorAsync(db);
        IValidator<CreateCourseRequest> validator = new CreateCourseValidator();
        var handler = new CreateCourseHandler(db, validator);

        var request = new CreateCourseRequest(
            "Intro to Faith", "intro-to-faith", null, null, null, null, null, null, author.Id);

        await handler.HandleAsync(request, CancellationToken.None);

        var result = await handler.HandleAsync(request, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
    }

    [Fact]
    public async Task CreateCourse_WhenSlugIsNotKebabCase_ReturnsValidationFailure()
    {
        await using var db = CreateDb();
        var author = await SeedAuthorAsync(db);
        IValidator<CreateCourseRequest> validator = new CreateCourseValidator();
        var handler = new CreateCourseHandler(db, validator);

        var request = new CreateCourseRequest(
            "Intro to Faith", "Intro To Faith", null, null, null, null, null, null, author.Id);

        var result = await handler.HandleAsync(request, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
    }

    // ── Publish / Unpublish state transitions ────────────────────────────────

    [Fact]
    public async Task PublishCourse_FromDraft_SetsStatusToPublished()
    {
        await using var db = CreateDb();
        var author = await SeedAuthorAsync(db);
        var course = new Course { Title = "Test", Slug = "test", AuthorId = author.Id };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var handler = new PublishCourseHandler(db);
        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Courses.FindAsync(course.Id);
        Assert.Equal(Api.Domain.Enums.CourseStatus.Published, updated!.Status);
    }

    [Fact]
    public async Task UnpublishCourse_FromPublished_SetsStatusToDraft()
    {
        await using var db = CreateDb();
        var author = await SeedAuthorAsync(db);
        var course = new Course
        {
            Title = "Test",
            Slug = "test",
            AuthorId = author.Id,
            Status = Api.Domain.Enums.CourseStatus.Published
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var handler = new UnpublishCourseHandler(db);
        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = await db.Courses.FindAsync(course.Id);
        Assert.Equal(Api.Domain.Enums.CourseStatus.Draft, updated!.Status);
    }

    [Fact]
    public async Task PublishCourse_WhenArchived_ReturnsConflict()
    {
        await using var db = CreateDb();
        var author = await SeedAuthorAsync(db);
        var course = new Course
        {
            Title = "Test",
            Slug = "test",
            AuthorId = author.Id,
            Status = Api.Domain.Enums.CourseStatus.Archived
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var handler = new PublishCourseHandler(db);
        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
    }

    [Fact]
    public async Task UnpublishCourse_WhenArchived_ReturnsConflict()
    {
        await using var db = CreateDb();
        var author = await SeedAuthorAsync(db);
        var course = new Course
        {
            Title = "Test",
            Slug = "test",
            AuthorId = author.Id,
            Status = Api.Domain.Enums.CourseStatus.Archived
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var handler = new UnpublishCourseHandler(db);
        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
    }
}
