namespace GameDBWebApi.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string UserMail { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
