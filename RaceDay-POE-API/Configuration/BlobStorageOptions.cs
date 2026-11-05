namespace RaceDay_POE_API.Configuration
{
    // Bound from the "BlobStorage" section / AzureBlobStorage connection string
    public class BlobStorageOptions
    {
        public const string SectionName = "BlobStorage";

        // Prefer ConnectionStrings:AzureBlobStorage, then this value
        public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";

        public string BannersContainer { get; set; } = "raceday-banners";
        public string ProfilesContainer { get; set; } = "raceday-profiles";
    }
}
