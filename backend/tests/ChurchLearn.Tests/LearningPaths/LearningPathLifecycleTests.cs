using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.LearningPaths.ArchiveLearningPath;
using ChurchLearn.Api.Features.LearningPaths.PublishLearningPath;
using ChurchLearn.Api.Features.LearningPaths.UnpublishLearningPath;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.LearningPaths;

public class LearningPathLifecycleTests
{
    [Fact]
    public async Task PublishLearningPath_WithPublishedCourses_SetsStatusToPublished()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db, CourseStatus.Published);
        var learningPath = BuildLearningPath(course);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new PublishLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Published", result.Value!.Status);
        Assert.Equal(LearningPathStatus.Published, learningPath.Status);
    }

    [Fact]
    public async Task PublishLearningPath_WhenEmpty_ReturnsValidationFailure()
    {
        await using var db = CreateDb();
        var learningPath = new LearningPath
        {
            Title = "Empty Path",
            Slug = "empty-path",
        };
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new PublishLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
        Assert.Equal(LearningPathStatus.Draft, learningPath.Status);
    }

    [Fact]
    public async Task PublishLearningPath_WithUnpublishedCourse_ReturnsValidationFailure()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db, CourseStatus.Draft);
        var learningPath = BuildLearningPath(course);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new PublishLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
        Assert.Contains(course.Id.ToString(), result.Error);
        Assert.Equal(LearningPathStatus.Draft, learningPath.Status);
    }

    [Fact]
    public async Task PublishLearningPath_WhenArchived_ReturnsConflict()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db, CourseStatus.Published);
        var learningPath = BuildLearningPath(course, LearningPathStatus.Archived);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new PublishLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
        Assert.Equal(LearningPathStatus.Archived, learningPath.Status);
    }

    [Fact]
    public async Task UnpublishLearningPath_FromPublished_SetsStatusToDraft()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db, CourseStatus.Published);
        var learningPath = BuildLearningPath(course, LearningPathStatus.Published);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new UnpublishLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Draft", result.Value!.Status);
        Assert.Equal(LearningPathStatus.Draft, learningPath.Status);
    }

    [Fact]
    public async Task UnpublishLearningPath_WhenArchived_ReturnsConflict()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db, CourseStatus.Published);
        var learningPath = BuildLearningPath(course, LearningPathStatus.Archived);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new UnpublishLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
        Assert.Equal(LearningPathStatus.Archived, learningPath.Status);
    }

    [Fact]
    public async Task ArchiveLearningPath_SetsStatusToArchived()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db, CourseStatus.Published);
        var learningPath = BuildLearningPath(course, LearningPathStatus.Published);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new ArchiveLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(LearningPathStatus.Archived, learningPath.Status);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Course> SeedCourseAsync(
        AppDbContext db,
        CourseStatus status)
    {
        var course = new Course
        {
            Title = "Course",
            Slug = $"course-{Guid.NewGuid():N}",
            Author = new Author { Name = "Test Author" },
            Status = status,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course;
    }

    private static LearningPath BuildLearningPath(
        Course course,
        LearningPathStatus status = LearningPathStatus.Draft)
    {
        var learningPath = new LearningPath
        {
            Title = "Learning Path",
            Slug = $"learning-path-{Guid.NewGuid():N}",
            Status = status,
        };
        var section = new LearningPathSection
        {
            LearningPath = learningPath,
            Title = "Section",
            OrderIndex = 0,
        };
        var pathCourse = new LearningPathCourse
        {
            LearningPath = learningPath,
            LearningPathSection = section,
            Course = course,
            OrderIndex = 0,
        };
        section.Courses.Add(pathCourse);
        learningPath.Sections.Add(section);
        learningPath.Courses.Add(pathCourse);
        return learningPath;
    }
}
