using System.Collections.Generic;

namespace ABCD.Infra.Data {
    public class BlogRecord {
        public int BlogId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<DomainRecord> Domains { get; set; } = new List<DomainRecord>();
        public ICollection<PostRecord> Posts { get; set; } = new List<PostRecord>();
    }
}
