using GameDBWebApi.DTOs.Game;

namespace GameDBWebApi.DTOs.User
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // recent or current user games (no pagination for profile)
        public List<GameCardDto> UserGames { get; set; } = new List<GameCardDto>();
    }
}
