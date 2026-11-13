using GameDBWebApi.Configuration;
using GameDBWebApi.Exceptions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GameDBWebApi.Services;

public sealed class MailKitEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailKitEmailService> _logger;


    public MailKitEmailService(
        IOptions<EmailSettings> options,
        ILogger<MailKitEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(
        string recipientEmail,
        string recipientName,
        string verificationUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        message.Subject = "Verify your GameVault email address";
        message.Body = new TextPart("plain")
        {
            Text = $"Hello {recipientName},\n\nVerify your email address by opening this link:\n{verificationUrl}\n\nThis link will expire soon."
        };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", recipientEmail);
            throw new ExternalServiceException("Unable to send the verification email.");
        }
    }

    public async Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        message.Subject = "Reset your GameVault password";
        message.Body = new TextPart("plain")
        {
            Text = $"Hello {recipientName},\n\nReset your password by opening this link:\n{resetUrl}\n\nThis link will expire soon."
        };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", recipientEmail);
            throw new ExternalServiceException("Unable to send the password reset email.");
        }
    }
}
