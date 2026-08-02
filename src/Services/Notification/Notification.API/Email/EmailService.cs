using Resend;

namespace Notification.API.Email;

public class EmailSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

public class EmailService(IResend resend, EmailSettings settings, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var email = new Resend.EmailMessage
        {
            From = settings.From,
            Subject = message.Subject,
            HtmlBody = message.Body,
        };
        email.To.Add(message.To);

        await resend.EmailSendAsync(email, cancellationToken);

        logger.LogInformation("Email sent to {To} — Subject: {Subject}", message.To, message.Subject);
    }
}
