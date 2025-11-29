namespace ABCD.Domain;

public enum PostStatus
{
    Draft,
    Published
}

public class Post {
    public BlogId BlogId { get; }
    public PostId PostId { get; set; }
    public required string Title { get; set; }

    public Post(BlogId blogId) {
        if (blogId.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(blogId), "BlogId must be greater than 0.");

        BlogId = blogId;
    }

    private Post() { }
}
