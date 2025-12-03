using ABCD.Lib;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ABCD.Infra.Data {
    public class DataContext : DbContext {

        public DataContext(DbContextOptions<DataContext> options, IOptions<Settings> settings) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BlogRecord>(entity => {
                entity.ToTable("Blogs");
                entity.HasKey(e => e.BlogId);
                entity.Property(e => e.BlogId).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description);
                entity.HasMany(e => e.Domains)
                      .WithOne(e => e.Blog)
                      .HasForeignKey(e => e.BlogId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Posts)
                      .WithOne(e => e.Blog)
                      .HasForeignKey(e => e.BlogId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DomainRecord>(entity => {
                entity.ToTable("Domains");
                entity.HasKey(e => new { e.BlogId, e.Domain });
                entity.Property(e => e.Domain).IsRequired().HasMaxLength(253);
                entity.Property(e => e.BlogId).IsRequired();
            });

            modelBuilder.Entity<PostRecord>(entity => {
                entity.ToTable("Posts");
                entity.HasKey(e => e.PostId);
                entity.Property(e => e.PostId).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
                entity.Property(e => e.Content);
                entity.Property(e => e.BlogId).IsRequired();
                entity.HasMany(e => e.Fragments)
                      .WithOne(e => e.Post)
                      .HasForeignKey(e => e.PostId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FragmentRecord>(entity => {
                entity.ToTable("Fragments");
                entity.HasKey(e => new { e.PostId, e.Position });
                entity.Property(e => e.PostId).IsRequired();
                entity.Property(e => e.Position).IsRequired();
                entity.Property(e => e.Content).IsRequired();
            });
        }

        public DbSet<BlogRecord> Blogs { get; set; }
        public DbSet<DomainRecord> Domains { get; set; }
        public DbSet<PostRecord> Posts { get; set; }
        public DbSet<FragmentRecord> Fragments { get; set; }
    }
}
