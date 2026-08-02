using Catalog.API.Data;
using Catalog.API.Services;
using MongoDB.Driver;

namespace Catalog.API.Features.UploadProductImage;

public static class UploadProductImageEndpoint
{
    public static IEndpointRouteBuilder MapUploadProductImageEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{id}/image", async (
            string id,
            IFormFile file,
            ICatalogContext context,
            IStorageService storage,
            CancellationToken ct) =>
        {
            var product = await context.Products
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync(ct);

            if (product is null)
                return Results.NotFound();

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
            if (!allowed.Contains(ext))
                return Results.BadRequest("Only JPG, PNG, WebP and SVG images are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                return Results.BadRequest("Image must be under 5 MB.");

            var objectName = $"{id}/image{ext}";

            await using var stream = file.OpenReadStream();
            var url = await storage.UploadAsync("products", objectName, stream, file.ContentType, ct);

            await context.Products.UpdateOneAsync(
                p => p.Id == id,
                Builders<Catalog.API.Models.Product>.Update.Set(p => p.ImageFile, url),
                cancellationToken: ct);

            return Results.Ok(new { imageUrl = url });
        })
        .DisableAntiforgery()
        .WithName("UploadProductImage")
        .WithTags("Catalog");

        return app;
    }
}
