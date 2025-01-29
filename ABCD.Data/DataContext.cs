using ABCD.Core;
using ABCD.Lib;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ABCD.Data {
    public class DataContext : DbContext {

        public DataContext(DbContextOptions<DataContext> options, IOptions<Settings> settings) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>(entity => {
                entity.ToTable("Blogs"); // Set table name
                entity.HasKey(e => e.BlogId);
                entity.Property(e => e.BlogId)
                      .ValueGeneratedOnAdd()
                      .IsRequired();
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.HasMany(e => e.Domains)
                      .WithOne(d => d.Blog)
                      .HasForeignKey(d => d.BlogId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();
            });

            modelBuilder.Entity<BlogDomain>(entity => {
                entity.ToTable("BlogDomains"); // Set table name
                entity.HasKey(e => new { e.BlogId, e.Domain });
                entity.Property(e => e.Domain)
                      .IsRequired()
                      .HasMaxLength(253);

                entity.HasOne(d => d.Blog)
                      .WithMany(b => b.Domains)
                      .HasForeignKey(d => d.BlogId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure other entities if needed
        }



        // Define your DbSets here
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogDomain> BlogDomains { get; set; }
        //public DbSet<Post> Posts { get; set; }
        //public DbSet<PostFragment> PostFragments { get; set; }
    }
}
