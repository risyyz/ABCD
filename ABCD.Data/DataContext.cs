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

                // Audit fields
                entity.Property(e => e.CreatedDate)
                      .IsRequired();
                entity.Property(e => e.LastUpdatedDate)
                      .IsRequired();
                entity.Property(e => e.CreatedBy)
                      .IsRequired()
                      .HasMaxLength(450);
                entity.Property(e => e.LastUpdatedBy)
                      .IsRequired()
                      .HasMaxLength(450);

                entity.HasMany(e => e.Domains)
                      .WithOne(d => d.Blog)
                      .HasForeignKey(d => d.BlogId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired()
                      .Metadata.PrincipalToDependent!.SetField("_domains");

                entity.HasMany(e => e.Posts)
                      .WithOne(p => p.Blog)
                      .HasForeignKey(p => p.BlogId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .Metadata.PrincipalToDependent!.SetField("_posts");
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

            modelBuilder.Entity<Post>(entity => {
                entity.ToTable("Posts");
                entity.HasKey(e => e.PostId);
                entity.Property(e => e.PostId)
                      .ValueGeneratedOnAdd()
                      .IsRequired();
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(e => e.SeoFriendlyLink)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(e => e.Category)
                      .HasMaxLength(100);
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>();
                entity.Property(e => e.Synopsis)
                      .HasMaxLength(1000);
                
                // Audit fields
                entity.Property(e => e.CreatedDate)
                      .IsRequired();
                entity.Property(e => e.LastUpdatedDate)
                      .IsRequired();
                entity.Property(e => e.CreatedBy)
                      .IsRequired()
                      .HasMaxLength(450);
                entity.Property(e => e.LastUpdatedBy)
                      .IsRequired()
                      .HasMaxLength(450);

                // Relationships
                entity.HasOne(p => p.Blog)
                      .WithMany(b => b.Posts)
                      .HasForeignKey(p => p.BlogId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                entity.HasMany(p => p.Fragments)
                      .WithOne(f => f.Post)
                      .HasForeignKey(f => f.PostId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .Metadata.PrincipalToDependent!.SetField("_fragments");

                // Unique constraint on SeoFriendlyLink within a Blog
                entity.HasIndex(e => new { e.BlogId, e.SeoFriendlyLink })
                      .IsUnique();
            });

            modelBuilder.Entity<Fragment>(entity => {
                entity.ToTable("Fragments");
                entity.HasKey(e => e.FragmentId);
                entity.Property(e => e.FragmentId)
                      .ValueGeneratedOnAdd()
                      .IsRequired();
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(e => e.Content)
                      .IsRequired();
                entity.Property(e => e.Type)
                      .IsRequired()
                      .HasConversion<string>();
                entity.Property(e => e.Position)
                      .IsRequired();
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>();
                
                // Audit fields
                entity.Property(e => e.CreatedDate)
                      .IsRequired();
                entity.Property(e => e.LastUpdatedDate)
                      .IsRequired();
                entity.Property(e => e.CreatedBy)
                      .IsRequired()
                      .HasMaxLength(450);
                entity.Property(e => e.LastUpdatedBy)
                      .IsRequired()
                      .HasMaxLength(450);

                // Relationships
                entity.HasOne(f => f.Post)
                      .WithMany(p => p.Fragments)
                      .HasForeignKey(f => f.PostId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                // Index on PostId and Position for efficient ordering
                entity.HasIndex(e => new { e.PostId, e.Position });
            });
        }



        // Define your DbSets here
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogDomain> BlogDomains { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Fragment> Fragments { get; set; }
    }
}
