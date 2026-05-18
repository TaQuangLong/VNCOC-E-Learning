using ChurchLearn.Api.Common;
using ChurchLearn.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChurchLearn.Api.Features.Quizzes.GetLessonQuiz;

public record AnswerOptionDto(int Id, string Text, int OrderIndex);

public record QuestionDto(int Id, string Text, string Type, int OrderIndex, IReadOnlyList<AnswerOptionDto> Options);

public record GetLessonQuizResponse(
    int Id,
    string Title,
    string? Description,
    int PassingScore,
    bool IsRequired,
    IReadOnlyList<QuestionDto> Questions);

public class GetLessonQuizHandler(AppDbContext db)
{
    public async Task<Result<GetLessonQuizResponse>> HandleAsync(
        int lessonId,
        CancellationToken cancellationToken)
    {
        var lessonExists = await db.Lessons
            .AsNoTracking()
            .AnyAsync(l => l.Id == lessonId, cancellationToken);

        if (!lessonExists)
            return Result<GetLessonQuizResponse>.Failure(
                $"Lesson {lessonId} not found.", ErrorCodes.NotFound);

        var quiz = await db.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions.OrderBy(qn => qn.OrderIndex))
                .ThenInclude(qn => qn.Options.OrderBy(o => o.OrderIndex))
            .FirstOrDefaultAsync(q => q.LessonId == lessonId, cancellationToken);

        if (quiz is null)
            return Result<GetLessonQuizResponse>.Failure(
                $"Lesson {lessonId} has no quiz.", ErrorCodes.NotFound);

        var response = new GetLessonQuizResponse(
            Id: quiz.Id,
            Title: quiz.Title,
            Description: quiz.Description,
            PassingScore: quiz.PassingScore,
            IsRequired: quiz.IsRequired,
            Questions: quiz.Questions
                .Select(q => new QuestionDto(
                    Id: q.Id,
                    Text: q.Text,
                    Type: q.Type.ToString(),
                    OrderIndex: q.OrderIndex,
                    Options: q.Options
                        .Select(o => new AnswerOptionDto(o.Id, o.Text, o.OrderIndex))
                        .ToList()))
                .ToList());

        return Result<GetLessonQuizResponse>.Success(response);
    }
}
