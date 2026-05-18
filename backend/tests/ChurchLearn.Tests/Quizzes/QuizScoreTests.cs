using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Features.Progress.MarkLessonComplete;
using ChurchLearn.Api.Features.Quizzes.SubmitQuiz;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Tests.Quizzes;

public class QuizScoreTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static ICurrentUser FakeUser(string userId = "user-1") =>
        new FakeCurrentUser(userId);

    private static async Task<(Course course, Lesson lesson, Quiz quiz)>
        SeedQuizAsync(AppDbContext db, bool isRequired = false, int passingScore = 60)
    {
        var author = new Author { Name = "Author" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var course = new Course
        {
            Title = "Test Course",
            Slug = $"test-course-{Guid.NewGuid():N}",
            AuthorId = author.Id,
            Status = CourseStatus.Published,
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var lesson = new Lesson
        {
            CourseId = course.Id,
            Title = "Lesson 1",
            ContentType = ContentType.Text,
            OrderIndex = 0,
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync();

        // Q1 — SingleChoice (option A = correct, option B = wrong)
        var q1 = new Question
        {
            QuizId = 0, // will be set via quiz nav
            Text = "What is 1+1?",
            Type = QuestionType.SingleChoice,
            OrderIndex = 0,
            Options =
            [
                new AnswerOption { Text = "2", IsCorrect = true, OrderIndex = 0 },
                new AnswerOption { Text = "3", IsCorrect = false, OrderIndex = 1 },
            ],
        };

        // Q2 — MultipleChoice (options A + C correct, B wrong)
        var q2 = new Question
        {
            QuizId = 0,
            Text = "Which are fruits?",
            Type = QuestionType.MultipleChoice,
            OrderIndex = 1,
            Options =
            [
                new AnswerOption { Text = "Apple", IsCorrect = true, OrderIndex = 0 },
                new AnswerOption { Text = "Carrot", IsCorrect = false, OrderIndex = 1 },
                new AnswerOption { Text = "Mango", IsCorrect = true, OrderIndex = 2 },
            ],
        };

        // Q3 — TrueFalse (True = correct)
        var q3 = new Question
        {
            QuizId = 0,
            Text = "The sky is blue?",
            Type = QuestionType.TrueFalse,
            OrderIndex = 2,
            Options =
            [
                new AnswerOption { Text = "True", IsCorrect = true, OrderIndex = 0 },
                new AnswerOption { Text = "False", IsCorrect = false, OrderIndex = 1 },
            ],
        };

        var quiz = new Quiz
        {
            LessonId = lesson.Id,
            Title = "Test Quiz",
            PassingScore = passingScore,
            IsRequired = isRequired,
            Questions = [q1, q2, q3],
        };
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        return (course, lesson, quiz);
    }

    private static SubmitQuizHandler CreateHandler(AppDbContext db, ICurrentUser user) =>
        new(db, user, new InlineValidator<SubmitQuizRequest>());

    // ── Score: all correct ───────────────────────────────────────────────────

    [Fact]
    public async Task SubmitQuiz_AllCorrect_Returns100Score()
    {
        await using var db = CreateDb();
        var (_, _, quiz) = await SeedQuizAsync(db);

        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.QuizId == quiz.Id)
            .ToListAsync();

        var answers = BuildCorrectAnswers(questions);
        var handler = CreateHandler(db, FakeUser());

        var result = await handler.HandleAsync(
            quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value!.Score);
        Assert.True(result.Value.Passed);
        Assert.Equal(3, result.Value.CorrectCount);
    }

    // ── Score: all wrong ─────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitQuiz_AllWrong_Returns0Score()
    {
        await using var db = CreateDb();
        var (_, _, quiz) = await SeedQuizAsync(db);

        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.QuizId == quiz.Id)
            .ToListAsync();

        var answers = BuildWrongAnswers(questions);
        var handler = CreateHandler(db, FakeUser());

        var result = await handler.HandleAsync(
            quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.Score);
        Assert.False(result.Value.Passed);
        Assert.Equal(0, result.Value.CorrectCount);
    }

    // ── Score: partial ───────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitQuiz_OneOfThreeCorrect_Returns33Score()
    {
        await using var db = CreateDb();
        var (_, _, quiz) = await SeedQuizAsync(db, passingScore: 60);

        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.QuizId == quiz.Id)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();

        // Correct Q1 (single choice), wrong Q2 and Q3
        var q1CorrectOption = questions[0].Options.First(o => o.IsCorrect);
        var q2WrongOption = questions[1].Options.First(o => !o.IsCorrect);
        var q3WrongOption = questions[2].Options.First(o => !o.IsCorrect);

        var answers = new List<SubmitAnswerDto>
        {
            new(questions[0].Id, [q1CorrectOption.Id]),
            new(questions[1].Id, [q2WrongOption.Id]),
            new(questions[2].Id, [q3WrongOption.Id]),
        };

        var handler = CreateHandler(db, FakeUser());
        var result = await handler.HandleAsync(
            quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Math.Round(100m / 3, 2), result.Value!.Score);
        Assert.False(result.Value.Passed);
        Assert.Equal(1, result.Value.CorrectCount);
    }

    // ── Multi-select: partial selection is wrong ─────────────────────────────

    [Fact]
    public async Task SubmitQuiz_MultiSelect_PartialCorrect_IsNotCorrect()
    {
        await using var db = CreateDb();
        var (_, _, quiz) = await SeedQuizAsync(db);

        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.QuizId == quiz.Id)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();

        var multiQ = questions.First(q => q.Type == QuestionType.MultipleChoice);
        var oneCorrectOption = multiQ.Options.First(o => o.IsCorrect);

        // Only select one of the two correct options
        var answers = questions.Select(q =>
            q.Id == multiQ.Id
                ? new SubmitAnswerDto(q.Id, [oneCorrectOption.Id])
                : new SubmitAnswerDto(q.Id, [q.Options.First(o => o.IsCorrect).Id])
        ).ToList();

        var handler = CreateHandler(db, FakeUser());
        var result = await handler.HandleAsync(
            quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var multiResult = result.Value!.Results.First(r => r.QuestionId == multiQ.Id);
        Assert.False(multiResult.IsCorrect);
    }

    // ── Multi-select: selecting extra wrong option is wrong ──────────────────

    [Fact]
    public async Task SubmitQuiz_MultiSelect_CorrectPlusWrongOption_IsNotCorrect()
    {
        await using var db = CreateDb();
        var (_, _, quiz) = await SeedQuizAsync(db);

        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.QuizId == quiz.Id)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();

        var multiQ = questions.First(q => q.Type == QuestionType.MultipleChoice);
        var allCorrectIds = multiQ.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
        var wrongId = multiQ.Options.First(o => !o.IsCorrect).Id;

        // Select all correct AND the wrong one
        var answers = questions.Select(q =>
            q.Id == multiQ.Id
                ? new SubmitAnswerDto(q.Id, [.. allCorrectIds, wrongId])
                : new SubmitAnswerDto(q.Id, [q.Options.First(o => o.IsCorrect).Id])
        ).ToList();

        var handler = CreateHandler(db, FakeUser());
        var result = await handler.HandleAsync(
            quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var multiResult = result.Value!.Results.First(r => r.QuestionId == multiQ.Id);
        Assert.False(multiResult.IsCorrect);
    }

    // ── CorrectOptionIds returned in response ────────────────────────────────

    [Fact]
    public async Task SubmitQuiz_Response_IncludesCorrectOptionIds()
    {
        await using var db = CreateDb();
        var (_, _, quiz) = await SeedQuizAsync(db);

        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.QuizId == quiz.Id)
            .ToListAsync();

        var answers = BuildCorrectAnswers(questions);
        var handler = CreateHandler(db, FakeUser());

        var result = await handler.HandleAsync(
            quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);

        Assert.True(result.IsSuccess);
        foreach (var qResult in result.Value!.Results)
        {
            Assert.NotEmpty(qResult.CorrectOptionIds);
        }
    }

    // ── IsRequired + quiz not passed blocks lesson complete ───────────────────

    [Fact]
    public async Task MarkLessonComplete_RequiredQuizNotPassed_ReturnsForbidden()
    {
        await using var db = CreateDb();
        var (course, lesson, _) = await SeedQuizAsync(db, isRequired: true);

        var enrollment = new Enrollment
        {
            UserId = "user-1",
            CourseId = course.Id,
            TotalLessonsCount = 1,
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        var handler = new MarkLessonCompleteHandler(db, FakeUser());
        var result = await handler.HandleAsync(lesson.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Forbidden, result.ErrorCode);
    }

    // ── IsRequired + quiz passed allows lesson complete ───────────────────────

    [Fact]
    public async Task MarkLessonComplete_RequiredQuizPassed_Succeeds()
    {
        await using var db = CreateDb();
        var (course, lesson, _) = await SeedQuizAsync(db, isRequired: true);

        var enrollment = new Enrollment
        {
            UserId = "user-1",
            CourseId = course.Id,
            TotalLessonsCount = 1,
        };
        db.Enrollments.Add(enrollment);

        var progress = new LessonProgress
        {
            UserId = "user-1",
            CourseId = course.Id,
            LessonId = lesson.Id,
            QuizPassed = true,
        };
        db.LessonProgresses.Add(progress);
        await db.SaveChangesAsync();

        var handler = new MarkLessonCompleteHandler(db, FakeUser());
        var result = await handler.HandleAsync(lesson.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsCompleted);
    }

    // ── Retry: second attempt stores new row ─────────────────────────────────

    [Fact]
    public async Task SubmitQuiz_RetryAllowed_CreatesNewAttemptRow()
    {
        await using var db = CreateDb();
        var (_, _, quiz) = await SeedQuizAsync(db);

        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.QuizId == quiz.Id)
            .ToListAsync();

        var answers = BuildCorrectAnswers(questions);
        var handler = CreateHandler(db, FakeUser());

        await handler.HandleAsync(quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);
        await handler.HandleAsync(quiz.Id, new SubmitQuizRequest(answers), CancellationToken.None);

        var attemptCount = await db.QuizAttempts
            .CountAsync(a => a.QuizId == quiz.Id && a.UserId == "user-1");

        Assert.Equal(2, attemptCount);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static List<SubmitAnswerDto> BuildCorrectAnswers(List<Question> questions) =>
        questions.Select(q => new SubmitAnswerDto(
            q.Id,
            q.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList())).ToList();

    private static List<SubmitAnswerDto> BuildWrongAnswers(List<Question> questions) =>
        questions.Select(q =>
        {
            var wrong = q.Options.Where(o => !o.IsCorrect).Select(o => o.Id).ToList();
            // If all options are correct (edge case), still pick the first
            if (wrong.Count == 0) wrong = [q.Options.First().Id];
            return new SubmitAnswerDto(q.Id, wrong);
        }).ToList();
}

// ── Test double ──────────────────────────────────────────────────────────────

file sealed class FakeCurrentUser(string userId) : ICurrentUser
{
    public string UserId => userId;
    public string Email => "test@example.com";
    public string DisplayName => "Test User";
    public bool IsAuthenticated => true;
    public bool IsInRole(string role) => false;
}
