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
        public async Task<IActionResult> UploadImage(
            [FromRoute] int postId,
            [FromForm] IFormFile file,
            [FromForm] string destinationFileName) {

            if (postId <= 0)
                return BadRequest("Invalid post id.");

            if (file == null || file.Length == 0)
                return BadRequest("Image file is required.");

            if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are allowed.");

            if (string.IsNullOrWhiteSpace(destinationFileName))
                return BadRequest("Destination file name is required.");

            destinationFileName = destinationFileName.Trim();

            if (destinationFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return BadRequest("Destination file name contains invalid characters.");

            if (!string.Equals(destinationFileName, Path.GetFileName(destinationFileName), StringComparison.Ordinal))
                return BadRequest("Destination file name must not include directory separators.");

            var wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
            var postFolderPath = Path.Combine(wwwrootPath, _fileUploadSettings.RootPath, postId.ToString());
            Directory.CreateDirectory(postFolderPath);

            var destinationPath = Path.Combine(postFolderPath, destinationFileName);

            // Save original image
            await using (var stream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write)) {
                await file.CopyToAsync(stream);
            }

            // Save resized images (open a new stream)
            await using (var imageStream = file.OpenReadStream()) {
                await SaveResizedImagesAsync(imageStream, postFolderPath, destinationFileName);
            }

            var requestPath = _fileUploadSettings.RequestPath.TrimEnd('/');
            var imageUrl = $"{requestPath}/{postId}/{destinationFileName}";

            return Ok(new ImageUploadResponse {
                ImageUrl = imageUrl,
                FileName = destinationFileName
            });
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
    }
}