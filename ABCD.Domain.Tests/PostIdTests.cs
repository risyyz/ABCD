namespace ABCD.Domain.Tests
{
    public class PostIdTests
    {
        [Fact]
        public void PostId_ValidValue_ShouldCreateInstance()
        {
            var id = new PostId(1);
            Assert.Equal(1, id.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void PostId_InvalidValue_ShouldThrow(int value)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PostId(value));
            Assert.Equal("Specified argument was out of the range of valid values. (Parameter 'PostId must be greater than 0.')", ex.Message);
        }

        [Fact]
        public void PostId_Equality_ShouldWork()
        {
            var id1 = new PostId(10);
            var id2 = new PostId(10);
            var id3 = new PostId(11);
            Assert.True(id1.Equals(id2));
            Assert.False(id1.Equals(id3));
        }
    }
}
