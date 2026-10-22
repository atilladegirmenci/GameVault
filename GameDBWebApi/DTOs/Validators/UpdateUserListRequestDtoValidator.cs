using FluentValidation;
using GameDBWebApi.DTOs.List;

namespace GameDBWebApi.DTOs.Validators
{
    public class UpdateUserListRequestDtoValidator : AbstractValidator<UpdateUserListRequestDto>
    {
        public UpdateUserListRequestDtoValidator()
        {
            RuleFor(x => x.ListTitle)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage("List title is required.")
                .MaximumLength(100)
                .WithMessage("List title cannot exceed 100 characters.");
        }
    }
}
