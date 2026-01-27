using ABCD.Domain;

namespace ABCD.Application {
    public interface IPostService {
        Task<Post> CreatePostAsync(CreatePostCommand command);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int postId);
        Task<Post> UpdateFragmentPositionAsync(ChangeFragmentPositionCommand command);
    }

    public record CreatePostCommand(string Title, string Path);

    public record ChangeFragmentPositionCommand(int PostId, int CurrentPosition, int NewPosition);
}
