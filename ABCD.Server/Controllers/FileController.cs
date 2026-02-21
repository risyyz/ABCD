using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ABCD.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase {
        private readonly FileUploadSettings _fileUploadSettings;

        public FileController(IOptions<FileUploadSettings> fileUploadSettings) {
            _fileUploadSettings = fileUploadSettings.Value;
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

            var postFolderPath = Path.Combine(_fileUploadSettings.RootPath, postId.ToString());
            Directory.CreateDirectory(postFolderPath);

            var destinationPath = Path.Combine(postFolderPath, destinationFileName);

            await using (var stream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write)) {
                await file.CopyToAsync(stream);
            }

            var requestPath = _fileUploadSettings.RequestPath.TrimEnd('/');
            var imageUrl = $"{requestPath}/{postId}/{destinationFileName}";

            return Ok(new ImageUploadResponse {
                ImageUrl = imageUrl,
                FileName = destinationFileName
            });
        }
    }
}