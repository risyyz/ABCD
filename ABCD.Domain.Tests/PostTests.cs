namespace ABCD.Domain.Tests
{
    public class PostTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties_WhenValid()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, "Valid Title");
            Assert.Equal(blogId, post.BlogId);
            Assert.Equal("Valid Title", post.Title);
            Assert.Equal(PostStatus.Draft, post.Status);
            Assert.Null(post.PostId);
            Assert.Null(post.DateLastPublished);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenBlogIdIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Post(null!, "Title"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Title_ShouldThrow_WhenNullOrEmptyOrWhitespaceAndWordMissing(string invalidTitle)
        {
            var blogId = new BlogId(2);
            var post = new Post(blogId, "Valid Title");
            Assert.Throws<ArgumentException>(() => post.Title = invalidTitle!);
        }        

        [Fact]
        public void Publish_ShouldSetStatusAndDate_WhenDraft()
        {
            var blogId = new BlogId(4);
            var post = new Post(blogId, "Valid Title");
            post.Publish();
            Assert.Equal(PostStatus.Published, post.Status);
            Assert.NotNull(post.DateLastPublished);
        }

        [Fact]
        public void Publish_ShouldNotChangeDate_WhenAlreadyPublished()
        {
            var blogId = new BlogId(5);
            var post = new Post(blogId, "Valid Title");
            post.Publish();
            var firstDate = post.DateLastPublished;
            post.Publish();
            Assert.Equal(PostStatus.Published, post.Status);
            Assert.Equal(firstDate, post.DateLastPublished);
        }

        [Fact]
        public void SetAsDraft_ShouldSetStatusToDraft()
        {
            var blogId = new BlogId(6);
            var post = new Post(blogId, "Valid Title");
            post.Publish();
            var publishedDate = post.DateLastPublished;
            post.UnPublish();
            Assert.Equal(PostStatus.Draft, post.Status);
            Assert.Equal(publishedDate, post.DateLastPublished);
        }

        [Fact]
        public void Constructor_WithPostId_ShouldSetProperties_WhenDraft()
        {
            var blogId = new BlogId(7);
            var postId = new PostId(10);
            var post = new Post(blogId, postId, "Valid Title", PostStatus.Draft, null);
            Assert.Equal(blogId, post.BlogId);
            Assert.Equal(postId, post.PostId);
            Assert.Equal("Valid Title", post.Title);
            Assert.Equal(PostStatus.Draft, post.Status);
            Assert.Null(post.DateLastPublished);
        }

        [Fact]
        public void Constructor_WithPostId_ShouldSetProperties_WhenPublished()
        {
            var blogId = new BlogId(8);
            var postId = new PostId(11);
            var publishedDate = DateTime.UtcNow;
            var post = new Post(blogId, postId, "Valid Title", PostStatus.Published, publishedDate);
            Assert.Equal(blogId, post.BlogId);
            Assert.Equal(postId, post.PostId);
            Assert.Equal("Valid Title", post.Title);
            Assert.Equal(PostStatus.Published, post.Status);
            Assert.Equal(publishedDate, post.DateLastPublished);
        }

        [Fact]
        public void Constructor_WithPostId_ShouldThrow_WhenPostIdIsNull()
        {
            var blogId = new BlogId(9);
            Assert.Throws<ArgumentNullException>(() => new Post(blogId, null!, "Valid Title", PostStatus.Draft, null));
        }

        [Fact]
        public void Constructor_WithPostId_ShouldThrow_WhenPublishedAndDateIsNullOrDefault()
        {
            var blogId = new BlogId(10);
            var postId = new PostId(12);
            Assert.Throws<ArgumentException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Published, null));
            Assert.Throws<ArgumentException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Published, default));
        }
    }
}