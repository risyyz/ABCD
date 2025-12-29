namespace ABCD.Server.Models
{
    public record PostCreateModel {        
        public required string Title { get; init; }
        public required string Path { get; init; }
    }
    
    public record PostSummaryResponseModel
    {
        public int PostId { get; init; }
        public int BlogId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? PathSegment { get; init; }
        public DateTime? DateLastPublished { get; init; }
    }

    public record PostDetailResponseModel
    {
        public int PostId { get; init; }
        public int BlogId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? PathSegment { get; init; }
        public DateTime? DateLastPublished { get; init; }
        public DateTime DateCreated { get; init; }
        public DateTime DateModified { get; init; }
        // add fragments or other related data as needed
    }

    public record PostListResponseModel
    {
        public List<PostSummaryResponseModel> Posts { get; init; } = new();
    }
}