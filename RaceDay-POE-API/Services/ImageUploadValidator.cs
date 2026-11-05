namespace RaceDay_POE_API.Services
{
    // Shared rules for banner and profile picture uploads
    public static class ImageUploadValidator
    {
        public const long MaxBytes = 2 * 1024 * 1024; // 2 MB

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        // Returns null when the file is acceptable, otherwise an error message
        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return "Please choose an image file to upload.";
            }

            if (file.Length > MaxBytes)
            {
                return "Image must be 2 MB or smaller.";
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                return "Only JPG, PNG and WebP images are allowed.";
            }

            if (string.IsNullOrWhiteSpace(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
            {
                return "Only JPG, PNG and WebP images are allowed.";
            }

            return null;
        }

        public static string NormaliseExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext == ".jpeg" ? ".jpg" : ext;
        }
    }
}
