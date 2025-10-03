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
                entity.Property(e => e.CreatedDate)
                      .IsRequired();
                entity.Property(e => e.LastUpdatedDate)
                      .IsRequired();
                entity.Property(e => e.CreatedBy)
                      .IsRequired()
                      .HasMaxLength(256);
                entity.Property(e => e.LastUpdatedBy)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.HasMany(e => e.Domains)
                      .WithOne(d => d.Blog)
                      .HasForeignKey(d => d.BlogId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                entity.HasMany(e => e.Posts)
                      .WithOne(p => p.Blog)
                      .HasForeignKey(p => p.BlogId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                // Ignore computed properties
                entity.Ignore(e => e.PublishedPosts);
                entity.Ignore(e => e.DraftPosts);
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
                entity.ToTable("Posts"); // Set table name
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
                      .HasMaxLength(200);
                entity.Property(e => e.Synopsis)
                      .HasMaxLength(2000);
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>();
                entity.Property(e => e.CreatedDate)
                      .IsRequired();
                entity.Property(e => e.LastUpdatedDate)
                      .IsRequired();
                entity.Property(e => e.CreatedBy)
                      .IsRequired()
                      .HasMaxLength(256);
                entity.Property(e => e.LastUpdatedBy)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.HasOne(e => e.Blog)
                      .WithMany(b => b.Posts)
                      .HasForeignKey(e => e.BlogId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                entity.HasIndex(e => new { e.BlogId, e.SeoFriendlyLink })
                      .IsUnique();

                // Ignore computed properties
                entity.Ignore(e => e.ActiveFragments);
            });

            modelBuilder.Entity<Fragment>(entity => {
                entity.ToTable("Fragments"); // Set table name
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
                entity.Property(e => e.CreatedDate)
                      .IsRequired();
                entity.Property(e => e.LastUpdatedDate)
                      .IsRequired();
                entity.Property(e => e.CreatedBy)
                      .IsRequired()
                      .HasMaxLength(256);
                entity.Property(e => e.LastUpdatedBy)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.HasOne(e => e.Post)
                      .WithMany(p => p.Fragments)
                      .HasForeignKey(e => e.PostId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                entity.HasIndex(e => new { e.PostId, e.Position });
            });

            // Configure other entities if needed
        }



        // Define your DbSets here
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogDomain> BlogDomains { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Fragment> Fragments { get; set; }
    }
}
