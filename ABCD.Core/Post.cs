using ABCD.Core.Common;

namespace ABCD.Core;

public enum PostStatus
{
    Draft,
    Published
}

/// <summary>
/// Rich domain model for blog posts with fragment management and SEO behaviors
/// </summary>
public class Post : AuditableEntity
{
    private readonly List<Fragment> _fragments = new();

    public int Id { get; set; }
    public int BlogId { get; set; }
    public required string Title { get; set; }
    public required string SeoFriendlyLink { get; set; }
    public string? Category { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public string? Synopsis { get; set; }

    // Navigation properties
    public Blog? Blog { get; set; }
    public IReadOnlyList<Fragment> Fragments => _fragments.AsReadOnly();

    /// <summary>
    /// Gets only the active fragments ordered by position
    /// </summary>
    public IEnumerable<Fragment> ActiveFragments => 
        _fragments.Where(f => f.ShouldIncludeInPublishedPost())
                  .OrderBy(f => f.Position);

    /// <summary>
    /// Publishes the post, making it publicly visible
    /// </summary>
    /// <param name="updatedBy">The user publishing the post</param>
    public void Publish(string updatedBy)
    {
        if (!ActiveFragments.Any())
            throw new InvalidOperationException("Cannot publish a post without active fragments");

        Status = PostStatus.Published;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Reverts post to draft status
    /// </summary>
    /// <param name="updatedBy">The user reverting the post</param>
    public void RevertToDraft(string updatedBy)
    {
        Status = PostStatus.Draft;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Updates the post title and generates an SEO-friendly link
    /// </summary>
    /// <param name="title">New title</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdateTitle(string title, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        SeoFriendlyLink = GenerateSeoFriendlyLink(title);
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Updates the post synopsis
    /// </summary>
    /// <param name="synopsis">New synopsis</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdateSynopsis(string synopsis, string updatedBy)
    {
        Synopsis = synopsis;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Updates the post category
    /// </summary>
    /// <param name="category">New category</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdateCategory(string? category, string updatedBy)
    {
        Category = category;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Adds a new fragment to the post
    /// </summary>
    /// <param name="fragment">Fragment to add</param>
    /// <param name="updatedBy">The user adding the fragment</param>
    public void AddFragment(Fragment fragment, string updatedBy)
    {
        if (fragment == null)
            throw new ArgumentNullException(nameof(fragment));

        fragment.PostId = Id;
        fragment.Position = _fragments.Count;
        _fragments.Add(fragment);
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Removes a fragment from the post and reorders remaining fragments
    /// </summary>
    /// <param name="fragmentId">ID of fragment to remove</param>
    /// <param name="updatedBy">The user removing the fragment</param>
    public void RemoveFragment(int fragmentId, string updatedBy)
    {
        var fragment = _fragments.FirstOrDefault(f => f.Id == fragmentId);
        if (fragment == null)
            throw new ArgumentException("Fragment not found", nameof(fragmentId));

        _fragments.Remove(fragment);
        ReorderFragments(updatedBy);
    }

    /// <summary>
    /// Moves a fragment up one position
    /// </summary>
    /// <param name="fragmentId">ID of fragment to move</param>
    /// <param name="updatedBy">The user moving the fragment</param>
    public void MoveFragmentUp(int fragmentId, string updatedBy)
    {
        var fragment = _fragments.FirstOrDefault(f => f.Id == fragmentId);
        if (fragment == null)
            throw new ArgumentException("Fragment not found", nameof(fragmentId));

        if (fragment.Position > 0)
        {
            var otherFragment = _fragments.FirstOrDefault(f => f.Position == fragment.Position - 1);
            if (otherFragment != null)
            {
                SwapFragmentPositions(fragment, otherFragment, updatedBy);
            }
        }
    }

    /// <summary>
    /// Moves a fragment down one position
    /// </summary>
    /// <param name="fragmentId">ID of fragment to move</param>
    /// <param name="updatedBy">The user moving the fragment</param>
    public void MoveFragmentDown(int fragmentId, string updatedBy)
    {
        var fragment = _fragments.FirstOrDefault(f => f.Id == fragmentId);
        if (fragment == null)
            throw new ArgumentException("Fragment not found", nameof(fragmentId));

        if (fragment.Position < _fragments.Count - 1)
        {
            var otherFragment = _fragments.FirstOrDefault(f => f.Position == fragment.Position + 1);
            if (otherFragment != null)
            {
                SwapFragmentPositions(fragment, otherFragment, updatedBy);
            }
        }
    }

    /// <summary>
    /// Determines if the post can be viewed by the public
    /// </summary>
    public bool IsPubliclyVisible()
    {
        return Status == PostStatus.Published && ActiveFragments.Any();
    }

    private void SwapFragmentPositions(Fragment fragment1, Fragment fragment2, string updatedBy)
    {
        var tempPosition = fragment1.Position;
        fragment1.UpdatePosition(fragment2.Position, updatedBy);
        fragment2.UpdatePosition(tempPosition, updatedBy);
        UpdateAuditFields(updatedBy);
    }

    private void ReorderFragments(string updatedBy)
    {
        var orderedFragments = _fragments.OrderBy(f => f.Position).ToList();
        for (int i = 0; i < orderedFragments.Count; i++)
        {
            orderedFragments[i].UpdatePosition(i, updatedBy);
        }
        UpdateAuditFields(updatedBy);
    }

    private static string GenerateSeoFriendlyLink(string title)
    {
        return title.ToLowerInvariant()
                    .Replace(" ", "-")
                    .Replace("'", "")
                    .Replace("\"", "")
                    // Remove special characters, keep only alphanumeric and hyphens
                    .Where(c => char.IsLetterOrDigit(c) || c == '-')
                    .Aggregate("", (current, c) => current + c);
    }
}
