namespace ChurchLearn.Api.Features.Reports.GetUserProgress;

public record UserQuizResultDto(
    int QuizId,
    string QuizTitle,
    int LessonId,
    string LessonTitle,
    decimal Score,
    bool Passed,
    DateTime SubmittedAt);

public record UserCourseProgressDto(
    int CourseId,
    string CourseTitle,
    string CourseSlug,
    DateTime EnrolledAt,
    int ProgressPercent,
    int CompletedLessonsCount,
    int TotalLessonsCount,
    bool IsCompleted,
    DateTime? CompletedAt,
    IReadOnlyList<UserQuizResultDto> QuizResults);

public record GetUserProgressResponse(
    string UserId,
    string DisplayName,
    string Email,
    IReadOnlyList<UserCourseProgressDto> Courses);
