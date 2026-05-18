using ChurchLearn.Api.Domain.Enums;

namespace ChurchLearn.Api.Domain.Entities;

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int OrderIndex { get; set; }

    public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
}
