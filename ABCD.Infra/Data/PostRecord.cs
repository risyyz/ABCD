using System.Collections.Generic;
using ABCD.Domain; // Import PostStatus enum

namespace ABCD.Infra.Data {
    public class PostRecord {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public int BlogId { get; set; }
        public BlogRecord Blog { get; set; } = null!;
        public ICollection<FragmentRecord> Fragments { get; set; } = new List<FragmentRecord>();

        // New properties
        public PostStatus Status { get; set; } = PostStatus.Draft; // Use enum, default to Draft
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public string Synopsis { get; set; } = string.Empty; // NVarchar(1000) - enforce max length at DB/validation
        public int? ParentPostId { get; set; } // nullable for root posts
        public PostRecord? ParentPost { get; set; } // navigation property (optional)
        public ICollection<PostRecord> ChildPosts { get; set; } = new List<PostRecord>(); // navigation property (optional)
        public string Slug { get; set; } = string.Empty; // URL path to access the post
    }
}
