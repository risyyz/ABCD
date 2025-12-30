using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(CreatePostCommand request);
        Task<IEnumerable<Post>> GetAllAsync();
    }

    public record CreatePostCommand(string Title, string Path);
}
