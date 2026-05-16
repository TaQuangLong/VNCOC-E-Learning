namespace ChurchLearn.Api.Infrastructure.Email;

public class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[NoOp Email] To: {To} | Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
