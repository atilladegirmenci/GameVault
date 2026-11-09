using FluentValidation;
using GameDBWebApi.DTOs.List;

namespace GameDBWebApi.DTOs.Validators
{
    public class AddGameToListRequestDtoValidator : AbstractValidator<AddGameToListRequestDto>
    {
        public AddGameToListRequestDtoValidator()
        {
            RuleFor(x => x.CatalogId)
                .GreaterThan(0)
                .WithMessage("Game catalog ID must be a positive integer.");
        }
    }
}
