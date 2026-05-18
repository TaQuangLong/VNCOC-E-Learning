using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.AddQuestion;

public record AddAnswerOptionDto(string Text, bool IsCorrect, int OrderIndex);

public record AddQuestionRequest(
    string Text,
    QuestionType Type,
    int OrderIndex,
    IReadOnlyList<AddAnswerOptionDto> Options);

public record AddQuestionResponse(int Id, string Text, string Type, int OrderIndex);

public class AddQuestionValidator : AbstractValidator<AddQuestionRequest>
{
    public AddQuestionValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Options)
            .NotNull()
            .Must(o => o.Count >= 2)
            .WithMessage("A question must have at least two options.");
        RuleForEach(x => x.Options).ChildRules(o =>
        {
            o.RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        });
    }
}

public class AddQuestionHandler(AppDbContext db, IValidator<AddQuestionRequest> validator)
{
    public async Task<Result<AddQuestionResponse>> HandleAsync(
        int quizId,
        AddQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<AddQuestionResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var quizExists = await db.Quizzes
            .AnyAsync(q => q.Id == quizId, cancellationToken);

        if (!quizExists)
            return Result<AddQuestionResponse>.Failure(
                $"Quiz {quizId} not found.", ErrorCodes.NotFound);

        var question = new Question
        {
            QuizId = quizId,
            Text = request.Text,
            Type = request.Type,
            OrderIndex = request.OrderIndex,
            Options = request.Options.Select(o => new AnswerOption
            {
                Text = o.Text,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex,
            }).ToList(),
        };

        db.Questions.Add(question);
        await db.SaveChangesAsync(cancellationToken);

        return Result<AddQuestionResponse>.Success(
            new AddQuestionResponse(question.Id, question.Text, question.Type.ToString(), question.OrderIndex));
    }
}
