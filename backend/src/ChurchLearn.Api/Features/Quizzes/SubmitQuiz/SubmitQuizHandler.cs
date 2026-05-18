using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.SubmitQuiz;

public record SubmitAnswerDto(int QuestionId, IReadOnlyList<int> SelectedOptionIds);

public record SubmitQuizRequest(IReadOnlyList<SubmitAnswerDto> Answers);

public record AnswerResultDto(
    int QuestionId,
    bool IsCorrect,
    IReadOnlyList<int> CorrectOptionIds,
    IReadOnlyList<int> SelectedOptionIds);

public record SubmitQuizResponse(
    int AttemptId,
    decimal Score,
    bool Passed,
    int CorrectCount,
    int TotalCount,
    IReadOnlyList<AnswerResultDto> Results);

public class SubmitQuizValidator : AbstractValidator<SubmitQuizRequest>
{
    public SubmitQuizValidator()
    {
        RuleFor(x => x.Answers)
            .NotNull()
            .Must(a => a.Count >= 1)
            .WithMessage("At least one answer is required.");

        RuleForEach(x => x.Answers).ChildRules(a =>
        {
            a.RuleFor(x => x.SelectedOptionIds)
                .NotNull()
                .Must(ids => ids.Count >= 1)
                .WithMessage("Each answer must have at least one selected option.");
        });
    }
}

public class SubmitQuizHandler(
    AppDbContext db,
    ICurrentUser currentUser,
    IValidator<SubmitQuizRequest> validator)
{
    public async Task<Result<SubmitQuizResponse>> HandleAsync(
        int quizId,
        SubmitQuizRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<SubmitQuizResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var quiz = await db.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);

        if (quiz is null)
            return Result<SubmitQuizResponse>.Failure(
                $"Quiz {quizId} not found.", ErrorCodes.NotFound);

        var answeredQuestionIds = request.Answers.Select(a => a.QuestionId).ToHashSet();
        var allQuestionIds = quiz.Questions.Select(q => q.Id).ToHashSet();

        if (!allQuestionIds.SetEquals(answeredQuestionIds))
            return Result<SubmitQuizResponse>.Failure(
                "All questions must be answered.", ErrorCodes.BadRequest);

        var results = new List<AnswerResultDto>();
        int correctCount = 0;

        foreach (var question in quiz.Questions)
        {
            var submitted = request.Answers.First(a => a.QuestionId == question.Id);
            var (isCorrect, correctOptionIds) = EvaluateQuestion(question, submitted);

            if (isCorrect) correctCount++;

            results.Add(new AnswerResultDto(
                QuestionId: question.Id,
                IsCorrect: isCorrect,
                CorrectOptionIds: correctOptionIds,
                SelectedOptionIds: submitted.SelectedOptionIds.ToList()));
        }

        decimal score = quiz.Questions.Count > 0
            ? Math.Round((decimal)correctCount / quiz.Questions.Count * 100, 2)
            : 0;

        bool passed = score >= quiz.PassingScore;

        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            UserId = currentUser.UserId,
            Score = score,
            Passed = passed,
            StartedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow,
        };

        foreach (var answer in request.Answers)
        {
            var questionResult = results.First(r => r.QuestionId == answer.QuestionId);
            foreach (var optionId in answer.SelectedOptionIds)
            {
                attempt.Answers.Add(new QuizAttemptAnswer
                {
                    QuestionId = answer.QuestionId,
                    SelectedAnswerOptionId = optionId,
                    IsCorrect = questionResult.IsCorrect,
                });
            }
        }

        db.QuizAttempts.Add(attempt);

        if (passed)
            await UpdateLessonProgressAsync(quiz.LessonId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return Result<SubmitQuizResponse>.Success(new SubmitQuizResponse(
            AttemptId: attempt.Id,
            Score: score,
            Passed: passed,
            CorrectCount: correctCount,
            TotalCount: quiz.Questions.Count,
            Results: results));
    }

    private static (bool IsCorrect, List<int> CorrectOptionIds) EvaluateQuestion(
        Question question, SubmitAnswerDto submitted)
    {
        var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
        bool isCorrect = question.Type switch
        {
            QuestionType.SingleChoice or QuestionType.TrueFalse =>
                submitted.SelectedOptionIds.Count == 1 &&
                correctIds.Contains(submitted.SelectedOptionIds[0]),

            QuestionType.MultipleChoice =>
                submitted.SelectedOptionIds.Count == correctIds.Count &&
                correctIds.All(id => submitted.SelectedOptionIds.Contains(id)) &&
                submitted.SelectedOptionIds.All(id => correctIds.Contains(id)),

            _ => false,
        };
        return (isCorrect, correctIds);
    }

    private async Task UpdateLessonProgressAsync(int lessonId, CancellationToken cancellationToken)
    {
        var progress = await db.LessonProgresses
            .FirstOrDefaultAsync(
                p => p.UserId == currentUser.UserId && p.LessonId == lessonId,
                cancellationToken);

        if (progress is null)
        {
            var lesson = await db.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken);

            if (lesson is not null)
                db.LessonProgresses.Add(new LessonProgress
                {
                    UserId = currentUser.UserId,
                    CourseId = lesson.CourseId,
                    LessonId = lessonId,
                    QuizPassed = true,
                });
        }
        else
        {
            progress.QuizPassed = true;
        }
    }
}
