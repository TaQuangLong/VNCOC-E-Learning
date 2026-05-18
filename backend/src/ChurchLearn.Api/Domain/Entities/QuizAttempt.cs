namespace ChurchLearn.Api.Domain.Entities;

public class QuizAttempt
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public decimal Score { get; set; }
    public bool Passed { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime SubmittedAt { get; set; }

    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}
