using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ABCD.Server.Models
{
    public record ImageUploadRequest {

        [BindRequired]
        [Required]
        [FromRoute(Name = "postId")]
        public int PostId { get; init; }

        [BindRequired]
        [Required]
        [FromForm(Name = "file")]
        public IFormFile File { get; init; }




        [BindRequired]
        [Required]
        [FromForm(Name = "destinationFileName")]
        public string? DestinationFileName { get => destinationFileName?.Trim(); init => destinationFileName = value; }

        private readonly string? destinationFileName;
    }
}
