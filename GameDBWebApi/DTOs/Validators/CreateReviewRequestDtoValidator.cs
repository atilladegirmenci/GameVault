using FluentValidation;
using GameDBWebApi.DTOs.Review;

namespace GameDBWebApi.DTOs.Validators
{
    public class CreateReviewRequestDtoValidator : AbstractValidator<CreateReviewRequestDto>
    {
        public CreateReviewRequestDtoValidator()
        {
            RuleFor(x => x.Rating)
                .Must(r => r >= 1 && r <= 5)
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .WithMessage("Review comment is required.")
                .MaximumLength(2000)
                .WithMessage("Review comment cannot exceed 2000 characters.");
        }
    }
}
