namespace ChurchLearn.Api.Domain.Entities;

public class QuizAttemptAnswer
{
    public int Id { get; set; }
    public int QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int SelectedAnswerOptionId { get; set; }
    public AnswerOption SelectedAnswerOption { get; set; } = null!;
    public bool IsCorrect { get; set; }
}
