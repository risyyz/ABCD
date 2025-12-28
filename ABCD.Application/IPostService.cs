using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(PostCreateRequest request);
    }

    public record PostCreateRequest(int BlogId, string Title, string Path);
}
