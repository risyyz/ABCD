using System;
using Xunit;

namespace ABCD.Domain.Tests
{
    public class FragmentTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties_WhenValid()
        {
            var postId = new PostId(1);
            var fragment = new Fragment(postId, FragmentType.Text, 2) { Content = "Hello" };
            Assert.Equal(postId, fragment.PostId);
            Assert.Equal(FragmentType.Text, fragment.FragmentType);
            Assert.Equal(2, fragment.Position);
            Assert.Equal("Hello", fragment.Content);
            Assert.True(fragment.Active);
        }

        [Fact]
        public void Constructor_ShouldAllowNullContent()
        {
            var postId = new PostId(2);
            var fragment = new Fragment(postId, FragmentType.Text, 2);
            Assert.Null(fragment.Content);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenPostIdIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Fragment(null!, FragmentType.Text, 1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_ShouldThrow_WhenPositionIsLessThanMin(int position)
        {
            var postId = new PostId(3);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Fragment(postId, FragmentType.Text, position));
        }

        [Fact]
        public void MoveUp_ShouldDecreasePosition_WhenAboveMin()
        {
            var postId = new PostId(4);
            var fragment = new Fragment(postId, FragmentType.Text, 2);
            fragment.MoveUp();
            Assert.Equal(1, fragment.Position);
        }

        [Fact]
        public void MoveUp_ShouldThrow_WhenAtMinPosition()
        {
            var postId = new PostId(5);
            var fragment = new Fragment(postId, FragmentType.Text, 1);
            Assert.Throws<InvalidOperationException>(() => fragment.MoveUp());
        }

        [Fact]
        public void MoveDown_ShouldIncreasePosition_WhenBelowMax()
        {
            var postId = new PostId(6);
            var fragment = new Fragment(postId, FragmentType.Text, 2);
            fragment.MoveDown(3);
            Assert.Equal(3, fragment.Position);
        }

        [Fact]
        public void MoveDown_ShouldThrow_WhenAtMaxPosition()
        {
            var postId = new PostId(7);
            var fragment = new Fragment(postId, FragmentType.Text, 3);
            Assert.Throws<InvalidOperationException>(() => fragment.MoveDown(3));
        }

        [Fact]
        public void ToggleActive_ShouldSwitchActiveState()
        {
            var postId = new PostId(8);
            var fragment = new Fragment(postId, FragmentType.Text, 2);
            Assert.True(fragment.Active);
            fragment.ToggleActive();
            Assert.False(fragment.Active);
            fragment.ToggleActive();
            Assert.True(fragment.Active);
        }
    }
}