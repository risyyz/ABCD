using ABCD.Application.Exceptions;
using ABCD.Domain;
using ABCD.Infra.Data;

using Moq;

namespace ABCD.Application.Tests {
    public class PostServiceTests {
        [Fact]
        public async Task UpdateFragmentAsync_UpdatesFragmentContent_WhenValid()
        {
            var command = new UpdateFragmentCommand(999, 1, "new content", "0x11");
            var fragments = new List<Fragment>
            {
                new Fragment(new FragmentId(1), new PostId(999), FragmentType.Heading, 1) { Content = "old content" }
            };
            var post = new Post(new BlogId(1), new PostId(999), "Title", PostStatus.Draft, fragments: fragments) { Version = new VersionToken("11") };
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(post);
            _postRepoMock.Setup(r => r.UpdateFragmentAsync(post, fragments[0])).ReturnsAsync(post);
            var service = new PostService(_context, _postRepoMock.Object);
            var result = await service.UpdateFragmentAsync(command);
            _postRepoMock.Verify(r => r.UpdateFragmentAsync(post, It.Is<Fragment>(f => f.Content == "new content")), Times.Once);
            Assert.Equal("new content", result.Fragments.First().Content);
        }

        [Fact]
        public async Task UpdateFragmentAsync_ThrowsPostNotFoundException_WhenPostDoesNotExist()
        {
            var command = new UpdateFragmentCommand(999, 1, "new content", "0x11");
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync((Post)null);
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<PostNotFoundException>(() => service.UpdateFragmentAsync(command));
        }

        [Fact]
        public async Task UpdateFragmentAsync_ThrowsVersionConflictException_WhenPostVersionDoesNotMatch()
        {
            var command = new UpdateFragmentCommand(999, 1, "new content", "0x11");
            var post = new Post(new BlogId(1), new PostId(999), "Title", PostStatus.Draft) { Version = new VersionToken("22") };
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(post);
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<VersionConflictException>(() => service.UpdateFragmentAsync(command));
        }
        
        private readonly Mock<IPostRepository> _postRepoMock = new();
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

            var service = new PostService(_context, _postRepoMock.Object);
            var result = await service.CreatePostAsync(command);

            Assert.Equal("Title", result.Title);
            Assert.Equal("path-segment", result.PathSegment.Value);
        }


        [Fact]
        public async Task CreatePostAsync_ThrowsDuplicatePostTitleException_WhenTitleExists() {
            var command = new CreatePostCommand("Title", "path-segment");
            _postRepoMock.Setup(r => r.GetByBlogIdAndTitleAsync(1, "Title")).ReturnsAsync(new Post(new BlogId(1), "Title"));
            var service = new PostService(_context, _postRepoMock.Object);

            await Assert.ThrowsAsync<DuplicatePostTitleException>(() => service.CreatePostAsync(command));
        }

        [Fact]
        public async Task CreatePostAsync_ThrowsDuplicatePathSegmentException_WhenPathSegmentExists() {
            var command = new CreatePostCommand("Title", "path-segment");
            _postRepoMock.Setup(r => r.GetByBlogIdAndTitleAsync(1, "Title")).ReturnsAsync((Post)null);
            _postRepoMock.Setup(r => r.GetByBlogIdAndPathSegmentAsync(1, "path-segment")).ReturnsAsync(new Post(new BlogId(1), "Other Title") { PathSegment = new PathSegment("path-segment") });
            var service = new PostService(_context, _postRepoMock.Object);

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
            var service = new PostService(_context, _postRepoMock.Object);

            var result = await service.GetAllAsync();

            Assert.Equal(2, ((List<Post>)result).Count);
        }

        [Fact]
        public async Task MoveFragmentAsync_ThrowsPostNotFoundException_WhenPostDoesNotExist() {
            var command = new MoveFragmentCommand(999, 1, 1, "11");
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync((Post)null);
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<PostNotFoundException>(() => service.MoveFragmentAsync(command));
        }

        [Fact]
        public async Task MoveFragmentAsync_ThrowsVersionConflictException_WhenPostVersionDoesNotMatch() {
            var command = new MoveFragmentCommand(999, 1, 1, "11");
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(new Post(new BlogId(1), "Title") { Version = new VersionToken("22") });
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<VersionConflictException>(() => service.MoveFragmentAsync(command));
        }

        [Fact]
        public async Task AddFragmentAsync_ThrowsPostNotFoundException_WhenPostDoesNotExist() {
            var command = new AddFragmentCommand(999, 1, FragmentType.Code, "11");
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync((Post)null);
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<PostNotFoundException>(() => service.AddFragmentAsync(command));
        }

        [Fact]
        public async Task AddFragmentAsync_ThrowsVersionConflictException_WhenPostVersionDoesNotMatch() {
            var command = new AddFragmentCommand(999, 1, FragmentType.Code, "11");
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(new Post(new BlogId(1), "Title") { Version = new VersionToken("22") });
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<VersionConflictException>(() => service.AddFragmentAsync(command));
        }

        [Fact]
        public async Task AddFragmentAsync_ThrowsArgumentException_WhenFragmentDoesNotExist() {
            var command = new AddFragmentCommand(999, 1, FragmentType.Code, "0x11");
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(new Post(new BlogId(1), "Title") { Version = new VersionToken("11") });
            var service = new PostService(_context, _postRepoMock.Object);
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddFragmentAsync(command));
            Assert.Equal("No fragment with id 1 exists in this post. (Parameter 'fragmentId')", ex.Message);
        }

        [Fact]
        public async Task AddFragmentAsync_SavesToDB() {
            var command = new AddFragmentCommand(999, 1, FragmentType.Code, "0x11");
            var fragments = new List<Fragment> { new Fragment(new FragmentId(1), new PostId(999), FragmentType.Heading, 1) };
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(new Post(new BlogId(1), new PostId(999), "Title", PostStatus.Draft, fragments: fragments) { Version = new VersionToken("11") });
            var service = new PostService(_context, _postRepoMock.Object);
            await service.AddFragmentAsync(command);
            _postRepoMock.Verify(r => r.UpdatePostAsync(It.IsAny<Post>()), Times.Once);
        }

        [Fact]
        public async Task DeleteFragmentAsync_DeletesFragment_WhenValid()
        {
            var command = new DeleteFragmentCommand(999, 1, "0x11");
            var fragments = new List<Fragment>
            {
                new Fragment(new FragmentId(1), new PostId(999), FragmentType.Heading, 1),
                new Fragment(new FragmentId(2), new PostId(999), FragmentType.Code, 2)
            };
            var post = new Post(new BlogId(1), new PostId(999), "Title", PostStatus.Draft, fragments: fragments) { Version = new VersionToken("11") };
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(post);
            _postRepoMock.Setup(r => r.UpdatePostAsync(It.IsAny<Post>())).ReturnsAsync(post);
            var service = new PostService(_context, _postRepoMock.Object);
            var result = await service.DeleteFragmentAsync(command);
            _postRepoMock.Verify(r => r.UpdatePostAsync(It.IsAny<Post>()), Times.Once);
            Assert.Single(result.Fragments);
            Assert.Equal(2, result.Fragments.First().FragmentId.Value);
        }

        [Fact]
        public async Task DeleteFragmentAsync_ThrowsPostNotFoundException_WhenPostDoesNotExist()
        {
            var command = new DeleteFragmentCommand(999, 1, "0x11");
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync((Post)null);
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<PostNotFoundException>(() => service.DeleteFragmentAsync(command));
        }

        [Fact]
        public async Task DeleteFragmentAsync_ThrowsVersionConflictException_WhenPostVersionDoesNotMatch()
        {
            var command = new DeleteFragmentCommand(999, 1, "0x11");
            var post = new Post(new BlogId(1), new PostId(999), "Title", PostStatus.Draft) { Version = new VersionToken("22") };
            _postRepoMock.Setup(r => r.GetByPostIdAsync(1, 999)).ReturnsAsync(post);
            var service = new PostService(_context, _postRepoMock.Object);
            await Assert.ThrowsAsync<VersionConflictException>(() => service.DeleteFragmentAsync(command));
        }
    }
}