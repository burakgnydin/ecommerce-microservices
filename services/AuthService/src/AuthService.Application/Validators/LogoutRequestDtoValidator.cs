using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class LogoutRequestDtoValidator : AbstractValidator<LogoutRequestDto>
{
    public LogoutRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
