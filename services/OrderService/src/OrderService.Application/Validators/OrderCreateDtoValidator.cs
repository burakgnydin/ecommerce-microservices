using FluentValidation;
using OrderService.Application.DTOs;

namespace OrderService.Application.Validators;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemCreateDtoValidator());

        RuleFor(x => x.ShippingTitle)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ShippingCity)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ShippingDistrict)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ShippingFullAddress)
            .NotEmpty()
            .MaximumLength(500);
    }
}
