using ABCD.Application;
using ABCD.Domain;
using ABCD.Lib;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

            var response = _typeMapper.Map<Post, PostDetailResponse>(post);
            return Ok(response);
        }

        [HttpPut("{postId:int}/fragments/{currentPosition:int}/position")]
        public async Task<IActionResult> UpdateFragmentPosition(
            [FromRoute] int postId,
            [FromRoute] int currentPosition,
            [FromBody][Required] FragmentChangePositionRequest request)
        {
            // Validate input
            if (request == null || request.NewPosition <= 0)
                return BadRequest("Invalid new position.");

            var result = await _postService.UpdateFragmentPositionAsync(new ChangeFragmentPositionCommand(postId, currentPosition, request.NewPosition));
            //if (!result)
            //    return NotFound("Fragment not found or could not update position.");

            return NoContent();
        }

        public class UpdateFragmentPositionRequest
        {
            [Required]
            public int NewPosition { get; set; }
        }
    }
}
