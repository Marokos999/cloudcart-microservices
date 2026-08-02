using Stripe;

namespace Payment.API;

public interface IStripeService
{
    Task<(string ClientSecret, string PaymentIntentId)> CreateIntentAsync(
        decimal amount,
        string customerId,
        string orderName);

    Event ParseWebhookEvent(string json, string signature, string secret);
}
