using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

/// <summary>
/// Payload describing a notification to log for the authenticated caller's order.
/// </summary>
/// <param name="OrderId">Id of the order the notification relates to.</param>
/// <param name="Type">Type of event being notified about.</param>
public record NotificationRequestDto(Guid OrderId, NotificationType Type);
