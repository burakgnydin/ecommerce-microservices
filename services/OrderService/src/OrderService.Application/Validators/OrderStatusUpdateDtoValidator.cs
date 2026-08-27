using FluentValidation;
using OrderService.Application.DTOs;

namespace OrderService.Application.Validators;

public class OrderStatusUpdateDtoValidator : AbstractValidator<OrderStatusUpdateDto>
{
    public OrderStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => status is "Cancelled" or "Paid")
            .WithMessage("Status must be 'Cancelled' or 'Paid'.");
    }
}
