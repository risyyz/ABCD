using ABCD.Domain;
using ABCD.Infra.Data;

using Microsoft.EntityFrameworkCore;

public class BlogRepositoryTests {
    private DataContext CreateContext(string dbName) {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new DataContext(options, Microsoft.Extensions.Options.Options.Create(new ABCD.Lib.Settings()));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBlog_WhenExists() {
        var dbName = nameof(GetByIdAsync_ReturnsBlog_WhenExists);
        using var context = CreateContext(dbName);
        var blogRecord = new BlogRecord { BlogId = 1, Name = "Test Blog Name", Description = "Desc" };
        blogRecord.Domains.Add(new DomainRecord { BlogId = 1, Domain = "example.com" });
        context.Blogs.Add(blogRecord);
        await context.SaveChangesAsync();
        var repo = new BlogRepository(context);
        var blog = await repo.GetByIdAsync(1);
        Assert.NotNull(blog);
        Assert.Equal("Test Blog Name", blog!.Name);
        Assert.Contains(blog.Domains, d => d.DomainName == "example.com");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBlogAndDomains() {
        var dbName = nameof(UpdateAsync_UpdatesBlogAndDomains);
        using var context = CreateContext(dbName);
        var blogRecord = new BlogRecord { BlogId = 2, Name = "Old Name", Description = "Old Desc" };
        blogRecord.Domains.Add(new DomainRecord { BlogId = 2, Domain = "old.com" });
        context.Blogs.Add(blogRecord);
        await context.SaveChangesAsync();
        var repo = new BlogRepository(context);
        var blog = await repo.GetByIdAsync(2);
        Assert.NotNull(blog);
        blog!.Name = "New Name";
        blog.Description = "New Desc";
        var oldDomain = blog.Domains.First();
        blog.RemoveDomain(oldDomain);
        blog.AddDomain(new BlogDomain("new.com"));
        var updated = await repo.UpdateAsync(blog);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("New Desc", updated.Description);
        Assert.Contains(updated.Domains, d => d.DomainName == "new.com");
        Assert.DoesNotContain(updated.Domains, d => d.DomainName == "old.com");
    }
}
