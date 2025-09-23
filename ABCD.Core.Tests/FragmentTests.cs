using ABCD.Core;

namespace ABCD.Core.Tests;

public class FragmentTests
{
    [Fact]
    public void Fragment_ShouldInheritAuditProperties()
    {
        // Arrange & Act
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            CreatedBy = "testuser",
            LastUpdatedBy = "testuser"
        };

        // Assert
        Assert.NotEqual(default(DateTime), fragment.CreatedDate);
        Assert.NotEqual(default(DateTime), fragment.LastUpdatedDate);
        Assert.Equal("testuser", fragment.CreatedBy);
        Assert.Equal("testuser", fragment.LastUpdatedBy);
    }

    [Fact]
    public void Activate_ShouldSetStatusToActiveAndUpdateAuditFields()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Status = FragmentStatus.Inactive,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        var originalUpdateDate = fragment.LastUpdatedDate;
        Thread.Sleep(1); // Ensure time difference

        // Act
        fragment.Activate("updater");

        // Assert
        Assert.Equal(FragmentStatus.Active, fragment.Status);
        Assert.Equal("updater", fragment.LastUpdatedBy);
        Assert.True(fragment.LastUpdatedDate > originalUpdateDate);
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactiveAndUpdateAuditFields()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        fragment.Deactivate("updater");

        // Assert
        Assert.Equal(FragmentStatus.Inactive, fragment.Status);
        Assert.Equal("updater", fragment.LastUpdatedBy);
    }

    [Fact]
    public void ShouldIncludeInPublishedPost_WhenActive_ReturnsTrue()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Status = FragmentStatus.Active,
            CreatedBy = "testuser",
            LastUpdatedBy = "testuser"
        };

        // Act
        var result = fragment.ShouldIncludeInPublishedPost();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldIncludeInPublishedPost_WhenInactive_ReturnsFalse()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Status = FragmentStatus.Inactive,
            CreatedBy = "testuser",
            LastUpdatedBy = "testuser"
        };

        // Act
        var result = fragment.ShouldIncludeInPublishedPost();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateContent_WithValidData_ShouldUpdateContentAndType()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Old content",
            Type = FragmentType.Text,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        fragment.UpdateContent("New content", FragmentType.Html, "updater");

        // Assert
        Assert.Equal("New content", fragment.Content);
        Assert.Equal(FragmentType.Html, fragment.Type);
        Assert.Equal("updater", fragment.LastUpdatedBy);
    }

    [Fact]
    public void UpdateContent_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Original content",
            Type = FragmentType.Text,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            fragment.UpdateContent("", FragmentType.Text, "updater"));
    }

    [Fact]
    public void UpdatePosition_WithValidPosition_ShouldUpdatePosition()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            Position = 0,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act
        fragment.UpdatePosition(5, "updater");

        // Assert
        Assert.Equal(5, fragment.Position);
        Assert.Equal("updater", fragment.LastUpdatedBy);
    }

    [Fact]
    public void UpdatePosition_WithNegativePosition_ShouldThrowArgumentException()
    {
        // Arrange
        var fragment = new Fragment
        {
            Title = "Test Fragment",
            Content = "Test content",
            Type = FragmentType.Text,
            CreatedBy = "creator",
            LastUpdatedBy = "creator"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            fragment.UpdatePosition(-1, "updater"));
    }
}