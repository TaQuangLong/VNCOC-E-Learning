using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.LearningPaths;
using ChurchLearn.Api.Features.LearningPaths.CreateLearningPath;
using ChurchLearn.Api.Infrastructure.Identity;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChurchLearn.Tests.LearningPaths;

public class LearningPathEndpointTests
{
    [Fact]
    public async Task CreateLearningPath_WithSectionsAndCourses_ReturnsCreatedAndPersistsStructure()
    {
        await using var application = await LearningPathTestApplication.CreateAsync();
        var courses = await application.SeedCoursesAsync(CourseStatus.Published, CourseStatus.Published);
        var request = ValidRequest(
            "foundations",
            [
                new("Foundations", "Start here", 0, [new(courses[0].Id, 0)]),
                new("Advanced", null, 1, [new(courses[1].Id, 0)]),
            ]);

        using var response = await application.AdminClient.PostAsJsonAsync(
            "/api/admin/learning-paths/",
            request);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created but received {(int)response.StatusCode}: {responseBody}");
        var created = await response.Content.ReadFromJsonAsync<CreateLearningPathResponse>();
        Assert.NotNull(created);
        Assert.Equal("Draft", created.Status);

        await using var scope = application.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var learningPath = await db.LearningPaths
            .Include(path => path.Sections)
                .ThenInclude(section => section.Courses)
            .SingleAsync(path => path.Id == created.Id);

        Assert.Equal("foundations", learningPath.Slug);
        Assert.Equal(LearningPathStatus.Draft, learningPath.Status);
        Assert.Equal([0, 1], learningPath.Sections.OrderBy(section => section.OrderIndex).Select(section => section.OrderIndex));
        Assert.Equal(
            courses.Select(course => course.Id),
            learningPath.Sections
                .OrderBy(section => section.OrderIndex)
                .SelectMany(section => section.Courses.OrderBy(course => course.OrderIndex))
                .Select(course => course.CourseId));
    }

    [Fact]
    public async Task CreateLearningPath_WhenSlugIsDuplicate_ReturnsConflict()
    {
        await using var application = await LearningPathTestApplication.CreateAsync();
        var course = Assert.Single(await application.SeedCoursesAsync(CourseStatus.Published));
        await application.SeedLearningPathAsync("foundations");

        using var response = await application.AdminClient.PostAsJsonAsync(
            "/api/admin/learning-paths/",
            ValidRequest("foundations", [SectionWithCourse(course.Id)]));

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.Conflict,
            $"Expected 409 Conflict but received {(int)response.StatusCode}: {responseBody}");
    }

    [Fact]
    public async Task CreateLearningPath_WithUnpublishedCourse_ReturnsBadRequest()
    {
        await using var application = await LearningPathTestApplication.CreateAsync();
        var course = Assert.Single(await application.SeedCoursesAsync(CourseStatus.Draft));

        using var response = await application.AdminClient.PostAsJsonAsync(
            "/api/admin/learning-paths/",
            ValidRequest("draft-course-path", [SectionWithCourse(course.Id)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLearningPath_WithDuplicateCourseAcrossSections_ReturnsBadRequest()
    {
        await using var application = await LearningPathTestApplication.CreateAsync();
        var course = Assert.Single(await application.SeedCoursesAsync(CourseStatus.Published));
        var request = ValidRequest(
            "duplicate-course-path",
            [
                new("First", null, 0, [new(course.Id, 0)]),
                new("Second", null, 1, [new(course.Id, 0)]),
            ]);

        using var response = await application.AdminClient.PostAsJsonAsync(
            "/api/admin/learning-paths/",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/admin/learning-paths/")]
    [InlineData("GET", "/api/admin/learning-paths/1")]
    [InlineData("POST", "/api/admin/learning-paths/")]
    [InlineData("PUT", "/api/admin/learning-paths/1")]
    [InlineData("POST", "/api/admin/learning-paths/1/publish")]
    [InlineData("POST", "/api/admin/learning-paths/1/unpublish")]
    [InlineData("DELETE", "/api/admin/learning-paths/1")]
    public async Task AdminLearningPathEndpoints_WhenCalledByStudent_ReturnForbidden(
        string method,
        string path)
    {
        await using var application = await LearningPathTestApplication.CreateAsync();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        using var response = await application.StudentClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static CreateLearningPathRequest ValidRequest(
        string slug,
        IReadOnlyList<CreateLearningPathSectionRequest> sections) =>
        new(
            "Foundations",
            slug,
            "A guided learning path",
            "Learn the foundations in order.",
            "https://example.com/path.jpg",
            "6 weeks",
            sections);

    private static CreateLearningPathSectionRequest SectionWithCourse(int courseId) =>
        new("Foundations", null, 0, [new(courseId, 0)]);
}

file sealed class LearningPathTestApplication : IAsyncDisposable
{
    private const string AuthenticationScheme = "Test";
    private readonly WebApplication app;

    private LearningPathTestApplication(WebApplication app)
    {
        this.app = app;
        AdminClient = CreateClient(AppRoles.Admin);
        StudentClient = CreateClient(AppRoles.Student);
    }

    public IServiceProvider Services => app.Services;
    public HttpClient AdminClient { get; }
    public HttpClient StudentClient { get; }

    public static async Task<LearningPathTestApplication> CreateAsync()
    {
        var builder = WebApplication.CreateBuilder();
        var databaseName = Guid.NewGuid().ToString();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
        builder.Services
            .AddAuthentication(AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                AuthenticationScheme,
                _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddLearningPathsFeature();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapLearningPathsEndpoints();
        await app.StartAsync();

        return new LearningPathTestApplication(app);
    }

    public async Task<List<Course>> SeedCoursesAsync(params CourseStatus[] statuses)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var author = new Author { Name = "Test Author" };
        var courses = statuses
            .Select((status, index) => new Course
            {
                Title = $"Course {index + 1}",
                Slug = $"course-{index + 1}-{Guid.NewGuid():N}",
                Author = author,
                Status = status,
            })
            .ToList();
        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();
        return courses;
    }

    public async Task SeedLearningPathAsync(string slug)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.LearningPaths.Add(new LearningPath
        {
            Title = "Existing Path",
            Slug = slug,
        });
        await db.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        AdminClient.Dispose();
        StudentClient.Dispose();
        await app.DisposeAsync();
    }

    private HttpClient CreateClient(string role)
    {
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, role);
        return client;
    }
}

file sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string RoleHeader = "X-Test-Role";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RoleHeader, out var role))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, role.ToString()),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
