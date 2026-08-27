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
    }
}
