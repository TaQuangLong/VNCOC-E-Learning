using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPath;
using ChurchLearn.Api.Features.LearningPaths.GetAdminLearningPaths;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.LearningPaths;

public class AdminLearningPathQueryTests
{
    [Fact]
    public async Task GetAdminLearningPaths_FiltersAndReturnsCounts()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        var olderPath = BuildLearningPath(
            "Older Path",
            "older-path",
            LearningPathStatus.Published,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            courses);
        var newerPath = BuildLearningPath(
            "Newer Path",
            "newer-path",
            LearningPathStatus.Published,
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            courses);
        var draftPath = BuildLearningPath(
            "Draft Path",
            "draft-path",
            LearningPathStatus.Draft,
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            courses);
        db.LearningPaths.AddRange(olderPath, newerPath, draftPath);
        await db.SaveChangesAsync();

        var handler = new GetAdminLearningPathsHandler(db);
        var response = await handler.HandleAsync(
            page: 1,
            pageSize: 1,
            status: "published",
            CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(newerPath.Id, item.Id);
        Assert.Equal(2, item.SectionCount);
        Assert.Equal(2, item.CourseCount);
    }

    [Fact]
    public async Task GetAdminLearningPath_ReturnsNestedSectionsAndCoursesInOrder()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        var learningPath = BuildLearningPath(
            "Foundations",
            "foundations",
            LearningPathStatus.Draft,
            DateTime.UtcNow,
            courses,
            reverseInsertionOrder: true);
        var firstSection = learningPath.Sections.Single(section => section.OrderIndex == 0);
        firstSection.Courses.Single().OrderIndex = 1;
        var firstCourse = new LearningPathCourse
        {
            LearningPath = learningPath,
            LearningPathSection = firstSection,
            Course = courses[2],
            OrderIndex = 0,
        };
        firstSection.Courses.Add(firstCourse);
        learningPath.Courses.Add(firstCourse);
        learningPath.ShortDescription = "Start here";
        learningPath.Description = "A complete foundation";
        learningPath.ThumbnailUrl = "https://example.com/path.jpg";
        learningPath.EstimatedDurationLabel = "3 months";
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new GetAdminLearningPathHandler(db);
        var result = await handler.HandleAsync(learningPath.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var detail = result.Value!;
        Assert.Equal("Foundations", detail.Title);
        Assert.Equal("3 months", detail.EstimatedDurationLabel);
        Assert.Equal([0, 1], detail.Sections.Select(section => section.OrderIndex));
        Assert.Equal(
            [courses[2].Id, courses[0].Id, courses[1].Id],
            detail.Sections.SelectMany(section => section.Courses).Select(course => course.CourseId));
        Assert.All(
            detail.Sections.SelectMany(section => section.Courses),
            course => Assert.Equal("Published", course.Status));
    }

    [Fact]
    public async Task GetAdminLearningPath_WhenMissing_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var handler = new GetAdminLearningPathHandler(db);

        var result = await handler.HandleAsync(999, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

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
        string title,
        string slug,
        LearningPathStatus status,
        DateTime createdAt,
        IReadOnlyList<Course> courses,
        bool reverseInsertionOrder = false)
    {
        var learningPath = new LearningPath
        {
            Title = title,
            Slug = slug,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        var sections = new[]
        {
            BuildSection(learningPath, "First", 0, courses[0]),
            BuildSection(learningPath, "Second", 1, courses[1]),
        };

        foreach (var section in reverseInsertionOrder ? sections.Reverse() : sections)
            learningPath.Sections.Add(section);

        return learningPath;
    }

    private static LearningPathSection BuildSection(
        LearningPath learningPath,
        string title,
        int orderIndex,
        Course course)
    {
        var section = new LearningPathSection
        {
            LearningPath = learningPath,
            Title = title,
            OrderIndex = orderIndex,
        };
        var learningPathCourse = new LearningPathCourse
        {
            LearningPath = learningPath,
            LearningPathSection = section,
            Course = course,
            OrderIndex = 0,
        };
        section.Courses.Add(learningPathCourse);
        learningPath.Courses.Add(learningPathCourse);
        return section;
    }
}
