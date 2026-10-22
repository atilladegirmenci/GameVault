namespace GameDBWebApi.DTOs.Review
{
    public class CreateReviewRequestDto
    {
        public short Rating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
