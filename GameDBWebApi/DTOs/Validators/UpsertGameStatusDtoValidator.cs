
using FluentValidation;
using GameDBWebApi.DTOs.Game;

namespace GameDBWebApi.DTOs.Validators
{
    public class UpsertGameStatusDtoValidator : AbstractValidator<UpsertGameStatusDto>
    {
        public UpsertGameStatusDtoValidator()
        {
            RuleFor(x => x.RawgId)
                .GreaterThan(0)
                .WithMessage("A valid RAWG game ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Game name is required.")
                .MaximumLength(250)
                .WithMessage("Game name cannot exceed 250 characters.");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("Game image URL is required.")
                .MaximumLength(1000)
                .WithMessage("Game image URL cannot exceed 1000 characters.");

             RuleFor(x => x.Score)
                .InclusiveBetween((short)0, (short)100)
                .WithMessage("Game score must be between 0 and 100.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid game status.");
        }
    }
}
