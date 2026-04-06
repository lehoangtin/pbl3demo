using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace StudyShare.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
             base.OnModelCreating(builder);

            builder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany()
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 🔥 FIX

            builder.Entity<Answer>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 🔥 FIX
            builder.Entity<SavedDocument>(entity =>
            {
                entity.HasOne(d => d.Document)
                    .WithMany()
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.NoAction); // ❌ Ngắt xóa liên đới ở đây

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction); // ❌ Ngắt luôn ở đây
            });
 // Ngắt luôn ở đây cho chắc chắn
        }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }  
        public DbSet<SavedDocument> SavedDocuments { get; set; }
    }
}