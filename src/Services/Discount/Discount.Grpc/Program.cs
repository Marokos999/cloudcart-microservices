using CloudCart.BuildingBlocks.Events;
using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(
    opt => opt.UseSqlite(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["MessageBroker:Host"], h =>
        {
            h.Username(builder.Configuration["MessageBroker:UserName"]!);
            h.Password(builder.Configuration["MessageBroker:Password"]!);
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DiscountContext>();
    if (db.Database.IsRelational())
        db.Database.MigrateAsync().GetAwaiter().GetResult();
    else
        db.Database.EnsureCreated();
}

app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "CloudCart Discount gRPC Service");

// REST API for coupon management (used by the admin panel through the gateway)
app.MapGet("/coupons", async (DiscountContext db) =>
    Results.Ok(await db.Coupons.OrderBy(c => c.ProductName).ToListAsync()));

app.MapGet("/coupons/product/{productName}", async (string productName, DiscountContext db) =>
{
    var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.ProductName == productName && c.Code == "");
    return coupon is null ? Results.NotFound() : Results.Ok(coupon);
});

app.MapGet("/coupons/code/{code}", async (string code, DiscountContext db) =>
{
    var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code != "" && c.Code.ToUpper() == code.ToUpper());
    return coupon is null ? Results.NotFound() : Results.Ok(coupon);
});

app.MapPost("/coupons", async (HttpContext ctx, Coupon coupon) =>
{
    var db = ctx.RequestServices.GetRequiredService<DiscountContext>();
    var publish = ctx.RequestServices.GetRequiredService<IPublishEndpoint>();

    coupon.Id = 0;
    db.Coupons.Add(coupon);
    await db.SaveChangesAsync();

    await publish.Publish(new CouponCreatedEvent
    {
        CouponId = coupon.Id,
        ProductName = coupon.ProductName,
        Description = coupon.Description,
        Amount = coupon.Amount,
        Code = coupon.Code ?? ""
    });

    return Results.Created($"/coupons/{coupon.Id}", coupon);
});

app.MapPut("/coupons/{id:int}", async (int id, Coupon updated, DiscountContext db) =>
{
    var coupon = await db.Coupons.FindAsync(id);
    if (coupon is null) return Results.NotFound();

    coupon.ProductName = updated.ProductName;
    coupon.Description = updated.Description;
    coupon.Amount = updated.Amount;
    coupon.Code = updated.Code;
    await db.SaveChangesAsync();
    return Results.Ok(coupon);
});

app.MapDelete("/coupons/{id:int}", async (int id, DiscountContext db) =>
{
    var coupon = await db.Coupons.FindAsync(id);
    if (coupon is null) return Results.NotFound();

    db.Coupons.Remove(coupon);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();