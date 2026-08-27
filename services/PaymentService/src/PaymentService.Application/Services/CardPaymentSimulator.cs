namespace PaymentService.Application.Services;

/// <summary>
/// Simulates a card payment outcome using a simple, deterministic card-number pattern
/// (no real payment gateway is involved). Cards ending in "0000" simulate a decline;
/// every other card is approved. Mirrors the well-known "test card" convention used by
/// real payment providers' sandboxes.
/// </summary>
public static class CardPaymentSimulator
{
    private const string DeclinedCardSuffix = "0000";
    private const int VisibleDigitCount = 4;

    public static bool IsApproved(string cardNumber) => !cardNumber.EndsWith(DeclinedCardSuffix);

    public static string Mask(string cardNumber)
    {
        var lastDigits = cardNumber[^VisibleDigitCount..];
        return $"**** **** **** {lastDigits}";
    }
}
