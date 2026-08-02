using Catalog.API.Services;
using MongoDB.Driver;

namespace Catalog.API.Data;

public static class CatalogImageSeeder
{
    private static readonly Dictionary<string, string> ProductImageMap = new()
    {
        { "5334c996-8457-4cf0-815c-ed2b77c4ff61", "playstation5.svg" },
        { "c67d6323-e8b1-4bdf-9a75-b0d0d2e7002d", "xbox-series-x.svg" },
        { "4f136e9f-ff8c-4c1f-9a33-d12f689bdab8", "nintendo-switch-oled.svg" },
        { "6ec1297b-ec0a-4aa1-be25-6726e3b51a27", "steelseries-arctis-nova-pro.svg" },
        { "b786103d-c621-4f5a-b498-23452610f88c", "logitech-g-pro-x-superlight2.svg" },
        { "f8a1d2b3-c4e5-4f6a-8b9c-0d1e2f3a4b5c", "asus-rog-swift-oled.svg" },
    };

    public static async Task SeedImagesAsync(
        IMongoCollection<Models.Product> products,
        IStorageService storage,
        ILogger logger,
        CancellationToken ct = default)
    {
        var assembly = typeof(CatalogImageSeeder).Assembly;
        logger.LogInformation("=== CatalogImageSeeder starting, {Count} products to check ===", ProductImageMap.Count);

        foreach (var (productId, fileName) in ProductImageMap)
        {
            var product = await products.Find(p => p.Id == productId).FirstOrDefaultAsync(ct);
            if (product is null)
            {
                logger.LogWarning("Product {ProductId} not found in MongoDB", productId);
                continue;
            }

            logger.LogInformation("Product {ProductId} imageFile = '{ImageFile}'", productId, product.ImageFile);

            var alreadyUploaded = product.ImageFile.StartsWith("http");
            if (alreadyUploaded)
            {
                logger.LogInformation("Product {ProductId} already has image, skipping", productId);
                continue;
            }

            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName));

            if (resourceName is null)
            {
                logger.LogWarning("Embedded resource not found for {FileName}", fileName);
                continue;
            }

            await using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var objectName = $"{productId}/image.svg";

            try
            {
                var url = await storage.UploadAsync("products", objectName, stream, "image/svg+xml", ct);

                await products.UpdateOneAsync(
                    p => p.Id == productId,
                    Builders<Models.Product>.Update.Set(p => p.ImageFile, url),
                    cancellationToken: ct);

                logger.LogInformation("Seeded image for product {ProductId}: {Url}", productId, url);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Failed to seed image for {ProductId}: {Message}", productId, ex.Message);
            }
        }
    }
}
