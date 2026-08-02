using Stripe;

namespace Payment.API;

public class StripeService : IStripeService
{
    public Event ParseWebhookEvent(string json, string signature, string secret) =>
        secret.Length > 0
            ? EventUtility.ConstructEvent(json, signature, secret)
            : EventUtility.ParseEvent(json);

    public async Task<(string ClientSecret, string PaymentIntentId)> CreateIntentAsync(
        decimal amount,
        string customerId,
        string orderName)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                ["customerId"] = customerId,
                ["orderName"] = orderName,
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        return (intent.ClientSecret, intent.Id);
    }
}
