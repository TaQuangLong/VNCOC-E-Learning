namespace ChurchLearn.Api.Features.Reports.GetCourseLearners;

public record CourseLearnerDto(
    string UserId,
    string DisplayName,
    string Email,
    DateTime EnrolledAt,
    int ProgressPercent,
    int CompletedLessonsCount,
    int TotalLessonsCount,
    int QuizPassedCount,
    DateTime? CompletedAt);

public record GetCourseLearnersResponse(
    int CourseId,
    string CourseTitle,
    IReadOnlyList<CourseLearnerDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
