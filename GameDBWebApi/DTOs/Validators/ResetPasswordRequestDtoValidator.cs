using FluentValidation;
using GameDBWebApi.DTOs.Auth;

namespace GameDBWebApi.DTOs.Validators;

public sealed class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Password reset token is required.");
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"(?=.*[A-Za-z])(?=.*\d)")
            .WithMessage("Password must contain at least one letter and one number.");
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }

}
