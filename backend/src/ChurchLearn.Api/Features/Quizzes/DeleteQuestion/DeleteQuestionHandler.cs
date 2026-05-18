using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.DeleteQuestion;

public class DeleteQuestionHandler(AppDbContext db)
{
    public async Task<Result<bool>> HandleAsync(
        int questionId,
        CancellationToken cancellationToken)
    {
        var question = await db.Questions
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is null)
            return Result<bool>.Failure(
                $"Question {questionId} not found.", ErrorCodes.NotFound);

        db.Questions.Remove(question);
        await db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
