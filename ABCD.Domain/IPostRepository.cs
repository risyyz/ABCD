namespace ABCD.Domain {
    public interface IPostRepository {
        Task<Post?> GetByIdAsync(int postId);
        Task<Post> AddAsync(Post post);
    }
}
