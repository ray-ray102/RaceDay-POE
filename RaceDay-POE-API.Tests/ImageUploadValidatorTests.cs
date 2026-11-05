using Microsoft.AspNetCore.Http;
using RaceDay_POE_API.Services;

namespace RaceDay_POE_API.Tests
{
    public class ImageUploadValidatorTests
    {
        private static IFormFile MakeFile(string name, string contentType, long length)
        {
            var stream = new MemoryStream(new byte[Math.Max(length, 1)]);
            return new FormFile(stream, 0, length, "file", name)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public void Validate_NullFile_ReturnsError()
        {
            Assert.NotNull(ImageUploadValidator.Validate(null));
        }

        [Fact]
        public void Validate_EmptyFile_ReturnsError()
        {
            var file = MakeFile("pic.jpg", "image/jpeg", 0);
            Assert.Equal("Please choose an image file to upload.", ImageUploadValidator.Validate(file));
        }

        [Fact]
        public void Validate_TooLarge_ReturnsError()
        {
            var file = MakeFile("pic.jpg", "image/jpeg", ImageUploadValidator.MaxBytes + 1);
            Assert.Equal("Image must be 2 MB or smaller.", ImageUploadValidator.Validate(file));
        }

        [Theory]
        [InlineData("pic.gif", "image/gif")]
        [InlineData("pic.bmp", "image/bmp")]
        [InlineData("pic.jpg", "application/octet-stream")]
        public void Validate_DisallowedType_ReturnsError(string name, string contentType)
        {
            var file = MakeFile(name, contentType, 100);
            Assert.Equal("Only JPG, PNG and WebP images are allowed.", ImageUploadValidator.Validate(file));
        }

        [Theory]
        [InlineData("banner.jpg", "image/jpeg")]
        [InlineData("banner.jpeg", "image/jpeg")]
        [InlineData("banner.png", "image/png")]
        [InlineData("banner.webp", "image/webp")]
        public void Validate_AllowedTypes_ReturnsNull(string name, string contentType)
        {
            var file = MakeFile(name, contentType, 100);
            Assert.Null(ImageUploadValidator.Validate(file));
        }
    }
}
