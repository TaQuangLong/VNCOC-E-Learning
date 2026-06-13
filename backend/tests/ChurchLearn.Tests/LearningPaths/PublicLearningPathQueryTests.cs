using System.Text.Json;
using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.LearningPaths.GetLearningPathBySlug;
using ChurchLearn.Api.Features.LearningPaths.GetLearningPaths;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.LearningPaths;

public class PublicLearningPathQueryTests
{
    [Fact]
    public async Task GetLearningPaths_ReturnsOnlyConsistentPublishedPathsInRequiredOrder()
    {
        await using var db = CreateDb();
        var publishedCourses = await SeedCoursesAsync(db, 3);
        var draftCourse = await SeedCourseAsync(db, "Draft Course", CourseStatus.Draft);
        var createdAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstPath = BuildLearningPath(
            "First Path",
            "first-path",
            LearningPathStatus.Published,
            createdAt,
            [publishedCourses[0]]);
        var secondPath = BuildLearningPath(
            "Second Path",
            "second-path",
            LearningPathStatus.Published,
            createdAt,
            [publishedCourses[1], publishedCourses[2]]);
        var draftPath = BuildLearningPath(
            "Draft Path",
            "draft-path",
            LearningPathStatus.Draft,
            createdAt.AddDays(1),
            [publishedCourses[0]]);
        var inconsistentPath = BuildLearningPath(
            "Inconsistent Path",
            "inconsistent-path",
            LearningPathStatus.Published,
            createdAt.AddDays(2),
            [draftCourse]);
        db.LearningPaths.AddRange(firstPath, secondPath, draftPath, inconsistentPath);
        await db.SaveChangesAsync();

        var handler = new GetLearningPathsHandler(db);
        var response = await handler.HandleAsync(1, 1, CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(secondPath.Id, item.Id);
        Assert.Equal(2, item.CourseCount);
        Assert.Equal(1, response.Page);
        Assert.Equal(1, response.PageSize);
    }

    [Fact]
    public async Task GetLearningPathBySlug_ForGuest_ReturnsOrderedDetailWithoutProgress()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db, 3);
        db.Lessons.AddRange(
            new Lesson { Course = courses[2], Title = "Lesson One", OrderIndex = 0 },
            new Lesson { Course = courses[2], Title = "Lesson Two", OrderIndex = 1 });
        var learningPath = BuildDetailedLearningPath(courses);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new GetLearningPathBySlugHandler(db, GuestUser());
        var result = await handler.HandleAsync(learningPath.Slug, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var detail = result.Value!;
        Assert.Null(detail.Progress);
        Assert.Equal(["Foundations", "Advanced"], detail.Sections.Select(section => section.Title));
        Assert.Equal(
            [courses[2].Id, courses[0].Id, courses[1].Id],
            detail.Sections.SelectMany(section => section.Courses).Select(course => course.Id));
        Assert.Equal(2, detail.Sections[0].Courses[0].LessonCount);
        Assert.All(
            detail.Sections.SelectMany(section => section.Courses),
            course =>
            {
                Assert.Null(course.IsEnrolled);
                Assert.Null(course.ProgressPercent);
                Assert.Null(course.IsCompleted);
            });

        var json = JsonSerializer.Serialize(detail);
        Assert.DoesNotContain("Progress\"", json);
        Assert.DoesNotContain("IsEnrolled", json);
        Assert.DoesNotContain("IsCompleted", json);
    }

    [Fact]
    public async Task GetLearningPathBySlug_ForAuthenticatedUser_ReturnsDerivedProgress()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db, 3);
        var learningPath = BuildLearningPath(
            "Foundations",
            "foundations",
            LearningPathStatus.Published,
            DateTime.UtcNow,
            courses);
        db.LearningPaths.Add(learningPath);
        db.Enrollments.AddRange(
            new Enrollment
            {
                UserId = "student-1",
                Course = courses[0],
                ProgressPercent = 100,
                CompletedAt = DateTime.UtcNow,
            },
            new Enrollment
            {
                UserId = "student-1",
                Course = courses[1],
                ProgressPercent = 40,
            });
        await db.SaveChangesAsync();

        var handler = new GetLearningPathBySlugHandler(db, AuthenticatedUser("student-1"));
        var result = await handler.HandleAsync(learningPath.Slug, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var detail = result.Value!;
        Assert.Equal(1, detail.Progress!.CompletedCoursesCount);
        Assert.Equal(3, detail.Progress.TotalCoursesCount);
        Assert.Equal(33, detail.Progress.ProgressPercent);

        var courseDetails = detail.Sections.SelectMany(section => section.Courses).ToDictionary(course => course.Id);
        Assert.True(courseDetails[courses[0].Id].IsEnrolled);
        Assert.True(courseDetails[courses[0].Id].IsCompleted);
        Assert.Equal(100, courseDetails[courses[0].Id].ProgressPercent);
        Assert.True(courseDetails[courses[1].Id].IsEnrolled);
        Assert.False(courseDetails[courses[1].Id].IsCompleted);
        Assert.Equal(40, courseDetails[courses[1].Id].ProgressPercent);
        Assert.False(courseDetails[courses[2].Id].IsEnrolled);
        Assert.False(courseDetails[courses[2].Id].IsCompleted);
        Assert.Equal(0, courseDetails[courses[2].Id].ProgressPercent);
    }

    [Theory]
    [InlineData(LearningPathStatus.Draft, CourseStatus.Published)]
    [InlineData(LearningPathStatus.Published, CourseStatus.Draft)]
    public async Task GetLearningPathBySlug_WhenUnavailable_ReturnsNotFound(
        LearningPathStatus pathStatus,
        CourseStatus courseStatus)
    {
        await using var db = CreateDb();
        var course = await SeedCourseAsync(db, "Course", courseStatus);
        var learningPath = BuildLearningPath(
            "Unavailable",
            "unavailable",
            pathStatus,
            DateTime.UtcNow,
            [course]);
        db.LearningPaths.Add(learningPath);
        await db.SaveChangesAsync();

        var handler = new GetLearningPathBySlugHandler(db, GuestUser());
        var result = await handler.HandleAsync(learningPath.Slug, CancellationToken.None);

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

    private static ICurrentUser GuestUser() => new FakeCurrentUser(false, string.Empty);

    private static ICurrentUser AuthenticatedUser(string userId) => new FakeCurrentUser(true, userId);

    private static async Task<List<Course>> SeedCoursesAsync(AppDbContext db, int count)
    {
        var courses = Enumerable.Range(1, count)
            .Select(index => new Course
            {
                Title = $"Course {index}",
                Slug = $"course-{index}-{Guid.NewGuid():N}",
                ShortDescription = $"Course {index} summary",
                ThumbnailUrl = $"https://example.com/course-{index}.jpg",
                Level = "Beginner",
                Author = new Author { Name = $"Author {index}" },
                Status = CourseStatus.Published,
            })
            .ToList();
        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();
        return courses;
    }

    private static async Task<Course> SeedCourseAsync(
        AppDbContext db,
        string title,
        CourseStatus status)
    {
        var course = new Course
        {
            Title = title,
            Slug = $"{title.ToLowerInvariant().Replace(' ', '-')}-{Guid.NewGuid():N}",
            Author = new Author { Name = "Author" },
            Status = status,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course;
    }

    private static LearningPath BuildLearningPath(
        string title,
        string slug,
        LearningPathStatus status,
        DateTime createdAt,
        IReadOnlyList<Course> courses)
    {
        var learningPath = new LearningPath
        {
            Title = title,
            Slug = slug,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        var section = new LearningPathSection
        {
            LearningPath = learningPath,
            Title = "Section",
            OrderIndex = 0,
        };

        for (var index = 0; index < courses.Count; index++)
            AddCourse(learningPath, section, courses[index], index);

        learningPath.Sections.Add(section);
        return learningPath;
    }

    private static LearningPath BuildDetailedLearningPath(IReadOnlyList<Course> courses)
    {
        var learningPath = new LearningPath
        {
            Title = "Foundations",
            Slug = "foundations",
            ShortDescription = "Start here",
            Description = "A complete foundation",
            ThumbnailUrl = "https://example.com/path.jpg",
            EstimatedDurationLabel = "3 months",
            Status = LearningPathStatus.Published,
        };
        var advanced = new LearningPathSection
        {
            LearningPath = learningPath,
            Title = "Advanced",
            OrderIndex = 1,
        };
        var foundations = new LearningPathSection
        {
            LearningPath = learningPath,
            Title = "Foundations",
            OrderIndex = 0,
        };
        AddCourse(learningPath, foundations, courses[0], 1);
        AddCourse(learningPath, foundations, courses[2], 0);
        AddCourse(learningPath, advanced, courses[1], 0);
        learningPath.Sections.Add(advanced);
        learningPath.Sections.Add(foundations);
        return learningPath;
    }

    private static void AddCourse(
        LearningPath learningPath,
        LearningPathSection section,
        Course course,
        int orderIndex)
    {
        var pathCourse = new LearningPathCourse
        {
            LearningPath = learningPath,
            LearningPathSection = section,
            Course = course,
            OrderIndex = orderIndex,
        };
        section.Courses.Add(pathCourse);
        learningPath.Courses.Add(pathCourse);
    }
}

file sealed class FakeCurrentUser(bool isAuthenticated, string userId) : ICurrentUser
{
    public string UserId => userId;
    public string Email => isAuthenticated ? "student@example.com" : string.Empty;
    public string DisplayName => isAuthenticated ? "Student" : string.Empty;
    public bool IsAuthenticated => isAuthenticated;
    public bool IsInRole(string role) => false;
}
