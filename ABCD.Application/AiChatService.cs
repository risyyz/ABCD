using System.Text.Json;

using Microsoft.Extensions.AI;

namespace ABCD.Application {

    public record AiChatMessage {
        public required string Role { get; init; }
        public required string Content { get; init; }
    }

    public record ProposedFragment {
        public required string FragmentType { get; init; }
        public required string Content { get; init; }
        public string Caption { get; init; } = string.Empty;
    }

    public record PostProposal {
        public required string Title { get; init; }
        public required string Path { get; init; }
        public required List<ProposedFragment> Fragments { get; init; }
    }

    public interface IAiChatService {
        Task<string> ChatAsync(IEnumerable<AiChatMessage> messages);
        Task<PostProposal> GeneratePostAsync(IEnumerable<AiChatMessage> messages);
    }

    public class AiChatService : IAiChatService {
        private readonly IChatClient _chatClient;

        private const string ChatSystemPrompt = """
            You are an expert blog writing assistant for a software development blog.
            Help the user brainstorm blog post ideas, outlines, and content about programming,
            coding, software architecture, and development practices.
            Be specific, practical, and targeted at working software developers.
            Keep responses concise and actionable.
            """;

        private const string GeneratePostSystemPrompt = """
            You are a blog post generator for a software development blog.
            Based on the conversation, generate a complete, well-structured blog post.
            Respond ONLY with valid JSON — no markdown fences, no explanation, just raw JSON — in exactly this format:
            {
              "title": "Post title here",
              "path": "url-slug-here",
              "fragments": [
                { "fragmentType": "RichText", "content": "<p>HTML paragraph content</p>" },
                { "fragmentType": "Code", "content": "csharp\npublic void Example() { }", "caption": "A short description of what this code demonstrates" },
                { "fragmentType": "Table", "content": "<table><tr><th>Feature</th><th>Value</th></tr><tr><td>Speed</td><td>Fast</td></tr></table>", "caption": "A short description of what this table shows" },
                { "fragmentType": "Image", "content": "", "caption": "A short description of what this image depicts" }
              ]
            }
            Use RichText fragments (with HTML) for paragraphs and headings. Combine consecutive paragraphs and headings into  a single RichText fragment where possible.
            Use Code fragments for code snippets. For Code fragments, the content MUST start with the language identifier (e.g. "csharp", "javascript", "typescript", "python", "sql", "html") followed by \n (a JSON escape sequence), then the code. Never use a literal line break — always use \n. Never wrap code in markdown fences.
            Use Table fragments for tabular data (HTML table format).
            Use Image fragments sparingly — only when an image is explicitly relevant.
            Code, Table and Image fragments MUST include a meaningful caption field that describes what the code demonstrates, what the table shows, or what the image depicts.
            Do NOT repeat the post title inside the fragments — it is already displayed separately as a page heading.
            CRITICAL JSON STRING ESCAPING — violating these rules produces unparseable output:
            - Every double-quote character " inside any string value MUST be escaped as \" (e.g. code with string literals, HTML attributes)
            - Every backslash \ inside any string value MUST be escaped as \\
            - Every newline inside any string value MUST be written as \n — never use a literal line break inside a JSON string
            """;

        public AiChatService(IChatClient chatClient) {
            _chatClient = chatClient;
        }

        public async Task<string> ChatAsync(IEnumerable<AiChatMessage> messages) {
            var chatMessages = BuildMessages(ChatSystemPrompt, messages);
            var response = await _chatClient.GetResponseAsync(chatMessages);
            return response.Text ?? string.Empty;
        }

        public async Task<PostProposal> GeneratePostAsync(IEnumerable<AiChatMessage> messages) {
            var chatMessages = BuildMessages(GeneratePostSystemPrompt, messages);
            chatMessages.Add(new ChatMessage(ChatRole.User, "Generate the complete blog post as JSON now."));

            var response = await _chatClient.GetResponseAsync(chatMessages, new ChatOptions {
                ResponseFormat = ChatResponseFormat.ForJsonSchema<PostProposal>()
            });
            var json = StripMarkdownFences(response.Text ?? string.Empty);

            return JsonSerializer.Deserialize<PostProposal>(json, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to parse post proposal from AI response.");
        }

        private static List<ChatMessage> BuildMessages(string systemPrompt, IEnumerable<AiChatMessage> messages) {
            var chatMessages = new List<ChatMessage> { new(ChatRole.System, systemPrompt) };
            chatMessages.AddRange(messages.Select(m =>
                new ChatMessage(m.Role == "user" ? ChatRole.User : ChatRole.Assistant, m.Content)));
            return chatMessages;
        }

        private static string StripMarkdownFences(string text) {
            var trimmed = text.Trim();
            if (trimmed.StartsWith("```"))
                trimmed = trimmed[(trimmed.IndexOf('\n') + 1)..];
            if (trimmed.EndsWith("```"))
                trimmed = trimmed[..trimmed.LastIndexOf("```")];
            return trimmed.Trim();
        }
    }
}
