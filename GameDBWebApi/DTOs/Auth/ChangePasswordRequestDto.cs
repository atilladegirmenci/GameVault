namespace GameDBWebApi.DTOs.Auth;

public sealed class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
