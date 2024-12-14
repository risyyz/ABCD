namespace ABCD.Core {
    public class Blog {
        public int BlogId { get; set; }
        public required string Title { get; set; }
        public ICollection<BlogDomain> Domains { get; set; }
        //public ICollection<Post> Posts { get; set; }
    }

    public class BlogDomain {
        public int BlogId { get; set; }
        public string Domain { get; set; }
        public Blog Blog { get; set; }  
    }
}
