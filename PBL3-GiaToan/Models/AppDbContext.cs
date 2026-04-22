using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace StudyShare.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

      protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // 1. Cấu hình Question: Khi xóa User, KHÔNG tự động xóa Question (Tránh vòng lặp)
    builder.Entity<Question>(entity => {
        entity.HasOne(q => q.User)
            .WithMany(u => u.Questions)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.NoAction); // 🔥 Đổi sang NoAction
    });

    // 2. Cấu hình Answer
    builder.Entity<Answer>(entity => {
        // Khi xóa Question -> Xóa sạch Answer liên quan (Được phép)
        entity.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Khi xóa User -> KHÔNG tự động xóa Answer (Tránh vòng lặp)
        entity.HasOne(a => a.User)
            .WithMany() 
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.NoAction); // 🔥 Đổi sang NoAction
    });

    // 3. Cấu hình SavedDocument
    builder.Entity<SavedDocument>(entity => {
        entity.HasOne(d => d.Document)
            .WithMany()
            .HasForeignKey(d => d.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(d => d.User)
            .WithMany(u => u.SavedDocuments)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    });

    // 4. Cấu hình Report (Bắt buộc tất cả NoAction)
    builder.Entity<Report>(entity => {
        entity.HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterUserId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(r => r.Target).WithMany().HasForeignKey(r => r.TargetUserId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(r => r.Document).WithMany().HasForeignKey(r => r.DocumentId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(r => r.Question).WithMany().HasForeignKey(r => r.QuestionId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(r => r.Answer).WithMany().HasForeignKey(r => r.AnswerId).OnDelete(DeleteBehavior.NoAction);
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