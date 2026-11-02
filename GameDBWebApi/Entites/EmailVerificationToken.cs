namespace GameDBWebApi.Entites;

public class EmailVerificationToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; } = null!;
}
