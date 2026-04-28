using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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

            // 1. Khởi tạo Roles (Thêm SuperAdmin)
            string[] roles = { "SuperAdmin", "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Khởi tạo Users mẫu
            var usersToSeed = new List<(string Email, string Name, int Points, int Warnings, bool IsBanned, string[] Roles)>
            {
                ("superadmin@studyshare.com", "Trùm Cuối", 9999, 0, false, new[] { "SuperAdmin", "Admin" }),
                ("admin@gmail.com", "Quản trị viên 1", 1000, 0, false, new[] { "Admin" }),
                ("lehoangtin@gmail.com", "Lê Hoàng Tín", 500, 0, false, new[] { "Admin" }),
                ("sinhvien1@gmail.com", "Sinh Viên Chăm Chỉ", 200, 0, false, new[] { "User" }),
                ("sinhvien2@gmail.com", "Sinh Viên Chăm Chỉ", 200, 0, false, new[] { "User" }),
                ("vipham@gmail.com", "User Bị 3 Gậy", 0, 3, false, new[] { "User" }), 
                ("banned@gmail.com", "User Bị Khóa", -50, 5, true, new[] { "User" })   
            };

            foreach (var u in usersToSeed)
            {
                var existingUser = await userManager.FindByEmailAsync(u.Email);
                if (existingUser == null)
                {
                    var user = new AppUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        FullName = u.Name,
                        EmailConfirmed = true,
                        Points = u.Points,
                        WarningCount = u.Warnings, 
                        IsBanned = u.IsBanned      
                    };
                    
                    var result = await userManager.CreateAsync(user, "User@123");
                    if (result.Succeeded)
                    {
                        foreach (var r in u.Roles)
                        {
                            await userManager.AddToRoleAsync(user, r);
                        }
                    }
                }
            }

            // Lấy IDs để làm data liên kết
            var tinUser = await userManager.FindByEmailAsync("lehoangtin@gmail.com");
            var sv1User = await userManager.FindByEmailAsync("sinhvien1@gmail.com");

            // 3. Khởi tạo Categories 
            // ---> ĐÃ XÓA để bạn tự thêm từ Database/Admin <---

            // Lấy 1 category bất kỳ đã có trong DB (do bạn tự thêm) để làm dữ liệu mẫu cho Document
            var defaultCategory = await context.Categories.FirstOrDefaultAsync();

            // 4. Khởi tạo Documents 
            // Chỉ Seed nếu chưa có document nào, có user, và bạn ĐÃ thêm ít nhất 1 Category vào DB
            if (!context.Documents.Any() && tinUser != null && sv1User != null && defaultCategory != null)
            {
                context.Documents.AddRange(new List<Document>
                {
                    new Document { 
                        Title = "Đồ án PBL3 - StudyShare Architecture", 
                        Description = "Tài liệu thiết kế mô hình 3 lớp cho dự án", 
                        FileName = "PBL3_Architecture.pdf", 
                        FilePath = "/uploads/PBL3_Arch.pdf", 
                        FileType = "application/pdf", 
                        FileSize = 1500000, 
                        UserId = tinUser.Id, 
                        CategoryId = defaultCategory.Id, // Dùng category ngẫu nhiên bạn đã tạo
                        IsApproved = true, 
                        UploadDate = DateTime.Now.AddDays(-7) 
                    },
                    new Document { 
                        Title = "Lab cấu hình OSPF & VLAN", 
                        Description = "Bài tập thực hành Cisco Packet Tracer", 
                        FileName = "Lab_Network.pdf", 
                        FilePath = "/uploads/Lab_Network.pdf", 
                        FileType = "application/pdf", 
                        FileSize = 850000, 
                        UserId = sv1User.Id, 
                        CategoryId = defaultCategory.Id, // Dùng category ngẫu nhiên bạn đã tạo
                        IsApproved = true, 
                        UploadDate = DateTime.Now.AddDays(-3) 
                    }
                });
                await context.SaveChangesAsync();
            }

            // 5. Khởi tạo Questions & Answers
            if (!context.Questions.Any() && sv1User != null && tinUser != null)
            {
                var q1 = new Question { 
                    Content = "Làm sao để map Role trong AutoMapper khi dùng Identity?", 
                    UserId = sv1User.Id, 
                    CreatedAt = DateTime.Now.AddDays(-2) 
                };
                context.Questions.Add(q1);
                await context.SaveChangesAsync();

                context.Answers.Add(new Answer { 
                    Content = "Bạn nên map các thuộc tính cơ bản trước, sau đó dùng UserManager.GetRolesAsync để gán Role thủ công vào ViewModel vì hàm này là async.", 
                    QuestionId = q1.Id, 
                    UserId = tinUser.Id, 
                    CreatedAt = DateTime.Now.AddMinutes(-30) 
                });
                await context.SaveChangesAsync();
            }
        }
    }
}