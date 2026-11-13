namespace GameDBWebApi.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(
        string recipientEmail,
        string recipientName,
        string verificationUrl,
        CancellationToken cancellationToken);
    Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken);
}
