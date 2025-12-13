using System.ComponentModel.DataAnnotations;

namespace ABCD.Infra.Data {
    public class DomainRecord {
        public int BlogId { get; set; }
        public string Domain { get; set; } = string.Empty;
        public BlogRecord Blog { get; set; } = null!;
    }
}
