namespace ABCD.Server.Requests {
    public class BlogUpdateRequest {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Domains { get; set; } = new();
    }
}
