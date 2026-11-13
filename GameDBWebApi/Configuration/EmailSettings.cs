namespace GameDBWebApi.Configuration;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "GameVault";
    public bool EnableSsl { get; set; } = true;
    public int VerificationTokenLifetimeMinutes { get; set; } = 1440;
    public string VerificationUrl { get; set; } = string.Empty;
    public int PasswordResetTokenLifetimeMinutes { get; set; } = 30;
    public string PasswordResetUrl { get; set; } = string.Empty;
}
