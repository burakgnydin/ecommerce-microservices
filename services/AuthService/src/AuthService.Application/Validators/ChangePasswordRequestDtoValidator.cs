using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(72).WithMessage("Password cannot exceed 72 characters.")
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
