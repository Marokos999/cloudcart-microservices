using Catalog.API.Data;
using Catalog.API.Features.CreateProduct;
using Catalog.API.Features.DeleteProduct;
using Catalog.API.Features.GetProductById;
using Catalog.API.Features.GetProducts;
using Catalog.API.Features.UpdateProduct;
using Catalog.API.Features.GetProductsByCategory;
using Catalog.API.Features.UploadAvatar;
using Catalog.API.Features.UploadProductImage;
using Catalog.API.Services;
using CloudCart.BuildingBlocks.Exceptions.Handler;
using MediatR;
using Minio;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddSingleton<ICatalogContext, CatalogContext>();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMinio(c => c
    .WithEndpoint(builder.Configuration["Minio:Endpoint"] ?? "cloudcart-minio:9000")
    .WithCredentials(
        builder.Configuration["Minio:AccessKey"] ?? "minioadmin",
        builder.Configuration["Minio:SecretKey"] ?? "Minio123!")
    .WithSSL(false));
builder.Services.AddScoped<IStorageService, MinioStorageService>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("catalog-api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"] ?? "http://otel-collector:4317")))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"] ?? "http://otel-collector:4317")));

var app = builder.Build();

app.UseExceptionHandler();

var catalogContext = app.Services.GetRequiredService<ICatalogContext>();
await CatalogInitialData.SeedAsync(catalogContext.Products);

using (var scope = app.Services.CreateScope())
{
    var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();
    var logger  = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await CatalogImageSeeder.SeedImagesAsync(catalogContext.Products, storage, logger);
}

app.MapGetProductsEndpoint();
app.MapGetProductsByCategoryEndpoint();
app.MapGetProductByIdEndpoint();
app.MapCreateProductEndpoint();
app.MapUpdateProductEndpoint();
app.MapDeleteProductEndpoint();
app.MapUploadProductImageEndpoint();
app.MapUploadAvatarEndpoint();

app.Run();