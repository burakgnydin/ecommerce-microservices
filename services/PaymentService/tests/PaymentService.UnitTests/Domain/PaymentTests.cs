using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.UnitTests.Domain;

public class PaymentTests
{
    [Fact]
    public void Succeeded_CreatesPaymentWithSucceededStatus()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var payment = Payment.Succeeded(orderId, userId, 19.98m, "**** **** **** 1234");

        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(userId, payment.UserId);
        Assert.Equal(19.98m, payment.Amount);
        Assert.Equal("**** **** **** 1234", payment.MaskedCardNumber);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.NotEqual(Guid.Empty, payment.Id);
    }

    [Fact]
    public void Failed_CreatesPaymentWithFailedStatus()
    {
        var payment = Payment.Failed(Guid.NewGuid(), Guid.NewGuid(), 19.98m, "**** **** **** 0000");

        Assert.Equal(PaymentStatus.Failed, payment.Status);
    }

    [Fact]
    public void Succeeded_Throws_WhenOrderIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => Payment.Succeeded(Guid.Empty, Guid.NewGuid(), 19.98m, "**** **** **** 1234"));
    }

    [Fact]
    public void Succeeded_Throws_WhenUserIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => Payment.Succeeded(Guid.NewGuid(), Guid.Empty, 19.98m, "**** **** **** 1234"));
    }

    [Fact]
    public void Succeeded_Throws_WhenAmountIsNotPositive()
    {
        Assert.Throws<ArgumentException>(() => Payment.Succeeded(Guid.NewGuid(), Guid.NewGuid(), 0m, "**** **** **** 1234"));
    }

    [Fact]
    public void Succeeded_Throws_WhenMaskedCardNumberIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => Payment.Succeeded(Guid.NewGuid(), Guid.NewGuid(), 19.98m, ""));
    }
}
