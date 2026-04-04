using ABCD.Application;
using ABCD.Domain;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Text.RegularExpressions;

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
            foreach (var fragment in ConsolidateFragments(request.Fragments)) {
                if (!Enum.TryParse<FragmentType>(fragment.FragmentType, true, out var fragmentType))
                    return BadRequest($"Invalid fragment type: {fragment.FragmentType}");

                var withFragment = await _postService.AddFragmentAsync(
                    new AddFragmentCommand(postId, afterFragmentId, fragmentType, version));
                version = withFragment.Version!.HexString;

                var newFragment = withFragment.Fragments.Last();
                afterFragmentId = newFragment.FragmentId!.Value;

                var content = fragmentType switch {
                    FragmentType.Code  => FormatCodeContent(fragment.Content, fragment.Caption),
                    FragmentType.Table => FormatTableContent(fragment.Content, fragment.Caption),
                    FragmentType.Image => FormatImageContent(fragment.Caption),
                    _                  => fragment.Content
                };

                var updated = await _postService.UpdateFragmentAsync(
                    new UpdateFragmentCommand(postId, afterFragmentId, content, version));
                version = updated.Version!.HexString;
            }

            return Ok(new { postId });
        }

        private static List<ProposedFragment> ConsolidateFragments(IEnumerable<ProposedFragment> fragments) {
            var result = new List<ProposedFragment>();
            ProposedFragment? current = null;

            foreach (var fragment in fragments) {
                if (fragment.FragmentType != "Image" && string.IsNullOrWhiteSpace(fragment.Content))
                    continue;

                if (current == null) {
                    current = fragment;
                    continue;
                }

                if (fragment.FragmentType == "RichText" && current.FragmentType == "RichText") {
                    current = current with { Content = current.Content + fragment.Content };
                }
                else if (fragment.FragmentType == "Code" && current.FragmentType == "Code" &&
                         DetectCodeLanguage(fragment.Content) == DetectCodeLanguage(current.Content)) {
                    current = current with { Content = current.Content + "\n\n" + fragment.Content };
                }
                else {
                    result.Add(current);
                    current = fragment;
                }
            }

            if (current != null)
                result.Add(current);

            return result;
        }

        private static string FormatCodeContent(string rawContent, string caption) {
            var trimmed = rawContent.Trim();
            var language = "javascript";
            var code = trimmed;

            if (trimmed.StartsWith("```")) {
                // Strip markdown code fences: ```language\ncode\n```
                var firstNewline = trimmed.IndexOf('\n');
                if (firstNewline > 0) {
                    var lang = NormaliseLanguage(trimmed[3..firstNewline].Trim());
                    if (!string.IsNullOrEmpty(lang)) language = lang;
                    code = trimmed[(firstNewline + 1)..];
                }
                if (code.TrimEnd().EndsWith("```"))
                    code = code[..code.LastIndexOf("```")].TrimEnd();
            }
            else {
                // Strip bare language tag on first line (e.g. "javascript\nconst x = 1;")
                var detected = DetectCodeLanguage(trimmed);
                if (!string.IsNullOrEmpty(detected)) {
                    language = NormaliseLanguage(detected);
                    var firstNewline = trimmed.IndexOf('\n');
                    code = firstNewline >= 0 ? trimmed[(firstNewline + 1)..] : string.Empty;
                }
            }

            return System.Text.Json.JsonSerializer.Serialize(
                new { language, code = code.Trim(), caption });
        }

        private static string FormatTableContent(string htmlContent, string caption) {
            var columns = new List<object>();
            var rows = new List<List<object>>();

            try {
                var headers = Regex.Matches(htmlContent, @"<th[^>]*>(.*?)</th>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                columns = [.. headers.Select(m => (object)new { header = StripHtml(m.Groups[1].Value), alignment = "left" })];

                var tableRows = Regex.Matches(htmlContent, @"<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match row in tableRows) {
                    var cells = Regex.Matches(row.Groups[1].Value, @"<td[^>]*>(.*?)</td>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (cells.Count > 0)
                        rows.Add([.. cells.Select(c => (object)new { value = StripHtml(c.Groups[1].Value) })]);
                }
            }
            catch { }

            return System.Text.Json.JsonSerializer.Serialize(new { caption, columns, rows });
        }

        private static string FormatImageContent(string caption) =>
            System.Text.Json.JsonSerializer.Serialize(new { imageUrl = string.Empty, caption, fileName = string.Empty });

        private static string StripHtml(string html) =>
            Regex.Replace(html.Trim(), @"<[^>]+>", string.Empty).Trim();

        private static string NormaliseLanguage(string lang) => lang.ToLowerInvariant() switch {
            "js" => "javascript",
            "ts" => "typescript",
            "cs" or "c#" => "csharp",
            "py" => "python",
            var l => l
        };

        private static string DetectCodeLanguage(string content) {
            var firstLine = content.TrimStart().Split('\n')[0].Trim();
            return firstLine.Length > 0 && firstLine.All(c => char.IsLetterOrDigit(c) || c == '#' || c == '+' || c == '-')
                ? firstLine.ToLowerInvariant()
                : string.Empty;
        }
    }
}
