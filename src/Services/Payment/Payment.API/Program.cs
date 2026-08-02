using Payment.API;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddScoped<IStripeService, StripeService>();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapPost("/create-intent", async (CreateIntentRequest request, IStripeService stripeService) =>
{
    var (clientSecret, paymentIntentId) = await stripeService.CreateIntentAsync(
        request.Amount, request.CustomerId, request.OrderName);

    return Results.Ok(new { clientSecret, paymentIntentId });
});

app.MapPost("/webhook", async (HttpRequest req, IStripeService stripeService) =>
{
    var json = await new StreamReader(req.Body).ReadToEndAsync();
    var webhookSecret = builder.Configuration["Stripe:WebhookSecret"] ?? "";

    try
    {
        var stripeEvent = stripeService.ParseWebhookEvent(
            json, req.Headers["Stripe-Signature"].ToString(), webhookSecret);

        if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            Console.WriteLine($"Payment succeeded: {intent?.Id} — {intent?.Amount / 100m:C}");
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

public record CreateIntentRequest(decimal Amount, string CustomerId, string OrderName);

public partial class Program { }
