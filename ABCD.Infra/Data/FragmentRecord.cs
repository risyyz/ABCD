using System.ComponentModel.DataAnnotations;

namespace ABCD.Infra.Data {
    public class FragmentRecord {
        public int PostId { get; set; }

        public int Position { get; set; }

        public PostRecord Post { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
    }
}
