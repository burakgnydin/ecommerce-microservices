using FluentValidation;
using OrderService.Application.DTOs;

namespace OrderService.Application.Validators;

public class CartItemQuantityUpdateDtoValidator : AbstractValidator<CartItemQuantityUpdateDto>
{
    public CartItemQuantityUpdateDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
