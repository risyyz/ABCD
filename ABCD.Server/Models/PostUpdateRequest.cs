namespace ABCD.Server.Models
{
    public record PostUpdateRequest(
        string Title,
        string? Synopsis,
        string PathSegment,
        string Version
    );
}
