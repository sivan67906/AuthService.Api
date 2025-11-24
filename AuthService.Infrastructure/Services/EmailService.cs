using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Services;

/// <summary>
/// SMTP-based email service implementation using EmailSettings from configuration.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _settings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> options)
    {
        _logger = logger;
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = false // use plain connection with AUTH; server may still upgrade via STARTTLS depending on config
        };

        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);

        try
        {
            _logger.LogInformation(
                "Sending email via SMTP to {To}. Host: {Host}, Port: {Port}, EnableSsl: {EnableSsl}",
                to, _settings.SmtpHost, _settings.SmtpPort, client.EnableSsl);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}.", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}.", to);
            throw;
        }
    }
}
