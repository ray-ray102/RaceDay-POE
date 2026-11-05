namespace RaceDay_POE_API.Services
{
    public interface IBlobStorageService
    {
        // Uploads the stream to the named container and returns a publicly readable blob URL
        Task<string> UploadAsync(Stream content, string contentType, string containerName, string blobName, CancellationToken ct = default);
    }
}
