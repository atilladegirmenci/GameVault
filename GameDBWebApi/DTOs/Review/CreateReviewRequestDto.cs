using System.ComponentModel.DataAnnotations;

namespace GameDBWebApi.DTOs.Review
{
    public class CreateReviewRequestDto
    {
        [Required]
        [Range(1, 5)]
        public short Rating { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Comment { get; set; } = string.Empty;
    }
}
