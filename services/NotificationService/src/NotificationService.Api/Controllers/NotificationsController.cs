using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using AppNotificationService = NotificationService.Application.Services.NotificationService;

namespace NotificationService.Api.Controllers;

/// <summary>
/// Sends notifications for the authenticated user's own orders.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Tags("Notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppNotificationService _notificationService;

    public NotificationsController(AppNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Records a notification for one of the authenticated user's own orders.
    /// </summary>
    /// <param name="dto">Notification data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Notify(NotificationRequestDto dto, CancellationToken cancellationToken)
    {
        await _notificationService.NotifyAsync(GetUserId(), dto, cancellationToken);
        return Accepted();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
