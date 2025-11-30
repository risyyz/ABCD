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

        [Fact]
        public void AddFragment_ShouldAddToLastPosition_WhenPositionNotProvided()
        {
            var blogId = new BlogId(1);
            var postId = new PostId(1);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First");
            post.AddFragment(FragmentType.Text, "Second");
            post.AddFragment(FragmentType.Text, "Third");

            Assert.Equal(3, post.Fragments.Count);
            Assert.Equal(new[] {1, 2, 3}, post.Fragments.Select(f => f.Position));
            Assert.Equal("Third", post.Fragments.Last().Content);
        }

        [Fact]
        public void AddFragment_ShouldInsertAtCorrectPosition_AndShiftOthers()
        {
            var blogId = new BlogId(2);
            var postId = new PostId(2);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First"); // pos 1
            post.AddFragment(FragmentType.Text, "Second"); // pos 2
            post.AddFragment(FragmentType.Text, "Third"); // pos 3

            post.AddFragment(FragmentType.Text, "Inserted", 2); // Insert at pos 2

            Assert.Equal(4, post.Fragments.Count);
            var positions = post.Fragments.Select(f => f.Position).ToArray();
            Assert.Equal(new[] {1, 2, 3, 4}, positions);
            var contents = post.Fragments.Select(f => f.Content).ToArray();
            Assert.Equal("First", contents[0]);
            Assert.Equal("Inserted", contents[1]);
            Assert.Equal("Second", contents[2]); // shifted
            Assert.Equal("Third", contents[3]); // shifted
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(5)] // out of range for 3 fragments
        public void AddFragment_ShouldThrow_WhenPositionIsInvalid(int invalidPosition)
        {
            var blogId = new BlogId(3);
            var postId = new PostId(3);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First");
            post.AddFragment(FragmentType.Text, "Second");
            post.AddFragment(FragmentType.Text, "Third");

            Assert.Throws<ArgumentOutOfRangeException>(() => post.AddFragment(FragmentType.Text, "Invalid", invalidPosition));
        }

        [Fact]
        public void AddFragment_ShouldHandleBoundaryCondition_WhenNoFragmentsAndPositionProvided()
        {
            var blogId = new BlogId(4);
            var postId = new PostId(4);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);

            // Valid: position 1
            post.AddFragment(FragmentType.Text, "First", 1);
            Assert.Single(post.Fragments);
            Assert.Equal(1, post.Fragments.First().Position);
            Assert.Equal("First", post.Fragments.First().Content);

            // Invalid: position 2
            var post2 = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            Assert.Throws<ArgumentOutOfRangeException>(() => post2.AddFragment(FragmentType.Text, "Invalid", 2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(5)] // out of range for 3 fragments
        public void MoveFragmentUp_ShouldThrow_WhenPositionIsOutOfRange(int invalidPosition)
        {
            var blogId = new BlogId(200);
            var postId = new PostId(200);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First");
            post.AddFragment(FragmentType.Text, "Second");
            post.AddFragment(FragmentType.Text, "Third");
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => post.MoveFragmentUp(invalidPosition));
            Assert.Contains("Position must be between", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(5)] // out of range for 3 fragments
        public void MoveFragmentDown_ShouldThrow_WhenPositionIsOutOfRange(int invalidPosition)
        {
            var blogId = new BlogId(201);
            var postId = new PostId(201);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First");
            post.AddFragment(FragmentType.Text, "Second");
            post.AddFragment(FragmentType.Text, "Third");
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => post.MoveFragmentDown(invalidPosition));
            Assert.Contains("Position must be between", ex.Message);
        }

        [Fact]
        public void MoveFragmentUp_ShouldThrow_WhenNoFragmentsExist()
        {
            var blogId = new BlogId(202);
            var postId = new PostId(202);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            var ex = Assert.Throws<InvalidOperationException>(() => post.MoveFragmentUp(1));
            Assert.Equal("Cannot move fragment when 0 fragment exists.", ex.Message);
        }

        [Fact]
        public void MoveFragmentDown_ShouldThrow_WhenNoFragmentsExist()
        {
            var blogId = new BlogId(203);
            var postId = new PostId(203);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            var ex = Assert.Throws<InvalidOperationException>(() => post.MoveFragmentDown(1));
            Assert.Equal("Cannot move fragment when 0 fragment exists.", ex.Message);
        }

        [Fact]
        public void MoveFragmentUp_ShouldThrow_WhenOnlyOneFragmentExists()
        {
            var blogId = new BlogId(204);
            var postId = new PostId(204);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First");
            var ex = Assert.Throws<InvalidOperationException>(() => post.MoveFragmentUp(1));
            Assert.Equal("Cannot move fragment when 1 fragment exists.", ex.Message);
        }

        [Fact]
        public void MoveFragmentDown_ShouldThrow_WhenOnlyOneFragmentExists()
        {
            var blogId = new BlogId(205);
            var postId = new PostId(205);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First");
            var ex = Assert.Throws<InvalidOperationException>(() => post.MoveFragmentDown(1));
            Assert.Equal("Cannot move fragment when 1 fragment exists.", ex.Message);
        }

        [Fact]
        public void MoveFragmentUp_ShouldMoveFragmentUp_WhenPossible()
        {
            var blogId = new BlogId(104);
            var postId = new PostId(104);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First"); // pos 1
            post.AddFragment(FragmentType.Text, "Second"); // pos 2
            post.AddFragment(FragmentType.Text, "Third"); // pos 3

            post.MoveFragmentUp(2); // Move "Second" up

            var positions = post.Fragments.Select(f => f.Position).ToArray();
            var contents = post.Fragments.Select(f => f.Content).ToArray();
            Assert.Equal(new[] {1, 2, 3}, positions);
            Assert.Equal("Second", contents[0]);
            Assert.Equal("First", contents[1]);
            Assert.Equal("Third", contents[2]);
        }

        [Fact]
        public void MoveFragmentDown_ShouldMoveFragmentDown_WhenPossible()
        {
            var blogId = new BlogId(105);
            var postId = new PostId(105);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft, null);
            post.AddFragment(FragmentType.Text, "First"); // pos 1
            post.AddFragment(FragmentType.Text, "Second"); // pos 2
            post.AddFragment(FragmentType.Text, "Third"); // pos 3

            post.MoveFragmentDown(2); // Move "Second" down

            var positions = post.Fragments.Select(f => f.Position).ToArray();
            var contents = post.Fragments.Select(f => f.Content).ToArray();
            Assert.Equal(new[] {1, 2, 3}, positions);
            Assert.Equal("First", contents[0]);
            Assert.Equal("Third", contents[1]);
            Assert.Equal("Second", contents[2]);
        }
    }
}