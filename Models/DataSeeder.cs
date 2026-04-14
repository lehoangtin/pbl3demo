using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudyShare.Models
{
    public static class DataSeeder
    {
        public static async Task SeedAllAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. Khởi tạo Quyền (Roles)
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Khởi tạo Người dùng (Users)
            var adminEmail = "admin@gmail.com";
            var u1Email = "sinhvien1@gmail.com";
            var u2Email = "sinhvien2@gmail.com";

            // Tạo Admin
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new AppUser 
                { 
                    UserName = adminEmail, 
                    Email = adminEmail, 
                    FullName = "Quản trị viên", 
                    EmailConfirmed = true,
                    Points = 999 
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Tạo Sinh viên 1
            if (await userManager.FindByEmailAsync(u1Email) == null)
            {
                var user1 = new AppUser 
                { 
                    UserName = u1Email, 
                    Email = u1Email, 
                    FullName = "Nguyễn Văn A", 
                    EmailConfirmed = true, 
                    Points = 100 
                };
                await userManager.CreateAsync(user1, "User@123");
                await userManager.AddToRoleAsync(user1, "User");
            }

            // Tạo Sinh viên 2
            if (await userManager.FindByEmailAsync(u2Email) == null)
            {
                var user2 = new AppUser 
                { 
                    UserName = u2Email, 
                    Email = u2Email, 
                    FullName = "Trần Thị B", 
                    EmailConfirmed = true, 
                    Points = 50 
                };
                await userManager.CreateAsync(user2, "User@123");
                await userManager.AddToRoleAsync(user2, "User");
            }

            // Lấy đối tượng User đã tạo để gán ID cho các bảng sau
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            var user1Obj = await userManager.FindByEmailAsync(u1Email);
            var user2Obj = await userManager.FindByEmailAsync(u2Email);

            // 3. Khởi tạo Danh mục (Categories)
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(new List<Category>
                {
                    new Category { Name = "Công nghệ thông tin", Description = "Lập trình, Database, AI..." },
                    new Category { Name = "Kinh tế", Description = "Marketing, Kế toán, Tài chính..." },
                    new Category { Name = "Ngoại ngữ", Description = "Tiếng Anh, Tiếng Nhật..." }
                });
                await context.SaveChangesAsync();
            }
            var cat = await context.Categories.FirstAsync();

            // 4. Khởi tạo Tài liệu (Documents)
            if (!context.Documents.Any())
            {
                context.Documents.AddRange(new List<Document>
                {
                    new Document 
                    { 
                        Title = "Giáo trình C# Nâng Cao", 
                        Description = "Tài liệu học lập trình C# từ cơ bản đến nâng cao", 
                        FileName = "CSharp_Advanced.pdf", 
                        FilePath = "/uploads/csharp.pdf", 
                        FileType = "application/pdf", 
                        FileSize = 1024000,
                        UserId = user1Obj.Id, 
                        CategoryId = cat.Id,
                        IsApproved = true,
                        UploadDate = DateTime.Now.AddDays(-5)
                    },
                    new Document 
                    { 
                        Title = "Bài tập cấu trúc dữ liệu", 
                        Description = "Tổng hợp các bài toán giải thuật phổ biến", 
                        FileName = "DSA_Exercise.docx", 
                        FilePath = "/uploads/dsa.docx", 
                        FileType = "application/msword", 
                        FileSize = 512000,
                        UserId = user2Obj.Id, 
                        CategoryId = cat.Id,
                        IsApproved = true,
                        UploadDate = DateTime.Now.AddDays(-2)
                    }
                });
                await context.SaveChangesAsync();
            }
            var doc1 = await context.Documents.FirstAsync();

            // 5. Khởi tạo Câu hỏi (Questions)
            if (!context.Questions.Any())
            {
                context.Questions.Add(new Question 
                { 
                    Content = "Làm sao để kết nối SQL Server trong ASP.NET Core?", 
                    UserId = user2Obj.Id,
                    CreatedAt = DateTime.Now.AddHours(-10)
                });
                await context.SaveChangesAsync();
            }
            var question = await context.Questions.FirstAsync();

            // 6. Khởi tạo Câu trả lời (Answers)
            if (!context.Answers.Any())
            {
                context.Answers.Add(new Answer 
                { 
                    Content = "Bạn cần cấu hình ConnectionString trong file appsettings.json.", 
                    QuestionId = question.Id, 
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.Now.AddHours(-5)
                });
                await context.SaveChangesAsync();
            }

            // 7. Khởi tạo Báo cáo vi phạm (Reports)
            if (!context.Reports.Any())
            {
                context.Reports.AddRange(new List<Report>
                {
                    // Kịch bản 1: Sinh viên 2 báo cáo tài liệu của Sinh viên 1
                    new Report 
                    { 
                        ReporterUserId = user2Obj.Id, 
                        TargetUserId = user1Obj.Id, 
                        DocumentId = doc1.Id, 
                        Reason = "Tài liệu chứa nội dung không chính xác và vi phạm bản quyền.",
                        CreatedAt = DateTime.Now.AddDays(-1)
                    },
                    // Kịch bản 2: Admin báo cáo Sinh viên 2 vì spam nội dung
                    new Report 
                    { 
                        ReporterUserId = adminUser.Id, 
                        TargetUserId = user2Obj.Id, 
                        Reason = "Người dùng cố tình đăng nội dung rác nhiều lần.",
                        CreatedAt = DateTime.Now.AddHours(-2)
                    }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}