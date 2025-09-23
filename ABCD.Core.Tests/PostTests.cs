using ABCD.Core;

namespace ABCD.Core.Tests;

public class PostTests
{
    [Fact]
    public void Post_ShouldInheritAuditProperties()
    {
        // Arrange & Act
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "testuser",
            LastUpdatedBy = "testuser"
        };

        // Assert
        Assert.NotEqual(default(DateTime), post.CreatedDate);
        Assert.NotEqual(default(DateTime), post.LastUpdatedDate);
        Assert.Equal("testuser", post.CreatedBy);
        Assert.Equal("testuser", post.LastUpdatedBy);
    }

    [Fact]
    public void Post_DefaultStatus_ShouldBeDraft()
    {
        // Arrange & Act
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "testuser",
            LastUpdatedBy = "testuser"
        };

        // Assert
        Assert.Equal(PostStatus.Draft, post.Status);
    }

    [Fact]
    public void Publish_WithActiveFragments_ShouldSetStatusToPublished()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        post.AddFragment(fragment, "creator");

        // Act
        post.Publish("publisher");

        // Assert
        Assert.Equal(PostStatus.Published, post.Status);
        Assert.Equal("publisher", post.LastUpdatedBy);
    }

    [Fact]
    public void Publish_WithoutActiveFragments_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            post.Publish("publisher"));
    }

    [Fact]
    public void RevertToDraft_ShouldSetStatusToDraft()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            Status = PostStatus.Published,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        post.RevertToDraft("updater");

        // Assert
        Assert.Equal(PostStatus.Draft, post.Status);
        Assert.Equal("updater", post.LastUpdatedBy);
    }

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitleAndGenerateSeoLink()
    {
        // Arrange
        var post = new Post
        {
            Title = "Old Title",
            SeoFriendlyLink = "old-title",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        post.UpdateTitle("New Amazing Title!", "updater");

        // Assert
        Assert.Equal("New Amazing Title!", post.Title);
        Assert.Equal("new-amazing-title", post.SeoFriendlyLink);
        Assert.Equal("updater", post.LastUpdatedBy);
    }

    [Fact]
    public void UpdateTitle_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var post = new Post
        {
            Title = "Original Title",
            SeoFriendlyLink = "original-title",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            post.UpdateTitle("", "updater"));
    }

    [Fact]
    public void AddFragment_WithValidFragment_ShouldAddFragmentWithCorrectPosition()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment1 = new Fragment
        {
            Title = "Fragment 1",
            Content = "Content 1",
            Type = FragmentType.Text,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment2 = new Fragment
        {
            Title = "Fragment 2",
            Content = "Content 2",
            Type = FragmentType.Html,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        post.AddFragment(fragment1, "creator");
        post.AddFragment(fragment2, "creator");

        // Assert
        Assert.Equal(2, post.Fragments.Count);
        Assert.Equal(0, fragment1.Position);
        Assert.Equal(1, fragment2.Position);
        Assert.Equal(post.PostId, fragment1.PostId);
        Assert.Equal(post.PostId, fragment2.PostId);
    }

    [Fact]
    public void MoveFragmentUp_FromSecondPosition_ShouldSwapPositions()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment1 = new Fragment
        {
            FragmentId = 1,
            Title = "Fragment 1",
            Content = "Content 1",
            Type = FragmentType.Text,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment2 = new Fragment
        {
            FragmentId = 2,
            Title = "Fragment 2",
            Content = "Content 2",
            Type = FragmentType.Html,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        post.AddFragment(fragment1, "creator");
        post.AddFragment(fragment2, "creator");

        // Act
        post.MoveFragmentUp(2, "mover");

        // Assert
        Assert.Equal(1, fragment1.Position); // Fragment 1 moved down
        Assert.Equal(0, fragment2.Position); // Fragment 2 moved up
    }

    [Fact]
    public void MoveFragmentDown_FromFirstPosition_ShouldSwapPositions()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment1 = new Fragment
        {
            FragmentId = 1,
            Title = "Fragment 1",
            Content = "Content 1",
            Type = FragmentType.Text,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment2 = new Fragment
        {
            FragmentId = 2,
            Title = "Fragment 2",
            Content = "Content 2",
            Type = FragmentType.Html,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        post.AddFragment(fragment1, "creator");
        post.AddFragment(fragment2, "creator");

        // Act
        post.MoveFragmentDown(1, "mover");

        // Assert
        Assert.Equal(1, fragment1.Position); // Fragment 1 moved down
        Assert.Equal(0, fragment2.Position); // Fragment 2 moved up
    }

    [Fact]
    public void ActiveFragments_ShouldReturnOnlyActiveFragmentsInOrder()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var activeFragment1 = new Fragment
        {
            Title = "Active 1",
            Content = "Content 1",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var inactiveFragment = new Fragment
        {
            Title = "Inactive",
            Content = "Content 2",
            Type = FragmentType.Html,
            Status = FragmentStatus.Inactive,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var activeFragment2 = new Fragment
        {
            Title = "Active 2",
            Content = "Content 3",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        post.AddFragment(activeFragment1, "creator");
        post.AddFragment(inactiveFragment, "creator");
        post.AddFragment(activeFragment2, "creator");

        // Act
        var activeFragments = post.ActiveFragments.ToList();

        // Assert
        Assert.Equal(2, activeFragments.Count);
        Assert.Equal("Active 1", activeFragments[0].Title);
        Assert.Equal("Active 2", activeFragments[1].Title);
        Assert.Equal(0, activeFragments[0].Position);
        Assert.Equal(2, activeFragments[1].Position);
    }

    [Fact]
    public void IsPubliclyVisible_WhenPublishedWithActiveFragments_ReturnsTrue()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            Status = PostStatus.Published,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        post.AddFragment(fragment, "creator");

        // Act
        var result = post.IsPubliclyVisible();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPubliclyVisible_WhenDraft_ReturnsFalse()
    {
        // Arrange
        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            Status = PostStatus.Draft,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        post.AddFragment(fragment, "creator");

        // Act
        var result = post.IsPubliclyVisible();

        // Assert
        Assert.False(result);
    }
}