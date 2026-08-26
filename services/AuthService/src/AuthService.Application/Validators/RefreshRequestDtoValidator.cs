using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class RefreshRequestDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
