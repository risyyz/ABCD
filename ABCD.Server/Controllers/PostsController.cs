using System.ComponentModel.DataAnnotations;

using ABCD.Application;
using ABCD.Domain;
using ABCD.Lib;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace ABCD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

            var response = _typeMapper.Map<Post, PostDetailResponse>(post);
            return Ok(response);
        }

        // 1. Create post
        [HttpPost("create")]
        public IActionResult CreatePost([FromBody] object request)
        {
            throw new NotImplementedException();
        }

        // 2. Update post
        [HttpPut("update/{postId:int}")]
        public IActionResult UpdatePost(int postId, [FromBody] object request)
        {
            throw new NotImplementedException();
        }

        // 3. Delete post
        [HttpDelete("delete/{postId:int}")]
        public IActionResult DeletePost(int postId)
        {
            throw new NotImplementedException();
        }

        // 4. Add fragment
        [HttpPost("{postId:int}/fragments/add")]
        public IActionResult AddFragment(int postId, [FromBody] object request)
        {
            throw new NotImplementedException();
        }

        // 5. Update fragment
        [HttpPut("{postId:int}/fragments/update/{fragmentId:int}")]
        public IActionResult UpdateFragment(int postId, int fragmentId, [FromBody] object request)
        {
            throw new NotImplementedException();
        }

        // 6. Change fragment position
        [HttpPatch("{postId:int}/fragments/{fragmentId:int}/position")]
        public async Task<IActionResult> UpdateFragmentPosition(
            [FromRoute] int postId,
            [FromRoute] int fragmentId,
            [FromBody][Required] FragmentChangePositionRequest request) {
            // Validate input
            if (request == null || request.NewPosition <= 0)
                return BadRequest("Invalid new position.");

            var updatedPost = await _postService.UpdateFragmentPositionAsync(new ChangeFragmentPositionCommand(postId, fragmentId, request.NewPosition, request.Version));
            var response = _typeMapper.Map<Post, PostDetailResponse>(updatedPost);
            return Ok(response);
        }

        // 7. Delete fragment
        [HttpDelete("{postId:int}/fragments/delete/{fragmentId:int}")]
        public IActionResult DeleteFragment(int postId, int fragmentId)
        {
            throw new NotImplementedException();
        }

        public class UpdateFragmentPositionRequest
        {
            [Required]
            public int NewPosition { get; set; }
        }
    }
}
