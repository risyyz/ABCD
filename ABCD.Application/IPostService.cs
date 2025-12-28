using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(PostCreateRequest request);
        Task<IEnumerable<Post>> GetAllAsync();
    }

    public record PostCreateRequest(int BlogId, string Title, string Path);
}
