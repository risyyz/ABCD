using ABCD.Core;

namespace ABCD.Core.Tests;

public class BlogTests
{
    [Fact]
    public void Blog_ShouldInheritAuditProperties()
    {
        // Arrange & Act
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "testuser",
            LastUpdatedBy = "testuser"
        };

        // Assert
        Assert.NotEqual(default(DateTime), blog.CreatedDate);
        Assert.NotEqual(default(DateTime), blog.LastUpdatedDate);
        Assert.Equal("testuser", blog.CreatedBy);
        Assert.Equal("testuser", blog.LastUpdatedBy);
    }

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitleAndAuditFields()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Old Title",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var originalUpdateDate = blog.LastUpdatedDate;
        Thread.Sleep(1); // Ensure time difference

        // Act
        blog.UpdateTitle("New Title", "updater");

        // Assert
        Assert.Equal("New Title", blog.Title);
        Assert.Equal("updater", blog.LastUpdatedBy);
        Assert.True(blog.LastUpdatedDate > originalUpdateDate);
    }

    [Fact]
    public void UpdateTitle_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Original Title",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            blog.UpdateTitle("", "updater"));
    }

    [Fact]
    public void AddDomain_WithValidDomain_ShouldAddDomain()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        blog.AddDomain("example.com", "updater");

        // Assert
        Assert.Single(blog.Domains);
        Assert.Equal("example.com", blog.Domains.First().Domain);
        Assert.Equal("updater", blog.LastUpdatedBy);
    }

    [Fact]
    public void AddDomain_WithDuplicateDomain_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddDomain("example.com", "creator");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            blog.AddDomain("EXAMPLE.COM", "updater"));
    }

    [Fact]
    public void AddDomain_WithEmptyDomain_ShouldThrowArgumentException()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            blog.AddDomain("", "updater"));
    }

    [Fact]
    public void RemoveDomain_WithExistingDomain_ShouldRemoveDomain()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddDomain("example.com", "creator");
        blog.AddDomain("test.com", "creator");

        // Act
        blog.RemoveDomain("example.com", "updater");

        // Assert
        Assert.Single(blog.Domains);
        Assert.Equal("test.com", blog.Domains.First().Domain);
    }

    [Fact]
    public void RemoveDomain_WithLastDomain_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddDomain("example.com", "creator");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            blog.RemoveDomain("example.com", "updater"));
    }

    [Fact]
    public void RemoveDomain_WithNonExistentDomain_ShouldThrowArgumentException()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddDomain("example.com", "creator");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            blog.RemoveDomain("notfound.com", "updater"));
    }

    [Fact]
    public void AddPost_WithValidPost_ShouldAddPost()
    {
        // Arrange
        var blog = new Blog
        {
            BlogId = 1,
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        blog.AddPost(post, "updater");

        // Assert
        Assert.Single(blog.Posts);
        Assert.Equal(1, post.BlogId);
        Assert.Equal(blog, post.Blog);
    }

    [Fact]
    public void AddPost_WithDuplicateSeoLink_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var blog = new Blog
        {
            BlogId = 1,
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var post1 = new Post
        {
            Title = "Test Post 1",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var post2 = new Post
        {
            Title = "Test Post 2",
            SeoFriendlyLink = "TEST-POST", // Different case but same link
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddPost(post1, "creator");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            blog.AddPost(post2, "updater"));
    }

    [Fact]
    public void PublishedPosts_ShouldReturnOnlyPublishedPostsOrderedByDate()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var draftPost = new Post
        {
            Title = "Draft Post",
            SeoFriendlyLink = "draft-post",
            Status = PostStatus.Draft,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var publishedPost1 = new Post
        {
            Title = "Published Post 1",
            SeoFriendlyLink = "published-post-1",
            Status = PostStatus.Published,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var publishedPost2 = new Post
        {
            Title = "Published Post 2",
            SeoFriendlyLink = "published-post-2",
            Status = PostStatus.Published,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Add active fragments to make posts publicly visible
        var fragment1 = new Fragment
        {
            Title = "Fragment 1",
            Content = "Content 1",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var fragment2 = new Fragment
        {
            Title = "Fragment 2",
            Content = "Content 2",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        publishedPost1.AddFragment(fragment1, "creator");
        publishedPost2.AddFragment(fragment2, "creator");

        // Adjust creation dates
        publishedPost1.CreatedDate = DateTime.UtcNow.AddDays(-2);
        publishedPost2.CreatedDate = DateTime.UtcNow.AddDays(-1);

        blog.AddPost(draftPost, "creator");
        blog.AddPost(publishedPost1, "creator");
        blog.AddPost(publishedPost2, "creator");

        // Act
        var publishedPosts = blog.PublishedPosts.ToList();

        // Assert
        Assert.Equal(2, publishedPosts.Count);
        Assert.Equal("Published Post 2", publishedPosts[0].Title); // Newest first
        Assert.Equal("Published Post 1", publishedPosts[1].Title);
    }

    [Fact]
    public void FindPostBySeoLink_WithExistingLink_ShouldReturnPost()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddPost(post, "creator");

        // Act
        var foundPost = blog.FindPostBySeoLink("TEST-POST");

        // Assert
        Assert.NotNull(foundPost);
        Assert.Equal("Test Post", foundPost.Title);
    }

    [Fact]
    public void FindPostBySeoLink_WithNonExistentLink_ShouldReturnNull()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        var foundPost = blog.FindPostBySeoLink("non-existent");

        // Assert
        Assert.Null(foundPost);
    }

    [Fact]
    public void HasDomain_WithExistingDomain_ShouldReturnTrue()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddDomain("example.com", "creator");

        // Act
        var hasDomain = blog.HasDomain("EXAMPLE.COM");

        // Assert
        Assert.True(hasDomain);
    }

    [Fact]
    public void HasDomain_WithNonExistentDomain_ShouldReturnFalse()
    {
        // Arrange
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        blog.AddDomain("example.com", "creator");

        // Act
        var hasDomain = blog.HasDomain("notfound.com");

        // Assert
        Assert.False(hasDomain);
    }
}