using FluentValidation;
using OrderService.Application.DTOs;

namespace OrderService.Application.Validators;

public class CartItemAddDtoValidator : AbstractValidator<CartItemAddDto>
{
    public CartItemAddDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
