namespace GameDBWebApi.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
