namespace Catalog.API.Services;

public interface IStorageService
{
    Task<string> UploadAsync(string bucket, string objectName, Stream data, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string bucket, string objectName, CancellationToken ct = default);
    string GetPublicUrl(string bucket, string objectName);
}
