namespace GameDBWebApi.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;      

    }
}
