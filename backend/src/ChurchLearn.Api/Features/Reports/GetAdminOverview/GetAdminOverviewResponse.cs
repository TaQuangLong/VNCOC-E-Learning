namespace ChurchLearn.Api.Features.Reports.GetAdminOverview;

public record PopularCourseDto(
    int CourseId,
    string Title,
    string Slug,
    int EnrollmentCount);

public record RecentUserDto(
    string UserId,
    string DisplayName,
    string Email,
    DateTime RegisteredAt);

public record GetAdminOverviewResponse(
    int TotalUsers,
    int TotalPublishedCourses,
    int TotalActiveEnrollments,
    int TotalQuizAttempts,
    int RecentRegistrationsLast7Days,
    IReadOnlyList<PopularCourseDto> MostPopularCourses,
    IReadOnlyList<RecentUserDto> RecentRegistrations);
