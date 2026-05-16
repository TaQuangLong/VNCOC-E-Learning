using System.Net;
using System.Net.Mail;

namespace ChurchLearn.Api.Infrastructure.Email;

public class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost required");
        var port = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        var user = configuration["Email:Username"] ?? throw new InvalidOperationException("Email:Username required");
        var pass = configuration["Email:Password"] ?? throw new InvalidOperationException("Email:Password required");
        var from = configuration["Email:FromAddress"] ?? user;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true,
        };

        var message = new MailMessage(from, to, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(message, cancellationToken);
    }
}
