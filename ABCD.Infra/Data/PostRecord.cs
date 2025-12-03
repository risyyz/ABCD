using System.Collections.Generic;

namespace ABCD.Infra.Data {
    public class PostRecord {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public int BlogId { get; set; }
        public BlogRecord Blog { get; set; } = null!;
        public ICollection<FragmentRecord> Fragments { get; set; } = new List<FragmentRecord>();
    }
}
