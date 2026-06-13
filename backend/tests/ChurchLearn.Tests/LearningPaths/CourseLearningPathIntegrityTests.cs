using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Courses.DeleteCourse;
using ChurchLearn.Api.Features.Courses.UnpublishCourse;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.LearningPaths;

public class CourseLearningPathIntegrityTests
{
    [Fact]
    public async Task UnpublishCourse_ReturnsAffectedPublishedPathsToDraft()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);
        var publishedPathOne = BuildLearningPath(course, LearningPathStatus.Published);
        var publishedPathTwo = BuildLearningPath(course, LearningPathStatus.Published);
        var draftPath = BuildLearningPath(course, LearningPathStatus.Draft);
        var archivedPath = BuildLearningPath(course, LearningPathStatus.Archived);
        db.LearningPaths.AddRange(publishedPathOne, publishedPathTwo, draftPath, archivedPath);
        await db.SaveChangesAsync();

        var handler = new UnpublishCourseHandler(db);
        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CourseStatus.Draft, course.Status);
        Assert.Equal(LearningPathStatus.Draft, publishedPathOne.Status);
        Assert.Equal(LearningPathStatus.Draft, publishedPathTwo.Status);
        Assert.Equal(LearningPathStatus.Draft, draftPath.Status);
        Assert.Equal(LearningPathStatus.Archived, archivedPath.Status);
        Assert.All(
            new[] { publishedPathOne, publishedPathTwo, draftPath, archivedPath },
            path => Assert.Single(path.Courses));
    }

    [Fact]
    public async Task ArchiveCourse_ReturnsAffectedPublishedPathsToDraft()
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db);
        var publishedPath = BuildLearningPath(course, LearningPathStatus.Published);
        var draftPath = BuildLearningPath(course, LearningPathStatus.Draft);
        db.LearningPaths.AddRange(publishedPath, draftPath);
        await db.SaveChangesAsync();

        var handler = new DeleteCourseHandler(db);
        var result = await handler.HandleAsync(course.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CourseStatus.Archived, course.Status);
        Assert.Equal(LearningPathStatus.Draft, publishedPath.Status);
        Assert.Equal(LearningPathStatus.Draft, draftPath.Status);
        Assert.Single(publishedPath.Courses);
        Assert.Single(draftPath.Courses);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Course> SeedCourseAsync(AppDbContext db)
    {
        var course = new Course
        {
            Title = "Course",
            Slug = $"course-{Guid.NewGuid():N}",
            Author = new Author { Name = "Test Author" },
            Status = CourseStatus.Published,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course;
    }

    private static LearningPath BuildLearningPath(
        Course course,
        LearningPathStatus status)
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
