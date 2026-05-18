using ChurchLearn.Api.Common;
using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.UpdateQuestion;

public record UpdateAnswerOptionDto(string Text, bool IsCorrect, int OrderIndex);

public record UpdateQuestionRequest(
    string Text,
    QuestionType Type,
    int OrderIndex,
    IReadOnlyList<UpdateAnswerOptionDto> Options);

public record UpdateQuestionResponse(int Id, string Text, string Type, int OrderIndex);

public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionRequest>
{
    public UpdateQuestionValidator()
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

public class UpdateQuestionHandler(AppDbContext db, IValidator<UpdateQuestionRequest> validator)
{
    public async Task<Result<UpdateQuestionResponse>> HandleAsync(
        int questionId,
        UpdateQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<UpdateQuestionResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var question = await db.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is null)
            return Result<UpdateQuestionResponse>.Failure(
                $"Question {questionId} not found.", ErrorCodes.NotFound);

        question.Text = request.Text;
        question.Type = request.Type;
        question.OrderIndex = request.OrderIndex;

        db.AnswerOptions.RemoveRange(question.Options);
        question.Options = request.Options.Select(o => new AnswerOption
        {
            Text = o.Text,
            IsCorrect = o.IsCorrect,
            OrderIndex = o.OrderIndex,
        }).ToList();

        await db.SaveChangesAsync(cancellationToken);

        return Result<UpdateQuestionResponse>.Success(
            new UpdateQuestionResponse(question.Id, question.Text, question.Type.ToString(), question.OrderIndex));
    }
}
