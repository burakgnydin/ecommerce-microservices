using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Enums;
using SutNotificationService = NotificationService.Application.Services.NotificationService;

namespace NotificationService.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly TestLogger _logger = new();
    private readonly SutNotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new SutNotificationService(_logger);
    }

    [Theory]
    [InlineData(NotificationType.OrderCreated, "Siparişiniz alındı.")]
    [InlineData(NotificationType.PaymentSucceeded, "Ödemeniz başarıyla alındı.")]
    [InlineData(NotificationType.PaymentFailed, "Ödemeniz başarısız oldu.")]
    public async Task NotifyAsync_LogsExpectedMessage_ForEachNotificationType(NotificationType type, string expectedMessage)
    {
        var dto = new NotificationRequestDto(Guid.NewGuid(), type);

        await _sut.NotifyAsync(Guid.NewGuid(), dto);

        var message = Assert.Single(_logger.Messages);
        Assert.Contains(expectedMessage, message);
    }

    [Fact]
    public async Task NotifyAsync_Throws_WhenNotificationTypeIsUnsupported()
    {
        var dto = new NotificationRequestDto(Guid.NewGuid(), (NotificationType)999);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.NotifyAsync(Guid.NewGuid(), dto));
    }

    private sealed class TestLogger : ILogger<SutNotificationService>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }
}
