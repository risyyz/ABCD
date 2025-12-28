namespace ABCD.Domain {
    public interface IPostRepository {
        Task<Post?> GetByIdAsync(int postId);
        Task<Post> AddAsync(Post post);
        Task<IEnumerable<Post>> GetAllByBlogIdAsync(int blogId); // Returns all posts for a blog
    }
}
