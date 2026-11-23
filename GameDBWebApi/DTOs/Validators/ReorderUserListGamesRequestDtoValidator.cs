using FluentValidation;
using GameDBWebApi.DTOs.List;

namespace GameDBWebApi.DTOs.Validators
{
    public class ReorderUserListGamesRequestDtoValidator
        : AbstractValidator<ReorderUserListGamesRequestDto>
    {
        public ReorderUserListGamesRequestDtoValidator()
        {
            RuleFor(request => request.GameIds)
                .NotNull()
                .WithMessage("Game IDs are required.");

            RuleForEach(request => request.GameIds)
                .GreaterThan(0)
                .WithMessage("Each game ID must be a positive integer.");
        }
    }
}
