using ABCD.Application;
using ABCD.Domain;
using ABCD.Infra.Data;

using Microsoft.Extensions.AI;

using Moq;

namespace ABCD.Application.Tests {
    public class AiChatServiceTests {
        private readonly Mock<IChatClient> _chatClientMock = new();
        private readonly AiChatService _service;
        private readonly Blog _blog = new(new BlogId(1)) { Name = "Test Blog" };
        private readonly RequestContext _requestContext;

        public AiChatServiceTests() {
            _requestContext = new RequestContext(_blog, new ApplicationUser());
            _service = new AiChatService(_chatClientMock.Object, _requestContext);
        }

        [Fact]
        public async Task ChatAsync_ReturnsResponseText() {
            SetupChatResponse("Hi there!");

            var result = await _service.ChatAsync([new AiChatMessage { Role = "user", Content = "Hello" }]);

            Assert.Equal("Hi there!", result);
        }

        [Fact]
        public async Task ChatAsync_ReturnsEmptyString_WhenResponseHasNoText() {
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChatResponse([]));

            var result = await _service.ChatAsync([new AiChatMessage { Role = "user", Content = "Hello" }]);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task ChatAsync_IncludesSystemPrompt_AsFirstMessage() {
            IEnumerable<ChatMessage>? captured = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => captured = msgs)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, "ok")]));

            await _service.ChatAsync([new AiChatMessage { Role = "user", Content = "Hello" }]);

            Assert.NotNull(captured);
            Assert.Equal(ChatRole.System, captured!.First().Role);
        }

        [Fact]
        public async Task ChatAsync_MapsUserAndAssistantRoles() {
            IEnumerable<ChatMessage>? captured = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => captured = msgs)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, "ok")]));

            await _service.ChatAsync([
                new AiChatMessage { Role = "user",      Content = "Hello" },
                new AiChatMessage { Role = "assistant", Content = "Hi"    },
                new AiChatMessage { Role = "user",      Content = "Bye"   }
            ]);

            var list = captured!.ToList();
            Assert.Equal(ChatRole.System,    list[0].Role);
            Assert.Equal(ChatRole.User,      list[1].Role);
            Assert.Equal(ChatRole.Assistant, list[2].Role);
            Assert.Equal(ChatRole.User,      list[3].Role);
        }

        [Fact]
        public async Task ChatAsync_UsesDefaultChatSystemPrompt_WhenBlogHasNullPrompt() {
            IEnumerable<ChatMessage>? captured = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => captured = msgs)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, "ok")]));

            await _service.ChatAsync([new AiChatMessage { Role = "user", Content = "Hello" }]);

            Assert.Contains("blog writing assistant", captured!.First().Text);
        }

        [Fact]
        public async Task ChatAsync_UsesCustomChatSystemPrompt_WhenBlogHasPromptConfigured() {
            var customPrompt = "You are a custom AI assistant for this specific blog.";
            var blog = new Blog(new BlogId(2)) { Name = "Custom Blog", AiChatSystemPrompt = customPrompt };
            var service = new AiChatService(_chatClientMock.Object, new RequestContext(blog, new ApplicationUser()));
            IEnumerable<ChatMessage>? captured = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => captured = msgs)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, "ok")]));

            await service.ChatAsync([new AiChatMessage { Role = "user", Content = "Hello" }]);

            Assert.Equal(customPrompt, captured!.First().Text);
        }

        [Fact]
        public async Task GeneratePostAsync_ReturnsDeserializedProposal() {
            var json = """{"title":"SOLID Principles","path":"solid-principles","fragments":[{"fragmentType":"RichText","content":"<p>Hello</p>"}]}""";
            SetupChatResponse(json);

            var result = await _service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write a post about SOLID" }]);

            Assert.Equal("SOLID Principles", result.Title);
            Assert.Equal("solid-principles", result.Path);
            Assert.Single(result.Fragments);
            Assert.Equal("RichText", result.Fragments[0].FragmentType);
            Assert.Equal("<p>Hello</p>", result.Fragments[0].Content);
        }

        [Fact]
        public async Task GeneratePostAsync_StripsMarkdownFences_BeforeDeserializing() {
            var json = """{"title":"Test","path":"test","fragments":[{"fragmentType":"RichText","content":"<p>Hello</p>"}]}""";
            SetupChatResponse("```json\n" + json + "\n```");

            var result = await _service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write a post" }]);

            Assert.Equal("Test", result.Title);
        }

        [Fact]
        public async Task GeneratePostAsync_ThrowsJsonException_WhenResponseIsInvalidJson() {
            SetupChatResponse("not valid json at all {{{");

            await Assert.ThrowsAsync<System.Text.Json.JsonException>(
                () => _service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write" }]));
        }

        [Fact]
        public async Task GeneratePostAsync_ThrowsInvalidOperationException_WhenJsonDeserializesToNull() {
            SetupChatResponse("null");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write" }]));
        }

        [Fact]
        public async Task GeneratePostAsync_AppendsGenerationTriggerMessage() {
            var json = """{"title":"T","path":"t","fragments":[{"fragmentType":"RichText","content":"<p>x</p>"}]}""";
            IEnumerable<ChatMessage>? captured = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => captured = msgs)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, json)]));

            await _service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write" }]);

            var last = captured!.Last();
            Assert.Equal(ChatRole.User, last.Role);
            Assert.Contains("JSON", last.Text ?? string.Empty);
        }

        [Fact]
        public async Task GeneratePostAsync_PassesJsonSchemaResponseFormat() {
            var json = """{"title":"T","path":"t","fragments":[{"fragmentType":"RichText","content":"<p>x</p>"}]}""";
            ChatOptions? capturedOptions = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((_, opts, _) => capturedOptions = opts)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, json)]));

            await _service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write" }]);

            Assert.NotNull(capturedOptions?.ResponseFormat);
        }

        [Fact]
        public async Task GeneratePostAsync_UsesDefaultGeneratePostSystemPrompt_WhenBlogHasNullPrompt() {
            var json = """{"title":"T","path":"t","fragments":[{"fragmentType":"RichText","content":"<p>x</p>"}]}""";
            IEnumerable<ChatMessage>? captured = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => captured = msgs)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, json)]));

            await _service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write" }]);

            Assert.Contains("blog post generator", captured!.First().Text);
        }

        [Fact]
        public async Task GeneratePostAsync_UsesCustomGeneratePostSystemPrompt_WhenBlogHasPromptConfigured() {
            var customPrompt = "Generate posts only about cloud infrastructure topics.";
            var blog = new Blog(new BlogId(2)) { Name = "Cloud Blog", AiGeneratePostSystemPrompt = customPrompt };
            var service = new AiChatService(_chatClientMock.Object, new RequestContext(blog, new ApplicationUser()));
            var json = """{"title":"T","path":"t","fragments":[{"fragmentType":"RichText","content":"<p>x</p>"}]}""";
            IEnumerable<ChatMessage>? captured = null;
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>((msgs, _, _) => captured = msgs)
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, json)]));

            await service.GeneratePostAsync([new AiChatMessage { Role = "user", Content = "Write" }]);

            Assert.Equal(customPrompt, captured!.First().Text);
        }

        private void SetupChatResponse(string text) {
            _chatClientMock
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChatResponse([new ChatMessage(ChatRole.Assistant, text)]));
        }
    }
}
