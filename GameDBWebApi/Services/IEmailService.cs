namespace GameDBWebApi.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string recipientEmail, string recipientName, string verificationUrl);
}
