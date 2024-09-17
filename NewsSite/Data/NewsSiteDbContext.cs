using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsSite.Models;
using Newtonsoft.Json;
using System.Reflection.Emit;

namespace NewsSite.Data
{
    public class NewsSiteDbContext : IdentityDbContext<User>
    {
        public NewsSiteDbContext(DbContextOptions<NewsSiteDbContext> options) : base(options)
        {

        }
        public DbSet<News> News { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Comments> Comments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasMany(n => n.News)
                .WithOne(c => c.Category)
                .HasForeignKey(n => n.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            });
            modelBuilder.Entity<News>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Introduction)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.HasMany(e => e.Comments)
                     .WithOne(c => c.News)
                     .HasForeignKey(c => c.NewsId)
                     .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Author)
                      .WithMany(u => u.News)
                      .HasForeignKey(n => n.AuthorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.Entity<Models.Comments>(entity =>
            {
                entity.HasOne(x => x.Author)
                      .WithMany()
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Reaction>(entity =>
            {
                entity.HasOne(r => r.News)
                      .WithMany(n => n.Reactions)
                      .HasForeignKey(r => r.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reactions)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
                

            base.OnModelCreating(modelBuilder);
        }
    }
}
