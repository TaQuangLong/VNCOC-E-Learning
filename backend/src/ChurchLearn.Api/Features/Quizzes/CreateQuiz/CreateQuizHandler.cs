using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.CreateQuiz;

public record CreateAnswerOptionDto(string Text, bool IsCorrect, int OrderIndex);

public record CreateQuestionDto(
    string Text,
    QuestionType Type,
    int OrderIndex,
    IReadOnlyList<CreateAnswerOptionDto> Options);

public record CreateQuizRequest(
    string Title,
    string? Description,
    int PassingScore,
    bool IsRequired,
    IReadOnlyList<CreateQuestionDto> Questions);

public record CreateQuizResponse(int Id, string Title, int QuestionCount);

public class CreateQuizValidator : AbstractValidator<CreateQuizRequest>
{
    public CreateQuizValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PassingScore).InclusiveBetween(0, 100);
        RuleFor(x => x.Questions)
            .NotNull()
            .Must(q => q.Count >= 1)
            .WithMessage("A quiz must have at least one question.");
        RuleForEach(x => x.Questions).ChildRules(q =>
        {
            q.RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
            q.RuleFor(x => x.Options)
                .NotNull()
                .Must(o => o.Count >= 2)
                .WithMessage("Each question must have at least two options.");
            q.RuleForEach(x => x.Options).ChildRules(o =>
            {
                o.RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
            });
        });
    }
}

public class CreateQuizHandler(AppDbContext db, IValidator<CreateQuizRequest> validator)
{
    public async Task<Result<CreateQuizResponse>> HandleAsync(
        int lessonId,
        CreateQuizRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<CreateQuizResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var lessonExists = await db.Lessons
            .AsNoTracking()
            .AnyAsync(l => l.Id == lessonId, cancellationToken);

        if (!lessonExists)
            return Result<CreateQuizResponse>.Failure(
                $"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var quizExists = await db.Quizzes
            .AnyAsync(q => q.LessonId == lessonId, cancellationToken);

        if (quizExists)
            return Result<CreateQuizResponse>.Failure(
                $"Lesson {lessonId} already has a quiz.", ErrorCodes.Conflict);

        var quiz = new Quiz
        {
            LessonId = lessonId,
            Title = request.Title,
            Description = request.Description,
            PassingScore = request.PassingScore,
            IsRequired = request.IsRequired,
            Questions = request.Questions.Select(q => new Question
            {
                Text = q.Text,
                Type = q.Type,
                OrderIndex = q.OrderIndex,
                Options = q.Options.Select(o => new AnswerOption
                {
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    OrderIndex = o.OrderIndex,
                }).ToList(),
            }).ToList(),
        };

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(cancellationToken);

        return Result<CreateQuizResponse>.Success(
            new CreateQuizResponse(quiz.Id, quiz.Title, quiz.Questions.Count));
    }
}
