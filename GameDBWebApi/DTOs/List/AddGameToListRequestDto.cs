using System.ComponentModel.DataAnnotations;

namespace GameDBWebApi.DTOs.List
{
    public class AddGameToListRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "RAWG game ID must be positive.")]
        public int RawgId { get; set; }
    }
}
