using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.Infrastructure.Data;

public static class OrderingInitialData
{
    public static async Task SeedAsync(OrderingContext context)
    {
        if (context.Orders.Any()) return;

        var customers = new[]
        {
            (Id: Guid.Parse("58c49479-ec65-4de2-86e7-033c546291aa"), Name: "Marko", LastName: "Petrovic", Email: "marko.petrovic@email.com"),
            (Id: Guid.Parse("ba0eb0f8-a3bc-4b97-b7c8-8f6d62af5b2f"), Name: "Ana",   LastName: "Jovanovic",  Email: "ana.jovanovic@email.com"),
            (Id: Guid.Parse("3eb2a11d-52c8-4d95-b4f7-a94a0562f82c"), Name: "Nikola",LastName: "Markovic",   Email: "nikola.markovic@email.com"),
            (Id: Guid.Parse("c1d6b9e4-77b1-4a60-9b3a-6f0c8e2d4751"), Name: "Milica",LastName: "Stojanovic", Email: "milica.stojanovic@email.com"),
            (Id: Guid.Parse("d4e5f6a7-b8c9-4d2e-8f3a-1b2c3d4e5f6a"), Name: "Stefan",LastName: "Nikolic",    Email: "stefan.nikolic@email.com"),
        };

        var productIds = new[]
        {
            Guid.Parse("5334c996-8457-4cf0-815c-ed2b77c4ff61"),
            Guid.Parse("c67d6323-e8b1-4bdf-9a75-b0d0d2e7002d"),
            Guid.Parse("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8"),
            Guid.Parse("6ec1297b-ec0a-4aa1-be25-6726e3b51a27"),
            Guid.Parse("b786103d-c621-4f5a-b498-23452610f88c"),
            Guid.Parse("f8a1d2b3-c4e5-4f6a-8b9c-0d1e2f3a4b5c"),
        };

        var prices = new[] { 499.99m, 449.99m, 349.99m, 249.99m, 159.99m, 799.99m };

        var statuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered,
            OrderStatus.Cancelled,
        };

        var cities = new[] { "Belgrade", "Novi Sad", "Nis", "Kragujevac", "Subotica" };
        var states = new[] { "Central Serbia", "Vojvodina", "Southern Serbia", "Sumadija", "Vojvodina" };
        var zips   = new[] { "11000", "21000", "18000", "34000", "24000" };

        var rnd = new Random(42);
        var orders = new List<Order>();

        for (int i = 1; i <= 120; i++)
        {
            var c = customers[rnd.Next(customers.Length)];
            var cityIdx = rnd.Next(cities.Length);
            var daysBack = rnd.Next(1, 365);

            var order = Order.Create(
                OrderId.Of(Guid.NewGuid()),
                CustomerId.Of(c.Id),
                OrderName.Of($"ORD-{i:D4}"),
                Address.Of(c.Name, c.LastName, c.Email, $"{rnd.Next(1, 200)} Main St", "Serbia", states[cityIdx], zips[cityIdx]),
                Address.Of(c.Name, c.LastName, c.Email, $"{rnd.Next(1, 200)} Main St", "Serbia", states[cityIdx], zips[cityIdx]),
                Payment.Of(
                    $"{c.Name} {c.LastName}",
                    $"4111111111111{rnd.Next(100, 999)}",
                    $"{rnd.Next(1, 12):D2}/{(DateTime.UtcNow.Year + rnd.Next(1, 4)) % 100:D2}",
                    $"{rnd.Next(100, 999)}",
                    rnd.Next(1, 3)
                )
            );

            var itemCount = rnd.Next(1, 4);
            var usedProducts = new HashSet<int>();
            for (int j = 0; j < itemCount; j++)
            {
                int pidx;
                do { pidx = rnd.Next(productIds.Length); } while (!usedProducts.Add(pidx));
                order.AddItem(
                    ProductId.Of(productIds[pidx]),
                    rnd.Next(1, 4),
                    prices[pidx]
                );
            }

            var status = statuses[rnd.Next(statuses.Length)];
            if (status != OrderStatus.Pending)
                order.UpdateStatus(status);

            orders.Add(order);
        }

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
    }
}
