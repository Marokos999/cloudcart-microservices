using MassTransit;
using Resend;
using EmailMessage = Notification.API.Email.EmailMessage;
using Microsoft.AspNetCore.SignalR;
using Notification.API.Email;
using Notification.API.EventHandlers;
using Notification.API.Hubs;
using Notification.API.Models;
using Notification.API.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()!;
builder.Services.AddSingleton(emailSettings);
builder.Services.AddResend(options =>
{
    options.ApiToken = emailSettings.ApiKey;
});
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<NotificationStore>();
builder.Services.AddSignalR();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(corsOrigins)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BasketCheckoutEventHandler>();
    x.AddConsumer<CouponCreatedEventHandler>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["MessageBroker:Host"], h =>
        {
            h.Username(builder.Configuration["MessageBroker:UserName"]!);
            h.Password(builder.Configuration["MessageBroker:Password"]!);
        });

        cfg.UseMessageRetry(r => r.Exponential(5,
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("notification-api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"] ?? "http://otel-collector:4317")))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"] ?? "http://otel-collector:4317")));

var app = builder.Build();

app.UseCors();

app.MapGet("/health", () => Results.Ok("Notification.API is running"));

app.MapGet("/notifications/{userId}", (string userId, NotificationStore store) =>
    Results.Ok(store.GetForUser(userId)));

app.MapPost("/notifications/{userId}/read", (string userId, NotificationStore store) =>
{
    store.MarkAllRead(userId);
    return Results.NoContent();
});

app.MapPost("/notifications/broadcast", async (
    BroadcastRequest req,
    IHubContext<NotificationHub> hub,
    NotificationStore store) =>
{
    var notification = new AppNotification(
        Id: Guid.NewGuid().ToString(),
        UserId: "broadcast",
        Icon: req.Icon ?? "📢",
        Text: req.Text,
        CreatedAt: DateTime.UtcNow.ToString("o"),
        Unread: true,
        Link: req.Link);

    store.AddBroadcast(notification);
    await hub.Clients.All.SendAsync("ReceiveNotification", notification);
    return Results.Ok();
});

app.MapPost("/notifications/contact", async (
    ContactRequest req,
    IEmailService emailService) =>
{
    var body = $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;background:#111;color:#f0f0f0;border-radius:8px;overflow:hidden;">
          <div style="background:#cc0000;padding:24px 40px;">
            <h2 style="margin:0;font-size:20px;">New Contact Form Message</h2>
          </div>
          <div style="padding:32px 40px;">
            <p><strong>Name:</strong> {req.Name}</p>
            <p><strong>Email:</strong> {req.Email}</p>
            <p><strong>Message:</strong></p>
            <p style="background:#1a1a1a;padding:16px;border-radius:4px;">{req.Message}</p>
          </div>
        </div>
        """;

    await emailService.SendAsync(new EmailMessage(
        To: req.To,
        Subject: $"[CloudCart Contact] Message from {req.Name}",
        Body: body));

    return Results.Ok();
});

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapPost("/notifications/send-coupon", async (
    SendCouponRequest req,
    IEmailService emailService) =>
{
    var body = $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;background:#0d0d0d;color:#f0f0f0;border-radius:8px;overflow:hidden;">
          <div style="background:#cc0000;padding:32px 40px;">
            <h1 style="margin:0;font-size:28px;letter-spacing:2px;text-transform:uppercase;font-family:'Bebas Neue',Arial,sans-serif;">CloudCart</h1>
            <p style="margin:8px 0 0;font-size:14px;opacity:.85;">Exclusive discount just for you</p>
          </div>
          <div style="padding:40px;">
            <p style="font-size:16px;margin-bottom:24px;">You have received a coupon code for your next purchase:</p>
            <div style="background:#1a1a1a;border:2px dashed #cc0000;border-radius:8px;padding:24px;text-align:center;margin-bottom:24px;">
              <p style="margin:0 0 8px;font-size:12px;text-transform:uppercase;letter-spacing:2px;opacity:.7;">Your coupon code</p>
              <p style="margin:0;font-size:36px;font-weight:700;letter-spacing:6px;color:#cc0000;font-family:monospace;">{req.Code}</p>
              <p style="margin:12px 0 0;font-size:24px;font-weight:700;">-${req.Amount} off</p>
            </div>
            <p style="font-size:14px;opacity:.7;margin-bottom:8px;">{req.Description}</p>
            <p style="font-size:14px;opacity:.7;">Enter the code at checkout to apply your discount.</p>
            <a href="http://localhost:4200/basket" style="display:inline-block;margin-top:24px;background:#cc0000;color:#fff;text-decoration:none;padding:14px 32px;border-radius:4px;font-weight:700;text-transform:uppercase;letter-spacing:1px;">Shop Now</a>
          </div>
          <div style="padding:20px 40px;background:#111;font-size:12px;opacity:.5;text-align:center;">
            CloudCart &copy; {DateTime.UtcNow.Year}
          </div>
        </div>
        """;

    await emailService.SendAsync(new EmailMessage(
        To: req.Email,
        Subject: $"Your CloudCart coupon: {req.Code}",
        Body: body));

    return Results.Ok();
});

app.Run();

record BroadcastRequest(string Text, string? Icon, string? Link);
record SendCouponRequest(string Email, string Code, int Amount, string Description);
record ContactRequest(string Name, string Email, string Message, string To);
