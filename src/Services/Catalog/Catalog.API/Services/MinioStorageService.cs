using Minio;
using Minio.DataModel.Args;
using Polly;
using Polly.Retry;

namespace Catalog.API.Services;

public class MinioStorageService(IMinioClient minio, IConfiguration config) : IStorageService
{
    private readonly string _publicUrl = config["Minio:PublicUrl"] ?? "https://storage.cloudcart.com";

    private static readonly ResiliencePipeline Pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential,
        })
        .Build();

    private static string PublicReadPolicy(string bucket) => $$"""
        {"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"AWS":["*"]},"Action":["s3:GetObject"],"Resource":["arn:aws:s3:::{{bucket}}/*"]}]}
        """;

    public async Task<string> UploadAsync(string bucket, string objectName, Stream data, string contentType, CancellationToken ct = default)
    {
        await Pipeline.ExecuteAsync(async token =>
        {
            var exists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket), token);
            if (!exists)
            {
                await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), token);
                await minio.SetPolicyAsync(new SetPolicyArgs()
                    .WithBucket(bucket)
                    .WithPolicy(PublicReadPolicy(bucket)), token);
            }

            data.Position = 0;
            await minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(data.Length)
                .WithContentType(contentType), token);
        }, ct);

        return GetPublicUrl(bucket, objectName);
    }

    public async Task DeleteAsync(string bucket, string objectName, CancellationToken ct = default)
    {
        await Pipeline.ExecuteAsync(async token =>
        {
            await minio.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName), token);
        }, ct);
    }

    public string GetPublicUrl(string bucket, string objectName) =>
        $"{_publicUrl}/{bucket}/{objectName}";
}
