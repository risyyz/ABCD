namespace ABCD.Domain.Tests;

public class BlogTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_ShouldThrow_WhenNullOrEmptyOrWhitespace(string invalidName)
    {
        var blog = new Blog { BlogId = new BlogId(1), Name = "one two three" };
        Assert.Throws<ArgumentException>(() => blog.Name = invalidName!);
    }

    [Theory]
    [InlineData("one two")]
    [InlineData("singleword")]
    public void Name_ShouldThrow_WhenLessThanThreeWords(string invalidName)
    {
        var blog = new Blog { BlogId = new BlogId(2), Name = "one two three" };
        Assert.Throws<ArgumentException>(() => blog.Name = invalidName);
    }

    [Theory]
    [InlineData("This is valid")]
    [InlineData("One two three four")]
    public void Name_ShouldSet_WhenValid(string validName)
    {
        var blog = new Blog { BlogId = new BlogId(3), Name = "one two three" };
        blog.Name = validName;
        Assert.Equal(validName, blog.Name);
    }

    [Theory]
    [InlineData(null)]
    public void Description_ShouldBeNull_WhenSetToNull(string? desc)
    {
        var blog = new Blog { BlogId = new BlogId(4), Name = "one two three", Description = "Fantastic blog" };
        blog.Description = desc;
        Assert.Null(blog.Description);
    }

    [Theory]
    [InlineData("  some description  ", "some description")]
    [InlineData("desc", "desc")]
    [InlineData("   padded   ", "padded")]
    public void Description_ShouldTrim_WhenSet(string input, string expected)
    {
        var blog = new Blog { BlogId = new BlogId(5), Name = "one two three" };
        blog.Description = input;
        Assert.Equal(expected, blog.Description);
    }

    [Fact]
    public void AddDomain_ShouldThrow_WhenNull()
    {
        var blog = new Blog { BlogId = new BlogId(6), Name = "A B C" };
        Assert.Throws<ArgumentNullException>(() => blog.AddDomain(null!));
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("another.com")]
    public void AddDomain_ShouldAdd_WhenNotDuplicate(string domainName)
    {
        var blog = new Blog { BlogId = new BlogId(7), Name = "A B C" };
        var domain = new BlogDomain(domainName);
        blog.AddDomain(domain);
        Assert.Contains(domain, blog.Domains);
        Assert.Single(blog.Domains);
    }

    [Fact]
    public void AddDomain_ShouldNotAdd_WhenDuplicate()
    {
        var blog = new Blog { BlogId = new BlogId(8), Name = "A B C" };
        var domain = new BlogDomain("example.com");
        blog.AddDomain(domain);
        blog.AddDomain(new BlogDomain("example.com"));
        Assert.Single(blog.Domains);
    }

    [Theory]
    [InlineData("example.com", "EXAMPLE.COM")]
    [InlineData("TestDomain.com", "testdomain.com")]
    [InlineData("MiXeDcAsE.com", "mixedcase.COM")]
    public void AddDomain_ShouldNotAdd_WhenDuplicate_CaseInsensitive(string domain1, string domain2)
    {
        var blog = new Blog { BlogId = new BlogId(13), Name = "A B C" };
        blog.AddDomain(new BlogDomain(domain1));
        blog.AddDomain(new BlogDomain(domain2));
        Assert.Single(blog.Domains);
    }

    [Fact]
    public void RemoveDomain_ShouldThrow_WhenNull()
    {
        var blog = new Blog { BlogId = new BlogId(9), Name = "A B C" };
        Assert.Throws<ArgumentNullException>(() => blog.RemoveDomain(null!));
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("another.com")]
    public void RemoveDomain_ShouldRemove_WhenPresent(string domainName)
    {
        var blog = new Blog { BlogId = new BlogId(10), Name = "A B C" };
        var domain = new BlogDomain(domainName);
        blog.AddDomain(domain);
        blog.RemoveDomain(domain);
        Assert.DoesNotContain(domain, blog.Domains);
    }

    [Fact]
    public void RemoveDomain_ShouldNotRemove_WhenNotPresent()
    {
        var blog = new Blog { BlogId = new BlogId(11), Name = "A B C" };
        var domain = new BlogDomain("example.com");
        blog.RemoveDomain(domain);
        Assert.Empty(blog.Domains);
    }

    [Theory]
    [InlineData("example.com", "EXAMPLE.COM")]
    [InlineData("TestDomain.com", "testdomain.com")]
    [InlineData("MiXeDcAsE.com", "mixedcase.COM")]
    public void RemoveDomain_ShouldRemove_CaseInsensitive(string domain1, string domain2)
    {
        var blog = new Blog { BlogId = new BlogId(14), Name = "A B C" };
        var d1 = new BlogDomain(domain1);
        blog.AddDomain(d1);
        blog.RemoveDomain(new BlogDomain(domain2));
        Assert.Empty(blog.Domains);
    }

    [Fact]
    public void Domains_ShouldBeReadOnly()
    {
        var blog = new Blog { BlogId = new BlogId(12), Name = "A B C" };
        Assert.IsAssignableFrom<IReadOnlyCollection<BlogDomain>>(blog.Domains);
    }
}