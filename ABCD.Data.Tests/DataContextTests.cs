using ABCD.Core;
using ABCD.Data;
using ABCD.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ABCD.Data.Tests;

public class DataContextTests
{
    private DataContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var settings = Options.Create(new Settings());
        return new DataContext(options, settings);
    }

    [Fact]
    public void DataContext_CanCreateContext()
    {
        // Arrange & Act
        using var context = CreateInMemoryContext();
        
        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Blogs);
        Assert.NotNull(context.BlogDomains);
        Assert.NotNull(context.Posts);
        Assert.NotNull(context.Fragments);
    }

    [Fact]
    public void DataContext_CanCreateBlogWithDomains()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };

        // Act
        context.Blogs.Add(blog);
        context.SaveChanges();

        // Assert
        var savedBlog = context.Blogs.First();
        Assert.Equal("Test Blog", savedBlog.Title);
        Assert.Equal("test-user", savedBlog.CreatedBy);
        Assert.True(savedBlog.BlogId > 0);
    }

    [Fact]
    public void DataContext_CanCreatePostWithFragments()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };
        context.Blogs.Add(blog);
        context.SaveChanges();

        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            BlogId = blog.BlogId,
            Category = "Technology",
            Status = PostStatus.Draft,
            Synopsis = "This is a test post",
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };

        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "This is test content",
            Type = FragmentType.Text,
            Position = 0,
            Status = FragmentStatus.Active,
            PostId = 0, // Will be set when post is saved
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };

        // Act
        context.Posts.Add(post);
        context.SaveChanges();
        
        fragment.PostId = post.PostId;
        context.Fragments.Add(fragment);
        context.SaveChanges();

        // Assert
        var savedPost = context.Posts
            .Include(p => p.Fragments)
            .First(p => p.PostId == post.PostId);
            
        Assert.Equal("Test Post", savedPost.Title);
        Assert.Equal("test-post", savedPost.SeoFriendlyLink);
        Assert.Equal("Technology", savedPost.Category);
        Assert.Equal(PostStatus.Draft, savedPost.Status);
        Assert.Equal("This is a test post", savedPost.Synopsis);
        
        var savedFragment = context.Fragments.First(f => f.FragmentId == fragment.FragmentId);
        Assert.Equal("Test Fragment", savedFragment.Title);
        Assert.Equal("This is test content", savedFragment.Content);
        Assert.Equal(FragmentType.Text, savedFragment.Type);
        Assert.Equal(0, savedFragment.Position);
        Assert.Equal(FragmentStatus.Active, savedFragment.Status);
        Assert.Equal(post.PostId, savedFragment.PostId);
    }

    [Fact]
    public void DataContext_EnforcesSeoLinkUniquenessWithinBlog()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };
        context.Blogs.Add(blog);
        context.SaveChanges();

        var post1 = new Post
        {
            Title = "First Post",
            SeoFriendlyLink = "same-link",
            BlogId = blog.BlogId,
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };

        var post2 = new Post
        {
            Title = "Second Post",
            SeoFriendlyLink = "same-link",
            BlogId = blog.BlogId,
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };

        // Act & Assert
        context.Posts.Add(post1);
        context.SaveChanges();
        
        context.Posts.Add(post2);
        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
    }

    [Fact]
    public void DataContext_CascadeDeletesFragmentsWhenPostDeleted()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var blog = new Blog
        {
            Title = "Test Blog",
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };
        context.Blogs.Add(blog);
        context.SaveChanges();

        var post = new Post
        {
            Title = "Test Post",
            SeoFriendlyLink = "test-post",
            BlogId = blog.BlogId,
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };
        context.Posts.Add(post);
        context.SaveChanges();

        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "This is test content",
            Type = FragmentType.Text,
            Position = 0,
            Status = FragmentStatus.Active,
            PostId = post.PostId,
            CreatedBy = "test-user",
            LastUpdatedBy = "test-user"
        };
        context.Fragments.Add(fragment);
        context.SaveChanges();

        // Act
        context.Posts.Remove(post);
        context.SaveChanges();

        // Assert
        Assert.Empty(context.Posts.Where(p => p.PostId == post.PostId));
        Assert.Empty(context.Fragments.Where(f => f.PostId == post.PostId));
    }
}