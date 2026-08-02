using Catalog.API.Services;
using System.Security.Claims;

namespace Catalog.API.Features.UploadAvatar;

public static class UploadAvatarEndpoint
{
    public static IEndpointRouteBuilder MapUploadAvatarEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/avatar/{userId}", async (
            string userId,
            IFormFile file,
            IStorageService storage,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var tokenUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? httpContext.User.FindFirstValue("sub");

            if (tokenUserId is null || tokenUserId != userId)
                return Results.Forbid();

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                return Results.BadRequest("Only JPG, PNG and WebP images are allowed.");

            if (file.Length > 2 * 1024 * 1024)
                return Results.BadRequest("Avatar must be under 2 MB.");

            var objectName = $"{userId}/avatar{ext}";
            await using var stream = file.OpenReadStream();
            var url = await storage.UploadAsync("avatars", objectName, stream, file.ContentType, ct);

            return Results.Ok(new { avatarUrl = url });
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .WithName("UploadAvatar")
        .WithTags("Catalog");

        return app;
    }
}
