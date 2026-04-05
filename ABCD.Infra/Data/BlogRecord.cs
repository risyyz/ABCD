namespace ABCD.Infra.Data {
    public class BlogRecord {
        public int BlogId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AiChatSystemPrompt { get; set; }
        public string? AiGeneratePostSystemPrompt { get; set; }
        public int MaxSeriesDepth { get; set; } = 2;
        public ICollection<DomainRecord> Domains { get; set; } = new List<DomainRecord>();
        public ICollection<PostRecord> Posts { get; set; } = new List<PostRecord>();
        public ICollection<SeriesRecord> Series { get; set; } = new List<SeriesRecord>();
    }
}
