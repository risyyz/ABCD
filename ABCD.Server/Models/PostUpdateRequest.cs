namespace ABCD.Server.Models
{
    public record PostUpdateRequest(
        string Title,
        string? Synopsis,
        string PathSegment,
        int? ParentPostId,
        string Version
    );
}
