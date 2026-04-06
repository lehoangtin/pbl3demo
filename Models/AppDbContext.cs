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
 builder.Entity<Report>(entity =>
    {
        // Khi xóa người báo cáo (Reporter), không xóa Report tự động
        entity.HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterUserId)
            .OnDelete(DeleteBehavior.NoAction); 

        // Khi xóa người bị báo cáo (Target), không xóa Report tự động
        entity.HasOne(r => r.Target)
            .WithMany()
            .HasForeignKey(r => r.TargetUserId)
            .OnDelete(DeleteBehavior.NoAction);
            
        // Tương tự cho Question và Answer nếu cần thiết
        entity.HasOne(r => r.Question)
            .WithMany()
            .HasForeignKey(r => r.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(r => r.Answer)
            .WithMany()
            .HasForeignKey(r => r.AnswerId)
            .OnDelete(DeleteBehavior.NoAction);
    });
        }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }  
        public DbSet<SavedDocument> SavedDocuments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Report> Reports { get; set; }
    }
}