using FluentValidation;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Validators;

public class NotificationRequestDtoValidator : AbstractValidator<NotificationRequestDto>
{
    public NotificationRequestDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}
