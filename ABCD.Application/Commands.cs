namespace ABCD.Application {
    public record CreatePostCommand(string Title, string Path);

    public record DeleteFragmentCommand(int PostId, int FragmentId, string Version);

    public record MoveFragmentCommand(int PostId, int FragmentId, int NewPosition, string Version);

    public record UpdateFragmentCommand(int PostId, int FragmentId, string Content, string Version);

    public record UpdatePostCommand(int PostId, string Title, string Synopsis, string PathSegment, string Version);

    public record CreateSeriesCommand(string Title, string Path, string? Description);

    public record UpdateSeriesCommand(int SeriesId, string Title, string? Description, string PathSegment, string Version);

    public record ToggleSeriesStatusCommand(int SeriesId, string Version);

    public record AddPostToSeriesCommand(int SeriesId, int PostId, int Position, string Version);

    public record RemovePostFromSeriesCommand(int SeriesId, int PostId, string Version);
}