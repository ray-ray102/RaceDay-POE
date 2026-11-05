using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using RaceDay_POE_API.Configuration;

namespace RaceDay_POE_API.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _client;

        public BlobStorageService(IOptions<BlobStorageOptions> options, IConfiguration configuration)
        {
            // Connection string can come from ConnectionStrings:AzureBlobStorage (preferred for secrets)
            // or BlobStorage:ConnectionString. Defaults to Azurite development storage.
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = options.Value.ConnectionString;
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "UseDevelopmentStorage=true";
            }

            _client = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadAsync(Stream content, string contentType, string containerName, string blobName, CancellationToken ct = default)
        {
            var container = _client.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

            var blob = container.GetBlobClient(blobName);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };
            await blob.UploadAsync(content, uploadOptions, ct);
            return blob.Uri.ToString();
        }
    }
}
