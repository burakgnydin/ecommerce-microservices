using FluentValidation;
using OrderService.Application.DTOs;

namespace OrderService.Application.Validators;

public class OrderItemCreateDtoValidator : AbstractValidator<OrderItemCreateDto>
{
    public OrderItemCreateDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
