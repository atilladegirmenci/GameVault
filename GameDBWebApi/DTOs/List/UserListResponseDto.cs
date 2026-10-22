namespace GameDBWebApi.DTOs.List
{
    public class UserListResponseDto
    {
        public int Id { get; set; }

        public string ListTitle { get; set; } = string.Empty;

        public int UserId { get; set; }

        public int GameCount { get; set; }
    }
}
