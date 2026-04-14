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
        // 👉 Dùng tên mới (đồng bộ với Program.cs bên minh)
        public static async Task SeedAllAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Users
            var adminEmail = "admin@gmail.com";
            var userEmail = "user@gmail.com";

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

            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new AppUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "Sinh viên Test",
                    EmailConfirmed = true,
                    Points = 100
                };
                await userManager.CreateAsync(user, "User@123");
                await userManager.AddToRoleAsync(user, "User");
            }

            // Lấy user
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            var normalUser = await userManager.FindByEmailAsync(userEmail);

            // 3. Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(new List<Category>
                {
                    new Category { Name = "Công nghệ thông tin", Description = "Lập trình, Database, AI..." },
                    new Category { Name = "Ngoại ngữ", Description = "Tiếng Anh, Tiếng Nhật..." }
                });
                await context.SaveChangesAsync();
            }

            var cat = await context.Categories.FirstAsync();

            // 4. Documents
            if (!context.Documents.Any())
            {
                context.Documents.Add(new Document
                {
                    Title = "Giáo trình C#",
                    Description = "Tài liệu học C#",
                    FileName = "csharp.pdf",
                    FilePath = "/uploads/csharp.pdf",
                    FileType = "application/pdf",
                    FileSize = 100000,
                    UserId = normalUser.Id,
                    CategoryId = cat.Id,
                    IsApproved = true,
                    UploadDate = DateTime.Now
                });
                await context.SaveChangesAsync();
            }

            // 5. Question
            if (!context.Questions.Any())
            {
                context.Questions.Add(new Question
                {
                    Content = "ASP.NET Core kết nối DB như thế nào?",
                    UserId = normalUser.Id,
                    CreatedAt = DateTime.Now
                });
                await context.SaveChangesAsync();
            }

            var question = await context.Questions.FirstAsync();

            // 6. Answer
            if (!context.Answers.Any())
            {
                context.Answers.Add(new Answer
                {
                    Content = "Bạn cần dùng ConnectionString trong appsettings.json",
                    QuestionId = question.Id,
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.Now
                });
                await context.SaveChangesAsync();
            }
        }
    }
}