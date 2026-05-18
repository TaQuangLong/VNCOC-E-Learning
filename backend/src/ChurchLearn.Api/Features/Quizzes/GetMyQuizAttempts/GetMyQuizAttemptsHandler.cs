using ChurchLearn.Api.Common;
using ChurchLearn.Api.Common.Interfaces;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.GetMyQuizAttempts;

public record QuizAttemptSummary(
    int Id,
    decimal Score,
    bool Passed,
    DateTime SubmittedAt);

public record GetMyQuizAttemptsResponse(
    int QuizId,
    IReadOnlyList<QuizAttemptSummary> Attempts,
    decimal? BestScore);

public class GetMyQuizAttemptsHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<GetMyQuizAttemptsResponse>> HandleAsync(
        int quizId,
        CancellationToken cancellationToken)
    {
        var quizExists = await db.Quizzes
            .AsNoTracking()
            .AnyAsync(q => q.Id == quizId, cancellationToken);

        if (!quizExists)
            return Result<GetMyQuizAttemptsResponse>.Failure(
                $"Quiz {quizId} not found.", ErrorCodes.NotFound);

        var attempts = await db.QuizAttempts
            .AsNoTracking()
            .Where(a => a.QuizId == quizId && a.UserId == currentUser.UserId)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new QuizAttemptSummary(a.Id, a.Score, a.Passed, a.SubmittedAt))
            .ToListAsync(cancellationToken);

        decimal? bestScore = attempts.Count > 0
            ? attempts.Max(a => a.Score)
            : null;

        return Result<GetMyQuizAttemptsResponse>.Success(
            new GetMyQuizAttemptsResponse(quizId, attempts, bestScore));
    }
}
