using Moq;
using PaymentService.Application.DTOs;
using PaymentService.Application.Exceptions;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using SutPaymentService = PaymentService.Application.Services.PaymentService;

namespace PaymentService.UnitTests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IOrderClient> _orderClient = new();
    private readonly Mock<INotificationClient> _notificationClient = new();
    private readonly SutPaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new SutPaymentService(_paymentRepository.Object, _orderClient.Object, _notificationClient.Object);
    }

    private static PaymentRequestDto CreateRequest(Guid orderId, string cardNumber = "4111111111111234")
        => new(orderId, cardNumber, 12, DateTime.UtcNow.Year + 1, "123");

    private static OrderInfo CreatePendingOrder(Guid orderId, Guid userId, decimal totalAmount = 19.98m)
        => new(orderId, userId, "Pending", totalAmount);

    [Fact]
    public async Task ChargeAsync_ReturnsSucceededPayment_AndMarksOrderAsPaid_WhenCardIsApproved()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = CreatePendingOrder(orderId, userId);
        var bearerToken = "test-token";
        _orderClient.Setup(c => c.GetOrderAsync(orderId, bearerToken, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _sut.ChargeAsync(userId, CreateRequest(orderId), bearerToken);

        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(order.TotalAmount, result.Amount);
        Assert.Equal("**** **** **** 1234", result.MaskedCardNumber);
        _paymentRepository.Verify(r => r.CreateAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Succeeded), It.IsAny<CancellationToken>()), Times.Once);
        _orderClient.Verify(c => c.MarkAsPaidAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _notificationClient.Verify(c => c.NotifyAsync(orderId, PaymentNotificationType.PaymentSucceeded, bearerToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChargeAsync_PersistsFailedPaymentAndThrows_WhenCardIsDeclined()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = CreatePendingOrder(orderId, userId);
        var bearerToken = "test-token";
        _orderClient.Setup(c => c.GetOrderAsync(orderId, bearerToken, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<PaymentDeclinedException>(
            () => _sut.ChargeAsync(userId, CreateRequest(orderId, "4111111111110000"), bearerToken));

        _paymentRepository.Verify(r => r.CreateAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
        _orderClient.Verify(c => c.MarkAsPaidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationClient.Verify(c => c.NotifyAsync(orderId, PaymentNotificationType.PaymentFailed, bearerToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChargeAsync_Throws_WhenOrderIsNotFound()
    {
        var orderId = Guid.NewGuid();
        var bearerToken = "test-token";
        _orderClient.Setup(c => c.GetOrderAsync(orderId, bearerToken, It.IsAny<CancellationToken>())).ReturnsAsync((OrderInfo?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ChargeAsync(Guid.NewGuid(), CreateRequest(orderId), bearerToken));

        _paymentRepository.Verify(r => r.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChargeAsync_Throws_WhenOrderIsNotPending()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = new OrderInfo(orderId, userId, "Paid", 19.98m);
        var bearerToken = "test-token";
        _orderClient.Setup(c => c.GetOrderAsync(orderId, bearerToken, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<OrderNotPayableException>(() => _sut.ChargeAsync(userId, CreateRequest(orderId), bearerToken));

        _paymentRepository.Verify(r => r.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsMappedPayments()
    {
        var userId = Guid.NewGuid();
        var payment = Payment.Succeeded(Guid.NewGuid(), userId, 19.98m, "**** **** **** 1234");
        _paymentRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([payment]);

        var result = await _sut.GetByUserIdAsync(userId);

        var single = Assert.Single(result);
        Assert.Equal(payment.Id, single.Id);
        Assert.Equal("Succeeded", single.Status);
    }
}
