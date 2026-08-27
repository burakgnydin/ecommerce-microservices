using FluentValidation;
using OrderService.Application.DTOs;

namespace OrderService.Application.Validators;

// v1 only supports order cancellation; payment-driven transitions (e.g. "Paid")
// are deferred to KAN-24 once payment-service integration lands.
public class OrderStatusUpdateDtoValidator : AbstractValidator<OrderStatusUpdateDto>
{
    public OrderStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status)
            .Equal("Cancelled");
    }
}
