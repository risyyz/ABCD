using ABCD.Domain.Exceptions;

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
            var ex = Assert.Throws<DomainValidationException>(() => new Post(null!, "Valid Title"));
            Assert.Equal("BlogId cannot be null.", ex.Message);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
            Assert.Equal("blogId", ((ArgumentNullException)ex.InnerException!).ParamName);
        }

        [Fact]
        public void Constructor_WithPostId_ShouldSetProperties_WhenDraft()
        {
            var blogId = new BlogId(7);
            var postId = new PostId(10);
            var post = new Post(blogId, postId, "Valid Title", PostStatus.Draft);
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
            // The constructor should throw because Published requires DateLastPublished
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Published));
            Assert.Equal("DateLastPublished must be set when status is Published.", ex.Message);
            Assert.IsType<ArgumentException>(ex.InnerException);
            Assert.Equal("status", ((ArgumentException)ex.InnerException!).ParamName);
        }

        [Fact]
        public void Constructor_WithPostId_ShouldThrow_WhenPostIdIsNull()
        {
            var blogId = new BlogId(9);
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, null!, "Valid Title", PostStatus.Draft));
            Assert.Equal("PostId cannot be null.", ex.Message);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
            Assert.Equal("postId", ((ArgumentNullException)ex.InnerException!).ParamName);
        }

        [Fact]
        public void Constructor_WithPostId_ShouldThrow_WhenPublished()
        {
            var blogId = new BlogId(10);
            var postId = new PostId(12);
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Published));
            Assert.Equal("DateLastPublished must be set when status is Published.", ex.Message);
            Assert.IsType<ArgumentException>(ex.InnerException);
            Assert.Equal("status", ((ArgumentException)ex.InnerException!).ParamName);
        }

        [Fact]
        public void Constructor_Passes_WhenFragmentIsNull()
        {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var post = new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, null);
            Assert.Empty(post.Fragments);
        }

        [Fact]
        public void Constructor_Passes_WhenFragmentIsEmpty() {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var post = new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, new List<Fragment>());
            Assert.Empty(post.Fragments);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenFragmentIdNull() {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var fragment = new Fragment(new PostId(14), FragmentType.RichText, 1) { Content = "Test" };
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, new List<Fragment> { fragment }));
            Assert.Equal("All fragments must have an id.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenFragmentPostIdMismatch() {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var fragment = new Fragment(new FragmentId(3), new PostId(14), FragmentType.RichText, 1) { Content = "Test" };
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, new List<Fragment> { fragment }));
            Assert.Equal("Fragments within a post cannot have different ids.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenFragmentPositionDoesNotStartAt1() {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var fragment = new Fragment(new FragmentId(1), postId, FragmentType.RichText, 2) { Content = "Test" };
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, new List<Fragment> { fragment }));
            Assert.Equal("Fragment positions must be consecutive starting from 1. Fragment '1' has invalid position 2.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenFragmentPosition1IsDuplicate() {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var fragment1 = new Fragment(new FragmentId(1), postId, FragmentType.RichText, 1) { Content = "Test1" };
            var fragment2 = new Fragment(new FragmentId(2), postId, FragmentType.RichText, 1) { Content = "Test2" };
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, new List<Fragment> { fragment1, fragment2 }));
            Assert.Equal("Fragment positions must be consecutive starting from 1. Fragment '2' has invalid position 1.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenFragmentPositionsAreNotConsecutive() {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var fragment1 = new Fragment(new FragmentId(1), postId, FragmentType.RichText, 1) { Content = "Test1" };
            var fragment2 = new Fragment(new FragmentId(2), postId, FragmentType.RichText, 2) { Content = "Test2" };
            var fragment3 = new Fragment(new FragmentId(3), postId, FragmentType.RichText, 2) { Content = "Test2" };
            var fragment4 = new Fragment(new FragmentId(4), postId, FragmentType.RichText, 3) { Content = "Test2" };
            var ex = Assert.Throws<DomainValidationException>(() => new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, new List<Fragment> { fragment1, fragment2, fragment3, fragment4 }));
            Assert.Equal("Fragment positions must be consecutive starting from 1. Fragment '3' has invalid position 2.", ex.Message);
        }

        [Fact]
        public void FragmentPositionsAreConsecutiveAfterConstruction() {
            var blogId = new BlogId(11);
            var postId = new PostId(13);
            var fragment1 = new Fragment(new FragmentId(1), postId, FragmentType.RichText, 1) { Content = "Test1" };
            var fragment2 = new Fragment(new FragmentId(2), postId, FragmentType.RichText, 2) { Content = "Test2" };
            var fragment3 = new Fragment(new FragmentId(3), postId, FragmentType.RichText, 3) { Content = "Test2" };
            var fragment4 = new Fragment(new FragmentId(4), postId, FragmentType.RichText, 4) { Content = "Test2" };

            var unsortedFragments = new List<Fragment> { fragment4, fragment2, fragment1, fragment3 };
            var post = new Post(blogId, postId, "Valid Title", PostStatus.Draft, null, unsortedFragments);
            foreach(var fragment in post.Fragments) {
                Assert.Equal(fragment.Position, post.Fragments.ToList().IndexOf(fragment) + 1);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Title_ShouldThrow_WhenNullOrEmptyOrWhitespaceAndWordMissing(string invalidTitle) {
            var blogId = new BlogId(2);
            var post = new Post(blogId, "Valid Title");
            var ex = Assert.Throws<DomainValidationException>(() => post.Title = invalidTitle!);
            Assert.Equal("Title must contain at least one word and cannot be null, empty, or whitespace.", ex.Message);
            Assert.IsType<ArgumentException>(ex.InnerException);
            Assert.Equal("value", ((ArgumentException)ex.InnerException!).ParamName);
        }

        [Fact]
        public void Publish_ShouldSetStatusAndDate_WhenDraft() {
            var blogId = new BlogId(4);
            var post = new Post(blogId, new PostId(7), "Valid Title", PostStatus.Draft) { PathSegment = new PathSegment("valid-segment") };
            post.Publish();
            Assert.Equal(PostStatus.Published, post.Status);
            Assert.NotNull(post.DateLastPublished);
        }

        [Fact]
        public void Publish_ShouldThrow_WhenAlreadyPublished() {
            var blogId = new BlogId(5);
            var post = new Post(blogId, new PostId(7), "Valid Title", PostStatus.Draft) { PathSegment = new PathSegment("valid-segment") };
            post.Publish();
            var ex = Assert.Throws<DomainValidationException>(() => post.Publish());
            Assert.Contains("Post cannot be published because it does not meet all publishing requirements.", ex.Message);
            Assert.Contains("Post status must be Draft", ex.Message);
        }

        [Fact]
        public void SetAsDraft_ShouldSetStatusToDraft() {
            var blogId = new BlogId(6);
            var post = new Post(blogId, new PostId(7), "Valid Title", PostStatus.Draft) { PathSegment = new PathSegment("valid-segment") }; ;
            post.Publish();
            var publishedDate = post.DateLastPublished;
            post.UnPublish();
            Assert.Equal(PostStatus.Draft, post.Status);
            Assert.Equal(publishedDate, post.DateLastPublished);
        }

        [Fact]
        public void AddFragment_ShouldAddToLastPosition_WhenPositionNotProvided()
        {
            var blogId = new BlogId(1);
            var postId = new PostId(1);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First");
            post.AddFragment(FragmentType.RichText, "Second");
            post.AddFragment(FragmentType.RichText, "Third");

            Assert.Equal(3, post.Fragments.Count);
            Assert.Equal(new[] {1, 2, 3}, post.Fragments.Select(f => f.Position));
            Assert.Equal("Third", post.Fragments.Last().Content);
        }

        [Fact]
        public void AddFragment_ShouldInsertAtCorrectPosition_AndShiftOthers()
        {
            var blogId = new BlogId(2);
            var postId = new PostId(2);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First"); // pos 1
            post.AddFragment(FragmentType.RichText, "Second"); // pos 2
            post.AddFragment(FragmentType.RichText, "Third"); // pos 3

            post.AddFragment(FragmentType.RichText, "Inserted", 2); // Insert at pos 2

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
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First");
            post.AddFragment(FragmentType.RichText, "Second");
            post.AddFragment(FragmentType.RichText, "Third");

            var ex = Assert.Throws<FragmentPositionException>(() => post.AddFragment(FragmentType.RichText, "Invalid", invalidPosition));
            Assert.Contains("Position must be between", ex.Message);
        }

        [Fact]
        public void AddFragment_ShouldHandleBoundaryCondition_WhenNoFragmentsAndPositionProvided()
        {
            var blogId = new BlogId(4);
            var postId = new PostId(4);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);

            // Valid: position 1
            post.AddFragment(FragmentType.RichText, "First", 1);
            Assert.Single(post.Fragments);
            Assert.Equal(1, post.Fragments.First().Position);
            Assert.Equal("First", post.Fragments.First().Content);

            // Invalid: position 2
            var post2 = new Post(blogId, postId, "Title", PostStatus.Draft);
            var ex = Assert.Throws<FragmentPositionException>(() => post2.AddFragment(FragmentType.RichText, "Invalid", 2));
            Assert.Contains("Position must be between", ex.Message);
        }

        [Fact]
        public void Parent_CanBeSetToNull()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, "Title");
            post.Parent = null;
            Assert.Null(post.Parent);
        }

        [Fact]
        public void Parent_CanBeSetToAnotherPost()
        {
            var blogId = new BlogId(1);
            var parent = new Post(blogId, new PostId(2), "Parent", PostStatus.Draft);
            var child = new Post(blogId, new PostId(3), "Child", PostStatus.Draft);
            child.Parent = parent;
            Assert.Equal(parent, child.Parent);
        }

        [Fact]
        public void Parent_Throws_WhenSetToSelf()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(2), "Title", PostStatus.Draft);
            var ex = Assert.Throws<DomainValidationException>(() => post.Parent = post);
            Assert.Equal("A post cannot be its own ancestor.", ex.Message);
        }

        [Fact]
        public void Parent_Throws_WhenSetToAncestor()
        {
            var blogId = new BlogId(1);
            var grandparent = new Post(blogId, new PostId(1), "Grandparent", PostStatus.Draft);
            var parent = new Post(blogId, new PostId(2), "Parent", PostStatus.Draft);
            var child = new Post(blogId, new PostId(3), "Child", PostStatus.Draft);
            parent.Parent = grandparent;
            child.Parent = parent;
            var ex = Assert.Throws<DomainValidationException>(() => grandparent.Parent = child);
            Assert.Equal("A post cannot be its own ancestor.", ex.Message);
        }

        [Fact]
        public void Parent_DoesNotThrow_WhenPostIdIsNull()
        {
            var blogId = new BlogId(1);
            var parent = new Post(blogId, "Parent");
            var child = new Post(blogId, "Child");
            child.Parent = parent;
            Assert.Equal(parent, child.Parent);
        }

        [Fact]
        public void EligibleForPublishing_ReturnsTrue_WhenDraftAndValidPathSegmentAndNoParent()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(1), "Title", PostStatus.Draft);
            post.PathSegment = new PathSegment("valid-segment");
            var result = post.EligibleForPublishing();
            Assert.True(result.CanPublish);
            Assert.Empty(result.Reasons);
        }

        [Fact]
        public void EligibleForPublishing_ReturnsFalse_WhenNotDraft()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(1), "Title", PostStatus.Draft);
            post.PathSegment = new PathSegment("valid-segment");
            post.Publish();
            var result = post.EligibleForPublishing();
            Assert.False(result.CanPublish);
            Assert.Contains("Post status must be Draft", result.Reasons, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void EligibleForPublishing_ReturnsFalse_WhenPathSegmentIsNull()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(1), "Title", PostStatus.Draft);
            post.PathSegment = null;
            var result = post.EligibleForPublishing();
            Assert.False(result.CanPublish);
            Assert.Contains("PathSegment must be set", result.Reasons, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void EligibleForPublishing_ReturnsFalse_WhenAncestorNotPublished()
        {
            var blogId = new BlogId(1);
            var parent = new Post(blogId, new PostId(2), "Parent", PostStatus.Draft);
            parent.PathSegment = new PathSegment("parent-segment");
            var child = new Post(blogId, new PostId(3), "Child", PostStatus.Draft);
            child.PathSegment = new PathSegment("child-segment");
            child.Parent = parent;
            var result = child.EligibleForPublishing();
            Assert.False(result.CanPublish);
            Assert.Contains("All ancestor posts must be Published", result.Reasons, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void EligibleForPublishing_ReturnsFalse_WhenAncestorPathSegmentIsNull()
        {
            var blogId = new BlogId(1);
            var parent = new Post(blogId, new PostId(2), "Parent", PostStatus.Published, DateTime.Today);
            parent.PathSegment = null;
            var child = new Post(blogId, new PostId(3), "Child", PostStatus.Draft);
            child.PathSegment = new PathSegment("child-segment");
            child.Parent = parent;
            var result = child.EligibleForPublishing();
            Assert.False(result.CanPublish);
            Assert.Contains("All ancestor posts must have a PathSegment", result.Reasons, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void PublishEligibilityResult_ReturnsDuplicateReason_WhenImmediateAncestorHasSamePathSegment()
        {
            var blogId = new BlogId(1);
            var parent = new Post(blogId, new PostId(2), "Parent", PostStatus.Published, DateTime.Today);
            parent.PathSegment = new PathSegment("duplicate-segment");
            var child = new Post(blogId, new PostId(3), "Child", PostStatus.Draft);
            child.PathSegment = new PathSegment("duplicate-segment");
            child.Parent = parent;
            var result = child.EligibleForPublishing();
            Assert.False(result.CanPublish);
            Assert.Contains("Duplicate PathSegment value found in ancestor chain: 'duplicate-segment'.", result.Reasons, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void PublishEligibilityResult_ReturnsDuplicateReason_WhenDistantAncestorHasSamePathSegment()
        {
            var blogId = new BlogId(1);
            var grandparent = new Post(blogId, new PostId(1), "Grandparent", PostStatus.Published, DateTime.Today);
            grandparent.PathSegment = new PathSegment("duplicate-segment");
            var parent = new Post(blogId, new PostId(2), "Parent", PostStatus.Published, DateTime.Today);
            parent.Parent = grandparent;
            parent.PathSegment = new PathSegment("parent-segment");
            var child = new Post(blogId, new PostId(3), "Child", PostStatus.Draft);
            child.Parent = parent;
            child.PathSegment = new PathSegment("duplicate-segment");
            var result = child.EligibleForPublishing();
            Assert.False(result.CanPublish);
            Assert.Contains("Duplicate PathSegment value found in ancestor chain: 'duplicate-segment'.", result.Reasons, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void PublishEligibilityResult_ReturnsMissingPostId_WhenPostIdNotSet() {
            var blogId = new BlogId(1);
            var post = new Post(blogId, "Child") { PathSegment = new PathSegment("post-segment") };
            post.PathSegment = new PathSegment("duplicate-segment");
            var result = post.EligibleForPublishing();
            Assert.False(result.CanPublish);
            Assert.Contains("Post does not have a valid id", result.Reasons, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void Publish_Throws_WhenCannotPublish()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(1), "Title", PostStatus.Draft);
            // No PathSegment
            var ex = Assert.Throws<DomainValidationException>(() => post.Publish());
            Assert.Contains("Post cannot be published because it does not meet all publishing requirements.", ex.Message);
            Assert.Contains("PathSegment must be set", ex.Message);
        }

        [Fact]
        public void Publish_SetsStatusAndDate_WhenCanPublish()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(1), "Title", PostStatus.Draft);
            post.PathSegment = new PathSegment("valid-segment");
            post.Publish();
            Assert.Equal(PostStatus.Published, post.Status);
            Assert.NotNull(post.DateLastPublished);
        }

        [Fact]
        public void UnPublish_SetsStatusToDraft_WhenPublished()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(1), "Title", PostStatus.Draft);
            post.PathSegment = new PathSegment("valid-segment");
            post.Publish();
            post.UnPublish();
            Assert.Equal(PostStatus.Draft, post.Status);
        }

        [Fact]
        public void UnPublish_Throws_WhenNotPublished()
        {
            var blogId = new BlogId(1);
            var post = new Post(blogId, new PostId(1), "Title", PostStatus.Draft);
            post.PathSegment = new PathSegment("valid-segment");
            var ex = Assert.Throws<DomainValidationException>(() => post.UnPublish());
            Assert.Equal("Post can only be unpublished if it is currently published.", ex.Message);
        }

        [Theory]
        [InlineData(1, 1, "New position must be different from the current position.")]
        [InlineData(1, 3, "Fragment can move by only one position at a time.")]
        [InlineData(3, 1, "Fragment can move by only one position at a time.")]
        public void ChangeFragmentPosition_ShouldThrow_WhenPositionsAreEqualOrDeltaNotOne(int currentPosition, int newPosition, string expectedMessage) {
            var blogId = new BlogId(1001);
            var postId = new PostId(1001);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First");
            post.AddFragment(FragmentType.RichText, "Second");
            post.AddFragment(FragmentType.RichText, "Third");

            var ex = Assert.Throws<FragmentPositionException>(() => post.ChangeFragmentPosition(currentPosition, newPosition));
            Assert.Contains(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(0, 1, "Current position must be between")]
        [InlineData(1, 0, "New position must be between")]
        [InlineData(4, 3, "Current position must be between")]
        [InlineData(3, 4, "New position must be between")]
        public void ChangeFragmentPosition_ShouldThrow_WhenPositionsOutOfRange(int currentPosition, int newPosition, string expectedMessage) {
            var blogId = new BlogId(1002);
            var postId = new PostId(1002);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First");
            post.AddFragment(FragmentType.RichText, "Second");
            post.AddFragment(FragmentType.RichText, "Third");

            var ex = Assert.Throws<FragmentPositionException>(() => post.ChangeFragmentPosition(currentPosition, newPosition));
            Assert.Contains(expectedMessage, ex.Message);
        }

        [Fact]
        public void ChangeFragmentPosition_ShouldThrow_WhenOnlyOneFragmentExists() {
            var blogId = new BlogId(1003);
            var postId = new PostId(1003);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First");

            var ex = Assert.Throws<FragmentPositionException>(() => post.ChangeFragmentPosition(1, 2));
            Assert.Equal("Cannot move fragment when 1 fragment exists.", ex.Message);
        }
        
        [Fact]
        public void ChangeFragmentPosition_ShouldThrow_WhenNoFragmentsExist() {
            var blogId = new BlogId(1004);
            var postId = new PostId(1004);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);

            var ex = Assert.Throws<FragmentPositionException>(() => post.ChangeFragmentPosition(1, 2));
            Assert.Equal("Cannot move fragment when 0 fragment exists.", ex.Message);
        }

        [Fact]
        public void ChangeFragmentPosition_ShouldMoveFragmentUp_WhenPossible() {
            var blogId = new BlogId(2001);
            var postId = new PostId(2001);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First");
            post.AddFragment(FragmentType.RichText, "Second");
            post.AddFragment(FragmentType.RichText, "Third");

            // Move fragment at position 2 up to position 1 
            post.ChangeFragmentPosition(2, 1);

            var positions = post.Fragments.Select(f => f.Position).ToArray();
            var contents = post.Fragments.Select(f => f.Content).ToArray();
            Assert.Equal(new[] { 1, 2, 3 }, positions);
            Assert.Equal("Second", contents[0]);
            Assert.Equal("First", contents[1]);
            Assert.Equal("Third", contents[2]);
        }

        [Fact]
        public void ChangeFragmentPosition_ShouldMoveFragmentDown_WhenPossible() {
            var blogId = new BlogId(2002);
            var postId = new PostId(2002);
            var post = new Post(blogId, postId, "Title", PostStatus.Draft);
            post.AddFragment(FragmentType.RichText, "First");
            post.AddFragment(FragmentType.RichText, "Second");
            post.AddFragment(FragmentType.RichText, "Third");

            // Move fragment at position 2 down to position 3
            post.ChangeFragmentPosition(2, 3);

            var positions = post.Fragments.Select(f => f.Position).ToArray();
            var contents = post.Fragments.Select(f => f.Content).ToArray();
            Assert.Equal(new[] { 1, 2, 3 }, positions);
            Assert.Equal("First", contents[0]);
            Assert.Equal("Third", contents[1]);
            Assert.Equal("Second", contents[2]);
        }
    }
}