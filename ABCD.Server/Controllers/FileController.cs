using ABCD.Server.Exceptions;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ABCD.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase {
        private readonly FileUploadSettings _fileUploadSettings;
        private readonly IWebHostEnvironment _env;

        public FileController(IOptions<FileUploadSettings> fileUploadSettings, IWebHostEnvironment env) {
            _fileUploadSettings = fileUploadSettings.Value;
            _env = env;
        }

        [HttpPost("posts/{postId:int}/image")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadImageAsync(ImageUploadRequest request) {
            ValidateImageUpload(request);

            var destinationFileName = request.DestinationFileName;
            var postFolderPath = Path.Combine(WWWRootPath, _fileUploadSettings.RootPath, request.PostId.ToString());
            Directory.CreateDirectory(postFolderPath);
            var destinationPath = Path.Combine(postFolderPath, destinationFileName);

            // Save original image
            await using (var stream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write)) {
                await request.File.CopyToAsync(stream);
            }

            var requestPath = _fileUploadSettings.RequestPath.TrimEnd('/');
            var imageUrl = $"{requestPath}/{request.PostId}/{destinationFileName}";

            return Ok(new ImageUploadResponse {
                ImageUrl = imageUrl,
                FileName = destinationFileName
            });
        }

        private void ValidateImageUpload(ImageUploadRequest request) {
            if (request.PostId <= 0)
                throw new ImageUploadException("Invalid post id.");

            if (request.File == null || request.File.Length == 0)
                throw new ImageUploadException("Image file is required.");

            if (string.IsNullOrWhiteSpace(request.File.ContentType) || !request.File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ImageUploadException("Only image files are allowed.");

            if (string.IsNullOrWhiteSpace(request.DestinationFileName))
                throw new ImageUploadException("Destination file name is required.");

            if (request.DestinationFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ImageUploadException("Destination file name contains invalid characters.");

            if (!string.Equals(request.DestinationFileName, Path.GetFileName(request.DestinationFileName), StringComparison.Ordinal))
                throw new ImageUploadException("Destination file name must not include directory separators.");
            
            var postFolderPath = Path.Combine(WWWRootPath, _fileUploadSettings.RootPath, request.PostId.ToString());
            var destinationPath = Path.Combine(postFolderPath, request.DestinationFileName);
            if (System.IO.File.Exists(destinationPath))
                throw new ImageUploadException($"A file named '{request.DestinationFileName}' already exists.");
        }

        private async Task SaveResizedImagesAsync(Stream input, string basePath, string fileName) {
            var sizes = new[] { 480, 800, 1200 };
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(input);

            foreach (var width in sizes) {
                var resized = image.Clone(ctx => ctx.Resize(new ResizeOptions {
                    Mode = ResizeMode.Max,
                    Size = new Size(width, 0)
                }));
                var resizedPath = Path.Combine(basePath, $"{Path.GetFileNameWithoutExtension(fileName)}-{width}w{Path.GetExtension(fileName)}");
                await resized.SaveAsync(resizedPath);
            }
        }

        private string WWWRootPath => Path.Combine(_env.ContentRootPath, "wwwroot");
    }
}