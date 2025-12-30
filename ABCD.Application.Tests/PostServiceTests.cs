using ABCD.Application.Exceptions;
using ABCD.Domain;
using ABCD.Infra.Data;

using Moq;

namespace ABCD.Application.Tests {
    public class PostServiceTests {
        private readonly Mock<IPostRepository> _postRepoMock = new();
        private readonly Mock<IBlogRepository> _blogRepoMock = new();
        private readonly Blog _blog = new(new BlogId(1)) { Name = "Test Blog" };
        private readonly RequestContext _context;

        public PostServiceTests() {
            _context = new RequestContext(_blog, new ApplicationUser());
        }

        [Fact]
        public async Task CreatePostAsync_CreatesPost_WhenNoDuplicates() {
            var command = new CreatePostCommand("Title", "path-segment");
            _postRepoMock.Setup(r => r.GetByBlogIdAndTitleAsync(1, "Title")).ReturnsAsync((Post)null);
            _postRepoMock.Setup(r => r.GetByBlogIdAndPathSegmentAsync(1, "path-segment")).ReturnsAsync((Post)null);
            _postRepoMock.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync((Post p) => p);

            var service = new PostService(_context, _postRepoMock.Object, _blogRepoMock.Object);
            var result = await service.CreatePostAsync(command);

            Assert.Equal("Title", result.Title);
            Assert.Equal("path-segment", result.PathSegment.Value);
        }

        [Fact]
        public async Task CreatePostAsync_ThrowsDuplicatePostTitleException_WhenTitleExists() {
            var command = new CreatePostCommand("Title", "path-segment");
            _postRepoMock.Setup(r => r.GetByBlogIdAndTitleAsync(1, "Title")).ReturnsAsync(new Post(new BlogId(1), "Title"));
            var service = new PostService(_context, _postRepoMock.Object, _blogRepoMock.Object);

            await Assert.ThrowsAsync<DuplicatePostTitleException>(() => service.CreatePostAsync(command));
        }

        [Fact]
        public async Task CreatePostAsync_ThrowsDuplicatePathSegmentException_WhenPathSegmentExists() {
            var command = new CreatePostCommand("Title", "path-segment");
            _postRepoMock.Setup(r => r.GetByBlogIdAndTitleAsync(1, "Title")).ReturnsAsync((Post)null);
            _postRepoMock.Setup(r => r.GetByBlogIdAndPathSegmentAsync(1, "path-segment")).ReturnsAsync(new Post(new BlogId(1), "Other Title") { PathSegment = new PathSegment("path-segment") });
            var service = new PostService(_context, _postRepoMock.Object, _blogRepoMock.Object);

            await Assert.ThrowsAsync<DuplicatePathSegmentException>(() => service.CreatePostAsync(command));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllPosts() {
            var posts = new List<Post>
            {
            new Post(new BlogId(1), "Title1"),
            new Post(new BlogId(1), "Title2")
        };
            _postRepoMock.Setup(r => r.GetAllByBlogIdAsync(1)).ReturnsAsync(posts);
            var service = new PostService(_context, _postRepoMock.Object, _blogRepoMock.Object);

            var result = await service.GetAllAsync();

            Assert.Equal(2, ((List<Post>)result).Count);
        }
    }
}