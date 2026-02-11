namespace ABCD.Domain.Tests
{
    public class BlogIdTests
    {
        [Fact]
        public void BlogId_ValidValue_ShouldCreateInstance()
        {
            var id = new BlogId(1);
            Assert.Equal(1, id.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void BlogId_InvalidValue_ShouldThrow(int value)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new BlogId(value));
            Assert.Equal("Specified argument was out of the range of valid values. (Parameter 'BlogId must be greater than 0.')", ex.Message);
        }

        [Fact]
        public void BlogId_Equality_ShouldWork()
        {
            var id1 = new BlogId(5);
            var id2 = new BlogId(5);
            var id3 = new BlogId(6);
            Assert.True(id1.Equals(id2));
            Assert.False(id1.Equals(id3));
        }
    }
}
