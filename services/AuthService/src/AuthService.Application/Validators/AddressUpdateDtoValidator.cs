using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class AddressUpdateDtoValidator : AbstractValidator<AddressUpdateDto>
{
    public AddressUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.District)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FullAddress)
            .NotEmpty()
            .MaximumLength(500);
    }
}
