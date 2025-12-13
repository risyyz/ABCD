using System.ComponentModel.DataAnnotations;
using ABCD.Domain; // Import the domain namespace for FragmentType

namespace ABCD.Infra.Data {
    public class FragmentRecord {
        public int PostId { get; set; }

        public int Position { get; set; }

        public PostRecord Post { get; set; } = null!;

        public string Content { get; set; } = string.Empty;

        public bool? Excluded { get; set; } // When true, the fragment will not be part of the final post body

        public FragmentType FragmentType { get; set; } // Use enum from domain model

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
    }
}
