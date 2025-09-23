using ABCD.Core;
using ABCD.Data;
using ABCD.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ABCD.Data.Tests;

/// <summary>
/// A simple validation to ensure Entity Framework configuration is correct
/// This can be run when .NET 9 SDK is available
/// </summary>
public class EFValidationHelper
{
    public static void ValidateEntityFrameworkConfiguration()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "ValidationDB")
            .Options;
        
        var settings = Options.Create(new Settings());
        
        using (var context = new DataContext(options, settings))
        {
            // Validate that all DbSets exist
            if (context.Blogs == null) throw new Exception("Blogs DbSet is null");
            if (context.BlogDomains == null) throw new Exception("BlogDomains DbSet is null");
            if (context.Posts == null) throw new Exception("Posts DbSet is null");
            if (context.Fragments == null) throw new Exception("Fragments DbSet is null");
            
            // Create test data to validate relationships
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
                Content = "Test content",
                Type = FragmentType.Text,
                Position = 0,
                Status = FragmentStatus.Active,
                PostId = post.PostId,
                CreatedBy = "test-user",
                LastUpdatedBy = "test-user"
            };
            
            context.Fragments.Add(fragment);
            context.SaveChanges();
            
            // Validate data was saved correctly
            var savedBlog = context.Blogs.First();
            var savedPost = context.Posts.First();
            var savedFragment = context.Fragments.First();
            
            if (savedBlog.BlogId != blog.BlogId) throw new Exception("Blog ID mismatch");
            if (savedPost.PostId != post.PostId) throw new Exception("Post ID mismatch");
            if (savedPost.BlogId != blog.BlogId) throw new Exception("Post-Blog relationship mismatch");
            if (savedFragment.PostId != post.PostId) throw new Exception("Fragment-Post relationship mismatch");
            
            Console.WriteLine("✅ Entity Framework configuration validation successful!");
            Console.WriteLine($"✅ Created Blog: {savedBlog.Title} (ID: {savedBlog.BlogId})");
            Console.WriteLine($"✅ Created Post: {savedPost.Title} (ID: {savedPost.PostId})");
            Console.WriteLine($"✅ Created Fragment: {savedFragment.Title} (ID: {savedFragment.FragmentId})");
        }
    }
}