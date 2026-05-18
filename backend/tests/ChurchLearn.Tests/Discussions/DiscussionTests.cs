using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Discussions.CreateDiscussion;
using ChurchLearn.Api.Features.Discussions.CreateReply;
using ChurchLearn.Api.Features.Discussions.DeleteDiscussion;
using ChurchLearn.Api.Features.Discussions.UpdateDiscussion;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.Discussions;

public class DiscussionTests
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

    private static async Task<(Course course, Lesson lesson)> SeedLessonAsync(AppDbContext db)
    {
        var author = new Author { Name = "Author" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var course = new Course
        {
            Title = "Test Course",
            Slug = $"test-{Guid.NewGuid():N}",
            AuthorId = author.Id,
            Status = CourseStatus.Published,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var lesson = new Lesson
        {
            CourseId = course.Id,
            Title = "Lesson 1",
            ContentType = ContentType.Text,
            OrderIndex = 0,
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync();

        return (course, lesson);
    }

    // ── CreateDiscussion ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateDiscussion_ValidRequest_ReturnsSuccess()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser("user-1");
        IValidator<CreateDiscussionRequest> validator = new CreateDiscussionValidator();

        var handler = new CreateDiscussionHandler(db, user, validator);
        var result = await handler.HandleAsync(lesson.Id, new CreateDiscussionRequest("Hello!"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Hello!", result.Value!.Content);
    }

    [Fact]
    public async Task CreateDiscussion_LessonNotFound_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var user = FakeUser();
        IValidator<CreateDiscussionRequest> validator = new CreateDiscussionValidator();

        var handler = new CreateDiscussionHandler(db, user, validator);
        var result = await handler.HandleAsync(999, new CreateDiscussionRequest("Hi"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task CreateDiscussion_EmptyContent_ReturnsValidationError()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser();
        IValidator<CreateDiscussionRequest> validator = new CreateDiscussionValidator();

        var handler = new CreateDiscussionHandler(db, user, validator);
        var result = await handler.HandleAsync(lesson.Id, new CreateDiscussionRequest(""), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("VALIDATION", result.ErrorCode);
    }

    // ── CreateReply ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateReply_ValidRequest_ReturnsSuccess()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser("user-1");
        IValidator<CreateReplyRequest> replyValidator = new CreateReplyValidator();
        IValidator<CreateDiscussionRequest> discValidator = new CreateDiscussionValidator();

        var parent = new Discussion { LessonId = lesson.Id, UserId = "user-1", Content = "Original post" };
        db.Discussions.Add(parent);
        await db.SaveChangesAsync();

        var handler = new CreateReplyHandler(db, user, replyValidator);
        var result = await handler.HandleAsync(parent.Id, new CreateReplyRequest("My reply"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("My reply", result.Value!.Content);
    }

    [Fact]
    public async Task CreateReply_ToReply_ReturnsBadRequest()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser("user-1");
        IValidator<CreateReplyRequest> validator = new CreateReplyValidator();

        var parent = new Discussion { LessonId = lesson.Id, UserId = "user-1", Content = "Top post" };
        db.Discussions.Add(parent);
        await db.SaveChangesAsync();

        var reply = new Discussion
        {
            LessonId = lesson.Id,
            UserId = "user-1",
            Content = "Reply",
            ParentDiscussionId = parent.Id,
        };
        db.Discussions.Add(reply);
        await db.SaveChangesAsync();

        var handler = new CreateReplyHandler(db, user, validator);
        var result = await handler.HandleAsync(reply.Id, new CreateReplyRequest("Nested reply"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("BAD_REQUEST", result.ErrorCode);
    }

    // ── SoftDelete ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteDiscussion_OwnPost_SoftDeletesPost()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser("user-1");

        var discussion = new Discussion { LessonId = lesson.Id, UserId = "user-1", Content = "Post" };
        db.Discussions.Add(discussion);
        await db.SaveChangesAsync();

        var handler = new DeleteDiscussionHandler(db, user);
        var result = await handler.HandleAsync(discussion.Id, default);

        Assert.True(result.IsSuccess);
        var deleted = await db.Discussions.FindAsync(discussion.Id);
        Assert.True(deleted!.IsDeleted);
        Assert.Equal("user-1", deleted.DeletedBy);
        Assert.NotNull(deleted.DeletedAt);
    }

    [Fact]
    public async Task DeleteDiscussion_OtherUsersPost_ReturnsForbidden()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser("user-2");

        var discussion = new Discussion { LessonId = lesson.Id, UserId = "user-1", Content = "Post" };
        db.Discussions.Add(discussion);
        await db.SaveChangesAsync();

        var handler = new DeleteDiscussionHandler(db, user);
        var result = await handler.HandleAsync(discussion.Id, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("FORBIDDEN", result.ErrorCode);
    }

    // ── UpdateDiscussion ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateDiscussion_OtherUsersPost_ReturnsForbidden()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser("user-2");
        IValidator<UpdateDiscussionRequest> validator = new UpdateDiscussionValidator();

        var discussion = new Discussion { LessonId = lesson.Id, UserId = "user-1", Content = "Post" };
        db.Discussions.Add(discussion);
        await db.SaveChangesAsync();

        var handler = new UpdateDiscussionHandler(db, user, validator);
        var result = await handler.HandleAsync(discussion.Id, new UpdateDiscussionRequest("Edited"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("FORBIDDEN", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateDiscussion_DeletedPost_ReturnsBadRequest()
    {
        await using var db = CreateDb();
        var (_, lesson) = await SeedLessonAsync(db);
        var user = FakeUser("user-1");
        IValidator<UpdateDiscussionRequest> validator = new UpdateDiscussionValidator();

        var discussion = new Discussion
        {
            LessonId = lesson.Id,
            UserId = "user-1",
            Content = "Post",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = "user-1",
        };
        db.Discussions.Add(discussion);
        await db.SaveChangesAsync();

        var handler = new UpdateDiscussionHandler(db, user, validator);
        var result = await handler.HandleAsync(discussion.Id, new UpdateDiscussionRequest("Edited"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("BAD_REQUEST", result.ErrorCode);
    }
}

file sealed class FakeCurrentUser(string userId) : ICurrentUser
{
    public string UserId => userId;
    public string Email => "test@example.com";
    public string DisplayName => "Test User";
    public bool IsAuthenticated => true;
    public bool IsInRole(string role) => false;
}
