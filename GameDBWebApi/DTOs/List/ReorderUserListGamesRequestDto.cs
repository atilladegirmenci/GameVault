namespace GameDBWebApi.DTOs.List
{
    public class ReorderUserListGamesRequestDto
    {
        public List<int> GameIds { get; set; } = new();
    }
}
