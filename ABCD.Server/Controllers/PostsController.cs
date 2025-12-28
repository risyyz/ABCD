using ABCD.Application;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostCreateRequest request)
        {
            var post = await _postService.CreatePostAsync(request);
            return CreatedAtAction(nameof(Create), new { postId = post.PostId?.Value }, post);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postService.GetAllAsync();
            return Ok(posts);
        }
    }
}
