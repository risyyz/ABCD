namespace ABCD.Domain {
    public interface IPostRepository {
        Task<Post?> GetByPostIdAsync(int postId);
        Task<Post> AddAsync(Post post);
        Task<IEnumerable<Post>> GetAllByBlogIdAsync(int blogId); // Returns all posts for a blog
        Task<Post> GetByBlogIdAndPathSegmentAsync(int v, PathSegment pathSegment);
    }
}
