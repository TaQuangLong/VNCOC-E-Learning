using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.LearningPaths.UpdateLearningPath;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.LearningPaths;

public class UpdateLearningPathTests
{
    [Fact]
    public async Task UpdateLearningPath_ReplacesFieldsSectionsAndCourses()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        var learningPath = BuildLearningPath(courses[0]);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();
        var originalSectionId = learningPath.Sections.Single().Id;

        var handler = CreateHandler(db);
        var request = new UpdateLearningPathRequest(
            "Updated Path",
            "updated-path",
            "Updated summary",
            "Updated description",
            "https://example.com/updated.jpg",
            "6 weeks",
            [
                new("Advanced", null, 1, [new(courses[2].Id, 0)]),
                new("Foundations", "Begin here", 0, [new(courses[1].Id, 0)]),
            ]);

        var result = await handler.HandleAsync(learningPath.Id, request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("updated-path", result.Value!.Slug);

        db.ChangeTracker.Clear();
        var updated = await db.LearningPaths
            .Include(path => path.Sections)
                .ThenInclude(section => section.Courses)
            .SingleAsync(path => path.Id == learningPath.Id);

        Assert.Equal("Updated Path", updated.Title);
        Assert.Equal("6 weeks", updated.EstimatedDurationLabel);
        Assert.DoesNotContain(updated.Sections, section => section.Id == originalSectionId);
        Assert.Equal([0, 1], updated.Sections.OrderBy(section => section.OrderIndex).Select(section => section.OrderIndex));
        Assert.Equal(
            [courses[1].Id, courses[2].Id],
            updated.Sections
                .OrderBy(section => section.OrderIndex)
                .SelectMany(section => section.Courses.OrderBy(course => course.OrderIndex))
                .Select(course => course.CourseId));
    }

    [Fact]
    public async Task UpdateLearningPath_WhenArchived_ReturnsConflictAndLeavesPathUnchanged()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        var learningPath = BuildLearningPath(courses[0], LearningPathStatus.Archived);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var request = ValidRequest(courses[1].Id);

        var result = await handler.HandleAsync(learningPath.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
        Assert.Equal("Original Path", learningPath.Title);
        Assert.Equal(courses[0].Id, learningPath.Courses.Single().CourseId);
    }

    [Fact]
    public async Task UpdateLearningPath_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            999,
            ValidRequest(courses[0].Id),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateLearningPath_WhenSlugBelongsToAnotherPath_ReturnsConflict()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        var learningPath = BuildLearningPath(courses[0]);
        var otherPath = BuildLearningPath(courses[1]);
        otherPath.Slug = "taken-path";
        db.LearningPaths.AddRange(learningPath, otherPath);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var request = ValidRequest(courses[2].Id) with { Slug = "taken-path" };

        var result = await handler.HandleAsync(learningPath.Id, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Conflict, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateLearningPath_WithUnpublishedCourse_ReturnsValidationFailure()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        courses[1].Status = CourseStatus.Draft;
        var learningPath = BuildLearningPath(courses[0]);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            learningPath.Id,
            ValidRequest(courses[1].Id),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Validation, result.ErrorCode);
        Assert.Contains(courses[1].Id.ToString(), result.Error);
    }

    private static UpdateLearningPathHandler CreateHandler(AppDbContext db)
    {
        IValidator<UpdateLearningPathRequest> validator = new UpdateLearningPathValidator();
        return new UpdateLearningPathHandler(db, validator);
    }

    private static UpdateLearningPathRequest ValidRequest(int courseId) =>
        new(
            "Updated Path",
            "updated-path",
            null,
            null,
            null,
            null,
            [new("Section", null, 0, [new(courseId, 0)])]);

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<List<Course>> SeedCoursesAsync(AppDbContext db)
    {
        var author = new Author { Name = "Test Author" };
        var courses = new List<Course>
        {
            new()
            {
                Title = "Course One",
                Slug = "course-one",
                Author = author,
                Status = CourseStatus.Published,
            },
            new()
            {
                Title = "Course Two",
                Slug = "course-two",
                Author = author,
                Status = CourseStatus.Published,
            },
            new()
            {
                Title = "Course Three",
                Slug = "course-three",
                Author = author,
                Status = CourseStatus.Published,
            },
        };
        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();
        return courses;
    }

    private static LearningPath BuildLearningPath(
        Course course,
        LearningPathStatus status = LearningPathStatus.Draft)
    {
        var learningPath = new LearningPath
        {
            Title = "Original Path",
            Slug = $"original-path-{Guid.NewGuid():N}",
            Status = status,
        };
        var section = new LearningPathSection
        {
            LearningPath = learningPath,
            Title = "Original Section",
            OrderIndex = 0,
        };
        var learningPathCourse = new LearningPathCourse
        {
            LearningPath = learningPath,
            LearningPathSection = section,
            Course = course,
            OrderIndex = 0,
        };
        section.Courses.Add(learningPathCourse);
        learningPath.Sections.Add(section);
        learningPath.Courses.Add(learningPathCourse);
        return learningPath;
    }
}
