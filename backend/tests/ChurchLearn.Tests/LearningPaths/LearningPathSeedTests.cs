using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ChurchLearn.Tests.LearningPaths;

public class LearningPathSeedTests
{
    private const string UnavailableCourseSlug = "foundations-of-spiritual-growth";

    private static readonly string[] RequiredCourseSlugs =
    [
        "foundations-of-spiritual-growth",
        "core-christian-doctrines",
        "life-of-christ-the-four-gospels",
        "the-art-of-prayer",
        "healthy-relationships-in-community",
        "sharing-your-faith",
        "walking-through-the-old-testament",
        "systematic-theology-essentials",
        "the-early-church-100-500-ad",
        "reformation-and-modern-christianity",
        "servant-leadership-in-the-church",
        "growing-your-ministry-team",
        "raising-faith-filled-families",
    ];

    [Fact]
    public async Task SeedLearningPaths_FreshDataset_CreatesConfiguredPublishedPaths()
    {
        await using var db = CreateDb();
        await SeedCoursesAsync(db);

        await DatabaseSeeder.SeedLearningPathsAsync(db, CancellationToken.None);

        var paths = await LoadPathsAsync(db);
        Assert.Equal(3, paths.Count);
        AssertPath(
            paths,
            "christian-foundations",
            "Christian Foundations",
            [
                ("Start Here",
                [
                    "foundations-of-spiritual-growth",
                    "core-christian-doctrines",
                    "life-of-christ-the-four-gospels",
                ]),
                ("Practices for Everyday Faith",
                [
                    "the-art-of-prayer",
                    "healthy-relationships-in-community",
                    "sharing-your-faith",
                ]),
            ]);
        AssertPath(
            paths,
            "bible-and-theology",
            "Bible & Theology",
            [
                ("The Biblical Story",
                [
                    "walking-through-the-old-testament",
                    "life-of-christ-the-four-gospels",
                ]),
                ("Christian Doctrine",
                [
                    "core-christian-doctrines",
                    "systematic-theology-essentials",
                ]),
                ("Church Through the Ages",
                [
                    "the-early-church-100-500-ad",
                    "reformation-and-modern-christianity",
                ]),
            ]);
        AssertPath(
            paths,
            "ministry-leadership",
            "Ministry Leadership",
            [
                ("Lead Like Jesus",
                [
                    "servant-leadership-in-the-church",
                    "growing-your-ministry-team",
                ]),
                ("Serve People Well",
                [
                    "healthy-relationships-in-community",
                    "sharing-your-faith",
                    "raising-faith-filled-families",
                ]),
            ]);
    }

    [Fact]
    public async Task SeedLearningPaths_WhenRunRepeatedly_DoesNotCreateDuplicates()
    {
        await using var db = CreateDb();
        await SeedCoursesAsync(db);

        await DatabaseSeeder.SeedLearningPathsAsync(db, CancellationToken.None);
        await DatabaseSeeder.SeedLearningPathsAsync(db, CancellationToken.None);

        Assert.Equal(3, await db.LearningPaths.CountAsync());
        Assert.Equal(7, await db.LearningPathSections.CountAsync());
        Assert.Equal(17, await db.LearningPathCourses.CountAsync());
    }

    [Fact]
    public async Task SeedLearningPaths_WhenSlugExists_PreservesExistingPath()
    {
        await using var db = CreateDb();
        var courses = await SeedCoursesAsync(db);
        var existingCourse = courses[UnavailableCourseSlug];
        var createdAt = new DateTime(2026, 1, 1, 2, 3, 4, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var existingPath = BuildEditedPath(existingCourse, createdAt, updatedAt);
        db.LearningPaths.Add(existingPath);
        await db.SaveChangesAsync();

        await DatabaseSeeder.SeedLearningPathsAsync(db, CancellationToken.None);
        db.ChangeTracker.Clear();

        var preserved = await db.LearningPaths
            .AsNoTracking()
            .Include(path => path.Sections)
            .ThenInclude(section => section.Courses)
            .SingleAsync(path => path.Slug == "christian-foundations");

        Assert.Equal("Administrator Title", preserved.Title);
        Assert.Equal("Administrator summary", preserved.ShortDescription);
        Assert.Equal("Administrator description", preserved.Description);
        Assert.Equal("https://example.com/administrator-path.jpg", preserved.ThumbnailUrl);
        Assert.Equal("Administrator duration", preserved.EstimatedDurationLabel);
        Assert.Equal(LearningPathStatus.Draft, preserved.Status);
        Assert.Equal(createdAt, preserved.CreatedAt);
        Assert.Equal(updatedAt, preserved.UpdatedAt);
        var section = Assert.Single(preserved.Sections);
        Assert.Equal("Administrator Section", section.Title);
        Assert.Equal(existingCourse.Id, Assert.Single(section.Courses).CourseId);
        Assert.Equal(3, await db.LearningPaths.CountAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SeedLearningPaths_WhenRequiredCourseUnavailable_SkipsEntirePath(
        bool courseExistsAsDraft)
    {
        await using var db = CreateDb();
        await SeedCoursesAsync(
            db,
            unavailableCourseSlug: UnavailableCourseSlug,
            includeUnavailableAsDraft: courseExistsAsDraft);

        await DatabaseSeeder.SeedLearningPathsAsync(db, CancellationToken.None);

        Assert.False(await db.LearningPaths.AnyAsync(
            path => path.Slug == "christian-foundations"));
        Assert.Equal(2, await db.LearningPaths.CountAsync());
        Assert.Equal(5, await db.LearningPathSections.CountAsync());
        Assert.Equal(11, await db.LearningPathCourses.CountAsync());
    }

    [Fact]
    public void ShouldSeedDemoData_InProductionWithFlagDisabled_ReturnsFalse()
    {
        var environment = new TestWebHostEnvironment
        {
            EnvironmentName = Environments.Production,
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:DemoData"] = "false",
            })
            .Build();

        var shouldSeed = DatabaseSeeder.ShouldSeedDemoData(environment, configuration);

        Assert.False(shouldSeed);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Dictionary<string, Course>> SeedCoursesAsync(
        AppDbContext db,
        string? unavailableCourseSlug = null,
        bool includeUnavailableAsDraft = false)
    {
        var author = new Author { Name = "Seed Test Author" };
        var courses = RequiredCourseSlugs
            .Where(slug => slug != unavailableCourseSlug || includeUnavailableAsDraft)
            .Select(slug => new Course
            {
                Title = slug,
                Slug = slug,
                Author = author,
                Status = slug == unavailableCourseSlug
                    ? CourseStatus.Draft
                    : CourseStatus.Published,
            })
            .ToDictionary(course => course.Slug);
        db.Courses.AddRange(courses.Values);
        await db.SaveChangesAsync();
        return courses;
    }

    private static async Task<List<LearningPath>> LoadPathsAsync(AppDbContext db)
    {
        db.ChangeTracker.Clear();
        return await db.LearningPaths
            .AsNoTracking()
            .Include(path => path.Sections)
            .ThenInclude(section => section.Courses)
            .ThenInclude(pathCourse => pathCourse.Course)
            .OrderBy(path => path.Slug)
            .ToListAsync();
    }

    private static void AssertPath(
        IReadOnlyCollection<LearningPath> paths,
        string slug,
        string title,
        IReadOnlyList<(string Title, string[] CourseSlugs)> expectedSections)
    {
        var path = Assert.Single(paths, candidate => candidate.Slug == slug);
        Assert.Equal(title, path.Title);
        Assert.Equal(LearningPathStatus.Published, path.Status);
        Assert.False(string.IsNullOrWhiteSpace(path.ShortDescription));
        Assert.False(string.IsNullOrWhiteSpace(path.Description));

        var sections = path.Sections.OrderBy(section => section.OrderIndex).ToList();
        Assert.Equal(Enumerable.Range(0, expectedSections.Count), sections.Select(section => section.OrderIndex));
        Assert.Equal(expectedSections.Select(section => section.Title), sections.Select(section => section.Title));

        for (var index = 0; index < expectedSections.Count; index++)
        {
            var courses = sections[index].Courses
                .OrderBy(course => course.OrderIndex)
                .ToList();
            Assert.Equal(
                Enumerable.Range(0, expectedSections[index].CourseSlugs.Length),
                courses.Select(course => course.OrderIndex));
            Assert.Equal(
                expectedSections[index].CourseSlugs,
                courses.Select(course => course.Course.Slug));
        }
    }

    private static LearningPath BuildEditedPath(
        Course course,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var path = new LearningPath
        {
            Title = "Administrator Title",
            Slug = "christian-foundations",
            ShortDescription = "Administrator summary",
            Description = "Administrator description",
            ThumbnailUrl = "https://example.com/administrator-path.jpg",
            EstimatedDurationLabel = "Administrator duration",
            Status = LearningPathStatus.Draft,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
        };
        var section = new LearningPathSection
        {
            LearningPath = path,
            Title = "Administrator Section",
            OrderIndex = 0,
        };
        var pathCourse = new LearningPathCourse
        {
            LearningPath = path,
            LearningPathSection = section,
            Course = course,
            OrderIndex = 0,
        };
        section.Courses.Add(pathCourse);
        path.Sections.Add(section);
        path.Courses.Add(pathCourse);
        return path;
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ChurchLearn.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
