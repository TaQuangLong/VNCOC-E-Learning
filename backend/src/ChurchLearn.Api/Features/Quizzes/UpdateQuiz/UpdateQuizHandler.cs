using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.UpdateQuiz;

public record UpdateQuizRequest(string Title, string? Description, int PassingScore, bool IsRequired);

public record UpdateQuizResponse(int Id, string Title, int PassingScore, bool IsRequired);

public class UpdateQuizValidator : AbstractValidator<UpdateQuizRequest>
{
    public UpdateQuizValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PassingScore).InclusiveBetween(0, 100);
    }
}

public class UpdateQuizHandler(AppDbContext db, IValidator<UpdateQuizRequest> validator)
{
    public async Task<Result<UpdateQuizResponse>> HandleAsync(
        int quizId,
        UpdateQuizRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<UpdateQuizResponse>.Failure(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                ErrorCodes.Validation);

        var quiz = await db.Quizzes
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);

        if (quiz is null)
            return Result<UpdateQuizResponse>.Failure(
                $"Quiz {quizId} not found.", ErrorCodes.NotFound);

        quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.PassingScore = request.PassingScore;
        quiz.IsRequired = request.IsRequired;
        quiz.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result<UpdateQuizResponse>.Success(
            new UpdateQuizResponse(quiz.Id, quiz.Title, quiz.PassingScore, quiz.IsRequired));
    }
}
