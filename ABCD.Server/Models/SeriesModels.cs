namespace ABCD.Server.Models {
    public record CreateSeriesRequest {
        public required string Title { get; init; }
        public required string Path { get; init; }
        public string? Description { get; init; }
    }

    public record SeriesUpdateRequest(
        string Title,
        string? Description,
        string PathSegment,
        string Version
    );

    public record ToggleSeriesStatusRequest {
        public string Version { get; init; } = string.Empty;
    }

    public record AddPostToSeriesRequest {
        public int PostId { get; init; }
        public int Position { get; init; }
        public string Version { get; init; } = string.Empty;
    }

    public record RemovePostFromSeriesRequest {
        public int PostId { get; init; }
        public string Version { get; init; } = string.Empty;
    }

    public record SeriesSummaryResponse {
        public int SeriesId { get; init; }
        public int BlogId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? PathSegment { get; init; }
        public DateTime? DateLastPublished { get; init; }
        public string Version { get; init; } = string.Empty;
        public int PostCount { get; init; }
    }

    public record SeriesDetailResponse {
        public int SeriesId { get; init; }
        public int BlogId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? PathSegment { get; init; }
        public DateTime? DateLastPublished { get; init; }
        public string Version { get; init; } = string.Empty;
        public List<SeriesPostResponse> Posts { get; init; } = new();
    }

    public record SeriesPostResponse {
        public int PostId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? PathSegment { get; init; }
        public int Position { get; init; }
    }

    public record PublicSeriesSummaryResponse {
        public int SeriesId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Url { get; init; }
        public DateTime? DateLastPublished { get; init; }
        public int PostCount { get; init; }
    }

    public record PublicSeriesDetailResponse {
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime? DateLastPublished { get; init; }
        public List<PublicSeriesPostResponse> Posts { get; init; } = new();
    }

    public record PublicSeriesPostResponse {
        public string Title { get; init; } = string.Empty;
        public string? Url { get; init; }
        public int Position { get; init; }
    }
}
