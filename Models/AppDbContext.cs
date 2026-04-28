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

            // 1. Cấu hình QUESTION
            builder.Entity<Question>(entity => {
                // Khi xóa User -> KHÔNG xóa Question (Tránh lỗi đa đường truyền - Multiple Cascade Paths)
                entity.HasOne(q => q.User)
                    .WithMany(u => u.Questions)
                    .HasForeignKey(q => q.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 2. Cấu hình ANSWER
            builder.Entity<Answer>(entity => {
                // Khi xóa Question -> Xóa sạch Answer (Hợp lý)
                entity.HasOne(a => a.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Khi xóa User -> KHÔNG xóa Answer
                entity.HasOne(a => a.User)
                    .WithMany() 
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 3. Cấu hình SAVED DOCUMENT (Lưu tài liệu)
            builder.Entity<SavedDocument>(entity => {
                // Xóa tài liệu gốc -> Xóa luôn bản lưu của User
                entity.HasOne(d => d.Document)
                    .WithMany()
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(u => u.SavedDocuments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 4. Cấu hình REPORT (Khắc phục lỗi bạn vừa gặp)
            builder.Entity<Report>(entity => {
                // Đối với Người dùng liên quan: Để NoAction để tránh vòng lặp xóa
                entity.HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterUserId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(r => r.Target).WithMany().HasForeignKey(r => r.TargetUserId).OnDelete(DeleteBehavior.NoAction);

                // ĐỐI VỚI NỘI DUNG: Chuyển sang CASCADE để khi xóa nội dung thì Report tự biến mất
                // Đây chính là chìa khóa để fix lỗi crash lúc nãy!
                
                entity.HasOne(r => r.Document)
                    .WithMany() // Nếu Document.cs có ICollection<Report> thì thay bằng .WithMany(d => d.Reports)
                    .HasForeignKey(r => r.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Question)
                    .WithMany() // Nếu Question.cs có ICollection<Report> thì thay bằng .WithMany(q => q.Reports)
                    .HasForeignKey(r => r.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Answer)
                    .WithMany() // Nếu Answer.cs có ICollection<Report> thì thay bằng .WithMany(a => a.Reports)
                    .HasForeignKey(r => r.AnswerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // Đăng ký các bảng dữ liệu
        public DbSet<Document> Documents { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }  
        public DbSet<SavedDocument> SavedDocuments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Report> Reports { get; set; }
    }
}