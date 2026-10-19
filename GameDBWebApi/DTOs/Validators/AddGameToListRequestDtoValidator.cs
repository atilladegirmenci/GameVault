using FluentValidation;
using GameDBWebApi.DTOs.List;

namespace GameDBWebApi.DTOs.Validators
{
    public class AddGameToListRequestDtoValidator : AbstractValidator<AddGameToListRequestDto>
    {
        public AddGameToListRequestDtoValidator()
        {
            RuleFor(x => x.RawgId)
                .GreaterThan(0)
                .WithMessage("RAWG game ID must be a positive integer.");
        }
    }
}
