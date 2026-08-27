using FluentValidation;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Validators;

public class PaymentRequestDtoValidator : AbstractValidator<PaymentRequestDto>
{
    public PaymentRequestDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .Matches(@"^\d{13,19}$")
            .WithMessage("Card number must be 13-19 digits.");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.ExpiryYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("Expiry year cannot be in the past.");

        RuleFor(x => x.Cvv)
            .NotEmpty()
            .Matches(@"^\d{3,4}$")
            .WithMessage("CVV must be 3-4 digits.");

        RuleFor(x => x)
            .Must(NotBeExpired)
            .WithMessage("Card has expired.")
            .OverridePropertyName(nameof(PaymentRequestDto.ExpiryYear));
    }

    private static bool NotBeExpired(PaymentRequestDto dto)
    {
        var now = DateTime.UtcNow;
        return dto.ExpiryYear > now.Year || (dto.ExpiryYear == now.Year && dto.ExpiryMonth >= now.Month);
    }
}
