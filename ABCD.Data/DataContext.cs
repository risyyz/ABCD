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
                entity.ToTable("Blogs");
                entity.HasKey(e => e.BlogId);

                entity.Property(e => e.BlogId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Ignore(b => b.Domains); 
                entity.HasMany<BlogDomain>("_domains")
                      .WithOne()
                      .HasForeignKey(p => p.BlogId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Enforce restrict delete

                entity.Ignore(b => b.Posts);
                entity.HasMany<Post>("_posts")
                     .WithOne()
                     .HasForeignKey(p => p.BlogId)
                     .IsRequired()
                     .OnDelete(DeleteBehavior.Restrict); // Enforce restrict delete
            });

            modelBuilder.Entity<BlogDomain>(entity => {
                entity.ToTable("BlogDomains");

                // Composite primary key
                entity.HasKey(e => new { e.BlogId, e.DomainName });

                // Make Domain required and set max length if needed
                entity.Property(e => e.DomainName)
                    .HasConversion(
                        v => v.Value, // to store in db
                        v => new DomainName(v)) // to read from db
                    .IsRequired()
                    .HasMaxLength(253); // Use a suitable max length for domains

                // Make BlogId required (already enforced by being part of PK and constructor)
                entity.Property(e => e.BlogId)
                    .IsRequired();
            });

            modelBuilder.Entity<Post>(entity => {
                entity.ToTable("Posts");
                entity.HasKey(e => e.PostId);

                entity.Property(e => e.PostId)
                      .ValueGeneratedOnAdd(); // Auto-increment identity

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.Property(e => e.BlogId)
                      .IsRequired();
            });
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogDomain> BlogDomains { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}
