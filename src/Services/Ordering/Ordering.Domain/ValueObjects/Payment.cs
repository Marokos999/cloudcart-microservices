namespace Ordering.Domain.ValueObjects;

public record Payment(
    string CardName,
    string CardNumber,
    string Expiration,
    string Cvv,
    int PaymentMethod)
{
    public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(cardName);
        ArgumentNullException.ThrowIfNull(cardNumber);
        ArgumentNullException.ThrowIfNull(expiration);
        ArgumentNullException.ThrowIfNull(cvv);

        return new Payment(cardName, cardNumber, expiration, cvv, paymentMethod);
    }
}