using ABCD.Application;
using ABCD.Domain;
using ABCD.Lib;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {        
        private readonly IPostService _postService;
        private readonly ITypeMapper _typeMapper;

        public PostsController(IPostService postService, ITypeMapper typeMapper)
        {
            _postService = postService;
            _typeMapper = typeMapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest payload)
        {
            var request = _typeMapper.Map<CreatePostRequest, CreatePostCommand>(payload);
            var post = await _postService.CreatePostAsync(request);
            return CreatedAtAction(nameof(Create), new { postId = post.PostId?.Value }, post);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postService.GetAllAsync();
            var response = _typeMapper.Map<IEnumerable<Post>, IEnumerable<PostSummaryResponse>>(posts);
            return Ok(response);
        }

        [HttpGet("{postId:int}")]
        public async Task<IActionResult> GetPostById([FromRoute] int postId) {
            var post = await _postService.GetByIdAsync(postId);
            if(post == null) {
                return NotFound();
            }

            var response = _typeMapper.Map<Post, PostSummaryResponse>(post);
            return Ok(response);
        }
    }
}
