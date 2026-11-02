using FluentValidation;
using GameDBWebApi.DTOs.Auth;

namespace GameDBWebApi.DTOs.Validators;

public sealed class ResendVerificationRequestDtoValidator : AbstractValidator<ResendVerificationRequestDto>
{
    public ResendVerificationRequestDtoValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
    }
}
