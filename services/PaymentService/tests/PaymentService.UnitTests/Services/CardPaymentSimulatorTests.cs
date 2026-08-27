using PaymentService.Application.Services;

namespace PaymentService.UnitTests.Services;

public class CardPaymentSimulatorTests
{
    [Theory]
    [InlineData("4111111111111234", true)]
    [InlineData("4111111111110000", false)]
    public void IsApproved_FollowsCardSuffixConvention(string cardNumber, bool expectedApproved)
    {
        Assert.Equal(expectedApproved, CardPaymentSimulator.IsApproved(cardNumber));
    }

    [Fact]
    public void Mask_KeepsOnlyLastFourDigitsVisible()
    {
        var masked = CardPaymentSimulator.Mask("4111111111111234");

        Assert.Equal("**** **** **** 1234", masked);
    }
}
