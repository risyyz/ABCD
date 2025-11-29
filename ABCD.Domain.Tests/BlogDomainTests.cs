using Xunit;
using ABCD.Domain;

namespace ABCD.Domain.Tests;

public class BlogDomainTests
{
    [Theory]
    [InlineData("example.com")]
    [InlineData("localhost")]
    [InlineData("TestDomain.com")]
    public void Constructor_ShouldSetDomainName_TrimAndLower(string input)
    {
        var domain = new BlogDomain($"  {input}  ");
        Assert.Equal(input.Trim().ToLowerInvariant(), domain.DomainName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid_domain!")]
    [InlineData(".com")]
    public void Constructor_ShouldThrow_OnInvalidDomain(string? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => new BlogDomain(input!));
    }

    [Theory]
    [InlineData("example.com", "EXAMPLE.COM")]
    [InlineData("TestDomain.com", "testdomain.com")]
    [InlineData("MiXeDcAsE.com", "mixedcase.COM")]
    public void Equals_ShouldBeCaseInsensitive(string domain1, string domain2)
    {
        var d1 = new BlogDomain(domain1);
        var d2 = new BlogDomain(domain2);
        Assert.True(d1.Equals(d2));
        Assert.True(d2.Equals(d1));
        Assert.Equal(d1.GetHashCode(), d2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnDomainName_LowerCase()
    {
        var domain = new BlogDomain("Example.com");
        Assert.Equal(domain.DomainName, domain.ToString());
        Assert.Equal("example.com", domain.ToString());
    }
}
