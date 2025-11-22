namespace ABCD.Domain;

public enum PostStatus
{
    Draft,
    Published
}

public class Post {
    public int BlogId { get; }
    public int PostId { get; set; }
    public required string Title { get; set; }

    public Post(int blogId) {
        if (blogId <= 0)
            throw new ArgumentOutOfRangeException(nameof(blogId), "BlogId must be greater than 0.");

        BlogId = blogId;
    }

    private Post() { }
}
