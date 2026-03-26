using ABCD.Application;
using ABCD.Domain;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase {
        private readonly IAiChatService _aiChatService;
        private readonly IPostService _postService;

        public AiController(IAiChatService aiChatService, IPostService postService) {
            _aiChatService = aiChatService;
            _postService = postService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request) {
            if (request.Messages is null || request.Messages.Count == 0)
                return BadRequest("At least one message is required.");

            var message = await _aiChatService.ChatAsync(request.Messages);
            return Ok(new AiChatResponse { Message = message });
        }

        [HttpPost("generate-post")]
        public async Task<IActionResult> GeneratePost([FromBody] ChatRequest request) {
            if (request.Messages is null || request.Messages.Count == 0)
                return BadRequest("At least one message is required.");

            var proposal = await _aiChatService.GeneratePostAsync(request.Messages);
            return Ok(proposal);
        }

        [HttpPost("create-post")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostFromProposalRequest request) {
            var post = await _postService.CreatePostAsync(new CreatePostCommand(request.Title, request.Path));
            var postId = post.PostId!.Value;
            var version = post.Version!.HexString;

            int afterFragmentId = 0;
            foreach (var fragment in request.Fragments) {
                if (!Enum.TryParse<FragmentType>(fragment.FragmentType, true, out var fragmentType))
                    return BadRequest($"Invalid fragment type: {fragment.FragmentType}");

                var withFragment = await _postService.AddFragmentAsync(
                    new AddFragmentCommand(postId, afterFragmentId, fragmentType, version));
                version = withFragment.Version!.HexString;

                var newFragment = withFragment.Fragments.Last();
                afterFragmentId = newFragment.FragmentId!.Value;

                var updated = await _postService.UpdateFragmentAsync(
                    new UpdateFragmentCommand(postId, afterFragmentId, fragment.Content, version));
                version = updated.Version!.HexString;
            }

            return Ok(new { postId });
        }
    }
}
