using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services;

// No interface: this is the only implementation planned (log-based notifications),
// so an abstraction would be premature per the project's YAGNI guideline.
public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(Guid userId, NotificationRequestDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Notification for user {UserId}, order {OrderId}: {Message}",
            userId,
            dto.OrderId,
            BuildMessage(dto.Type));

        return Task.CompletedTask;
    }

    private static string BuildMessage(NotificationType type) => type switch
    {
        NotificationType.OrderCreated => "Siparişiniz alındı.",
        NotificationType.PaymentSucceeded => "Ödemeniz başarıyla alındı.",
        NotificationType.PaymentFailed => "Ödemeniz başarısız oldu.",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported notification type.")
    };
}
