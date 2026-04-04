using ABCD.Application;
using ABCD.Domain;
using ABCD.Server.Controllers;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Mvc;

using Moq;

namespace ABCD.Server.Tests.Controllers {
    public class AiControllerTests {
        private readonly Mock<IAiChatService> _aiChatServiceMock = new();
        private readonly Mock<IPostService> _postServiceMock = new();
        private readonly AiController _controller;

        public AiControllerTests() {
            _controller = new AiController(_aiChatServiceMock.Object, _postServiceMock.Object);
        }

        [Fact]
        public async Task Chat_ReturnsBadRequest_WhenMessagesIsNull() {
            var result = await _controller.Chat(new ChatRequest { Messages = null! });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Chat_ReturnsBadRequest_WhenMessagesIsEmpty() {
            var result = await _controller.Chat(new ChatRequest { Messages = [] });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Chat_ReturnsOkWithMessage_WhenMessagesProvided() {
            var messages = new List<AiChatMessage> { new() { Role = "user", Content = "Hello" } };
            _aiChatServiceMock.Setup(s => s.ChatAsync(messages)).ReturnsAsync("AI response");

            var result = await _controller.Chat(new ChatRequest { Messages = messages });

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AiChatResponse>(ok.Value);
            Assert.Equal("AI response", response.Message);
        }

        [Fact]
        public async Task GeneratePost_ReturnsBadRequest_WhenMessagesIsNull() {
            var result = await _controller.GeneratePost(new ChatRequest { Messages = null! });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GeneratePost_ReturnsBadRequest_WhenMessagesIsEmpty() {
            var result = await _controller.GeneratePost(new ChatRequest { Messages = [] });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GeneratePost_ReturnsOkWithProposal_WhenMessagesProvided() {
            var messages = new List<AiChatMessage> { new() { Role = "user", Content = "Write a post" } };
            var proposal = new PostProposal {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [new ProposedFragment { FragmentType = "RichText", Content = "<p>Hello</p>" }]
            };
            _aiChatServiceMock.Setup(s => s.GeneratePostAsync(messages)).ReturnsAsync(proposal);

            var result = await _controller.GeneratePost(new ChatRequest { Messages = messages });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(proposal, ok.Value);
        }

        [Fact]
        public async Task CreatePost_ReturnsBadRequest_WhenFragmentTypeIsInvalid() {
            var request = new CreatePostFromProposalRequest {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [new ProposedFragment { FragmentType = "Unknown", Content = "some content" }]
            };
            _postServiceMock.Setup(s => s.CreatePostAsync(It.IsAny<CreatePostCommand>())).ReturnsAsync(MakePost(1));

            var result = await _controller.CreatePost(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreatePost_ReturnsOkWithPostId_WhenValid() {
            var request = new CreatePostFromProposalRequest {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [new ProposedFragment { FragmentType = "RichText", Content = "<p>Hello</p>" }]
            };
            SetupPostServiceMocks(postId: 42, fragmentId: 100);

            var result = await _controller.CreatePost(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            var postId = ok.Value?.GetType().GetProperty("postId")?.GetValue(ok.Value);
            Assert.Equal(42, postId);
        }

        [Fact]
        public async Task CreatePost_SkipsNonImageFragments_WithBlankContent() {
            var request = new CreatePostFromProposalRequest {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [
                    new ProposedFragment { FragmentType = "RichText", Content = "   " },
                    new ProposedFragment { FragmentType = "RichText", Content = "<p>Real content</p>" }
                ]
            };
            SetupPostServiceMocks(postId: 1, fragmentId: 10);

            await _controller.CreatePost(request);

            _postServiceMock.Verify(s => s.AddFragmentAsync(It.IsAny<AddFragmentCommand>()), Times.Once);
        }

        [Fact]
        public async Task CreatePost_DoesNotSkipImageFragments_WithEmptyContent() {
            var request = new CreatePostFromProposalRequest {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [new ProposedFragment { FragmentType = "Image", Content = "", Caption = "A diagram" }]
            };
            SetupPostServiceMocks(postId: 1, fragmentId: 10);

            await _controller.CreatePost(request);

            _postServiceMock.Verify(s => s.AddFragmentAsync(It.IsAny<AddFragmentCommand>()), Times.Once);
        }

        [Fact]
        public async Task CreatePost_ConsolidatesConsecutiveRichTextFragments() {
            var request = new CreatePostFromProposalRequest {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [
                    new ProposedFragment { FragmentType = "RichText", Content = "<p>First</p>" },
                    new ProposedFragment { FragmentType = "RichText", Content = "<p>Second</p>" }
                ]
            };
            SetupPostServiceMocks(postId: 1, fragmentId: 10);
            string? capturedContent = null;
            _postServiceMock
                .Setup(s => s.UpdateFragmentAsync(It.IsAny<UpdateFragmentCommand>()))
                .Callback<UpdateFragmentCommand>(cmd => capturedContent = cmd.Content)
                .ReturnsAsync(MakePost(1));

            await _controller.CreatePost(request);

            _postServiceMock.Verify(s => s.AddFragmentAsync(It.IsAny<AddFragmentCommand>()), Times.Once);
            Assert.Equal("<p>First</p><p>Second</p>", capturedContent);
        }

        [Fact]
        public async Task CreatePost_DoesNotConsolidateCodeFragments_WithDifferentLanguages() {
            var request = new CreatePostFromProposalRequest {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [
                    new ProposedFragment { FragmentType = "Code", Content = "csharp\npublic class A {}", Caption = "C#" },
                    new ProposedFragment { FragmentType = "Code", Content = "javascript\nconst x = 1;",  Caption = "JS" }
                ]
            };
            SetupPostServiceMocks(postId: 1, fragmentId: 10);

            await _controller.CreatePost(request);

            _postServiceMock.Verify(s => s.AddFragmentAsync(It.IsAny<AddFragmentCommand>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CreatePost_ConsolidatesConsecutiveCodeFragments_WithSameLanguage() {
            var request = new CreatePostFromProposalRequest {
                Title = "Test Post",
                Path = "test-post",
                Fragments = [
                    new ProposedFragment { FragmentType = "Code", Content = "csharp\npublic class A {}", Caption = "Part 1" },
                    new ProposedFragment { FragmentType = "Code", Content = "csharp\npublic class B {}", Caption = "Part 2" }
                ]
            };
            SetupPostServiceMocks(postId: 1, fragmentId: 10);

            await _controller.CreatePost(request);

            _postServiceMock.Verify(s => s.AddFragmentAsync(It.IsAny<AddFragmentCommand>()), Times.Once);
        }

        private void SetupPostServiceMocks(int postId, int fragmentId) {
            var fragment = new Fragment(new FragmentId(fragmentId), new PostId(postId), FragmentType.RichText, 1);
            var postWithFragment = new Post(new BlogId(1), new PostId(postId), "Test Post", PostStatus.Draft, fragments: [fragment]) { Version = new VersionToken("CCDD") };

            _postServiceMock.Setup(s => s.CreatePostAsync(It.IsAny<CreatePostCommand>())).ReturnsAsync(MakePost(postId));
            _postServiceMock.Setup(s => s.AddFragmentAsync(It.IsAny<AddFragmentCommand>())).ReturnsAsync(postWithFragment);
            _postServiceMock.Setup(s => s.UpdateFragmentAsync(It.IsAny<UpdateFragmentCommand>())).ReturnsAsync(MakePost(postId));
        }

        private static Post MakePost(int postId) =>
            new Post(new BlogId(1), new PostId(postId), "Test Post", PostStatus.Draft) { Version = new VersionToken("AABB") };
    }
}
