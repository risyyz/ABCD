using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Models {
    public record FragmentDeleteRequest {
        [Required]
        public int PostId { get; init; }

        [Required]
        public int FragmentId { get; init; }

        [Required]
        [FromHeader(Name = "If-Match")]
        public string Version { get; init; }
    }
}
