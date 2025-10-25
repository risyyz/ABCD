using ABCD.Server.Requests;
using ABCD.Services;

using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : ControllerBase {
        private readonly IBlogService _blogService;
        public BlogsController(IBlogService blogService) => _blogService = blogService;

        [HttpGet("{blogId:int}")]
        public async Task<IActionResult> Get(int blogId) {
            var blog = await _blogService.GetBlogByIdAsync(blogId);
            return Ok(blog);
        }

        [HttpPut("{blogId:int}")]
        public async Task<IActionResult> Update(int blogId, [FromBody] BlogUpdateRequest request) {
            if (blogId != request.BlogId)
                return BadRequest("Blog ID in URL and body do not match.");

            var blogModel = request.ToBlogModel();
            var updated = await _blogService.UpdateBlogAsync(blogModel);
            return Ok(updated);
        }
    }
}
