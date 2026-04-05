using ABCD.Domain;

namespace ABCD.Infra.Data {
    public class SeriesRecord {
        public int SeriesId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BlogId { get; set; }
        public BlogRecord Blog { get; set; } = null!;
        public string? PathSegment { get; set; }
        public SeriesStatus Status { get; set; } = SeriesStatus.Draft;
        public DateTime? DateLastPublished { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public byte[] Version { get; set; } = Array.Empty<byte>();
        public ICollection<PostRecord> Posts { get; set; } = new List<PostRecord>();
    }
}
