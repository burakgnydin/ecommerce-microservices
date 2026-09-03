using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class UserUpdateRequestDtoValidator : AbstractValidator<UserUpdateRequestDto>
{
    public UserUpdateRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);
    }
}
