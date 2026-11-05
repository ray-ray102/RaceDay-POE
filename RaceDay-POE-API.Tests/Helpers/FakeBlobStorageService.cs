using RaceDay_POE_API.Services;

namespace RaceDay_POE_API.Tests.Helpers
{
    // In-memory stand-in so controller tests never talk to Azure
    public class FakeBlobStorageService : IBlobStorageService
    {
        public Task<string> UploadAsync(Stream content, string contentType, string containerName, string blobName, CancellationToken ct = default)
        {
            return Task.FromResult($"https://test.blob.local/{containerName}/{blobName}");
        }
    }
}
