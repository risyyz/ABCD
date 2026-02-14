namespace ABCD.Server.Models
{
    public record CreatePostRequest {        
        public required string Title { get; init; }
        public required string Path { get; init; }
    }
    
    public record PostSummaryResponse
    {
        public int PostId { get; init; }
        public int BlogId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? PathSegment { get; init; }
        public DateTime? DateLastPublished { get; init; }
        public string Version { get; init; } = string.Empty;
    }

    public record FragmentResponse
    {
        public int FragmentId { get; init; }
        public string FragmentType { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public int Position { get; init; }
        public bool? Active { get; init; }
        public string Version { get; init; } = string.Empty;
    }

    public record PostDetailResponse {
        public int PostId { get; init; }
        public int BlogId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? PathSegment { get; init; }
        public DateTime? DateLastPublished { get; init; }
        public DateTime DateCreated { get; init; }
        public DateTime DateModified { get; init; }
        public string Version { get; init; } = string.Empty;
        public List<FragmentResponse> Fragments { get; init; } = new();
    }

    public record PostListResponseModel
    {
        public List<PostSummaryResponse> Posts { get; init; } = new();
    }
}