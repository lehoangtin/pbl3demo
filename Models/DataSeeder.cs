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

            // 1. Khởi tạo Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Khởi tạo Users với các mức điểm khác nhau
            var usersToSeed = new List<(string Email, string Name, int Points, string Role)>
            {
                ("admin@gmail.com", "Quản trị viên", 999, "Admin"),
                ("lehoangtin@gmail.com", "Lê Hoàng Tín", 500, "User"),
                ("sinhvien1@gmail.com", "Sinh Viên Chăm Chỉ", 150, "User"),
                ("sinhvien2@gmail.com", "Sinh Viên Vi Phạm", -10, "User"), // Điểm < 0 để test tính năng ban account
                ("user@gmail.com", "Người Dùng Thông Thường", 50, "User") // Điểm thấp để test cảnh cáo
            };

            foreach (var u in usersToSeed)
            {
                if (await userManager.FindByEmailAsync(u.Email) == null)
                {
                    var user = new AppUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        FullName = u.Name,
                        EmailConfirmed = true,
                        Points = u.Points
                    };
                    await userManager.CreateAsync(user, "User@123");
                    await userManager.AddToRoleAsync(user, u.Role);
                }
            }

            // Lấy lại các user để liên kết với Document và Question
            var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
            var tinUser = await userManager.FindByEmailAsync("lehoangtin@gmail.com");
            var sv1User = await userManager.FindByEmailAsync("sinhvien1@gmail.com");

            // 3. Khởi tạo Categories (Đa dạng chủ đề học thuật)
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(new List<Category>
                {
                    new Category { Name = "Công nghệ phần mềm", Description = "C#, ASP.NET MVC, Java Swing..." },
                    new Category { Name = "Mạng máy tính", Description = "Cisco Packet Tracer, NAT, DHCP, VLSM..." },
                    new Category { Name = "Toán chuyên ngành", Description = "Đại số tuyến tính, SVD, Cholesky..." },
                    new Category { Name = "Cơ sở dữ liệu", Description = "SQL Server, MySQL, Docker Compose..." },
                    new Category { Name = "Ngoại ngữ", Description = "Tài liệu ôn thi JLPT N5, N4, N2..." },
                    new Category { Name = "Thuật toán", Description = "Master Theorem, Phân tích độ phức tạp..." }
                });
                await context.SaveChangesAsync();
            }

            var catSoftware = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Công nghệ phần mềm");
            var catNetwork = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Mạng máy tính");
            var catMath = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Toán chuyên ngành");
            var catLang = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Ngoại ngữ");

            // 4. Khởi tạo Documents (Nhiều loại file và tình trạng duyệt)
            if (!context.Documents.Any() && catSoftware != null && catNetwork != null)
            {
                context.Documents.AddRange(new List<Document>
                {
                    new Document { Title = "Đồ án PBL3 - StudyShare", Description = "Mã nguồn và báo cáo PBL3", FileName = "PBL3_BaoCao.docx", FilePath = "/uploads/PBL3_BaoCao.docx", FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileSize = 2500000, UserId = tinUser.Id, CategoryId = catSoftware.Id, IsApproved = true, UploadDate = DateTime.Now.AddDays(-10) },
                    new Document { Title = "Slide NAT PAT DHCP", Description = "Hướng dẫn cấu hình Cisco Router", FileName = "3. NAT PAT DHCP.pdf", FilePath = "/uploads/3_NAT_PAT_DHCP.pdf", FileType = "application/pdf", FileSize = 1048576, UserId = sv1User.Id, CategoryId = catNetwork.Id, IsApproved = true, UploadDate = DateTime.Now.AddDays(-5) },
                    new Document { Title = "Phân tích ma trận SVD", Description = "Code C++ implement SVD không dùng thư viện", FileName = "svd_algorithm.pdf", FilePath = "/uploads/svd_algorithm.pdf", FileType = "application/pdf", FileSize = 512000, UserId = tinUser.Id, CategoryId = catMath.Id, IsApproved = false, UploadDate = DateTime.Now.AddDays(-1) },
                    new Document { Title = "Từ vựng & Ngữ pháp N2", Description = "Tổng hợp Kanji và Choukai N2", FileName = "JLPT_N2.pdf", FilePath = "/uploads/JLPT_N2.pdf", FileType = "application/pdf", FileSize = 4500000, UserId = sv1User.Id, CategoryId = catLang.Id, IsApproved = true, UploadDate = DateTime.Now.AddDays(-2) }
                });
                await context.SaveChangesAsync();
            }

            // 5. Khởi tạo Questions
            if (!context.Questions.Any())
            {
                context.Questions.AddRange(new List<Question>
                {
                    new Question { Content = "Mọi người cho mình hỏi, phép dịch trái (bitwise shift left) có phải là tương đương với căn bậc 2 không?", UserId = sv1User.Id, CreatedAt = DateTime.Now.AddDays(-3) },
                    new Question { Content = "Định nghĩa về số nguyên tố cùng nhau (coprime). Hai số nguyên tố cùng nhau thì bản thân mỗi số có bắt buộc phải là số nguyên tố không?", UserId = tinUser.Id, CreatedAt = DateTime.Now.AddDays(-2) },
                    new Question { Content = "Làm sao để triển khai SQL Server bằng Docker Compose và kết nối từ ASP.NET Core?", UserId = sv1User.Id, CreatedAt = DateTime.Now.AddHours(-10) }
                });
                await context.SaveChangesAsync();
            }

            var qBitwise = await context.Questions.FirstOrDefaultAsync(q => q.Content.Contains("phép dịch trái"));
            var qCoprime = await context.Questions.FirstOrDefaultAsync(q => q.Content.Contains("coprime"));
            var qDocker = await context.Questions.FirstOrDefaultAsync(q => q.Content.Contains("Docker Compose"));

            // 6. Khởi tạo Answers
            if (!context.Answers.Any())
            {
                var answers = new List<Answer>();

                if (qBitwise != null)
                {
                    answers.Add(new Answer { Content = "Không phải nhé bạn. Dịch trái 1 bit tương đương với việc nhân số đó cho 2, còn dịch phải 1 bit là chia cho 2. Hoàn toàn không liên quan đến căn bậc 2.", QuestionId = qBitwise.Id, UserId = tinUser.Id, CreatedAt = DateTime.Now.AddDays(-2) });
                }

                if (qCoprime != null)
                {
                    answers.Add(new Answer { Content = "Không bắt buộc bạn nhé. Hai số được gọi là nguyên tố cùng nhau khi và chỉ khi ước chung lớn nhất (ƯCLN) của chúng bằng 1. Ví dụ số 8 và 9 đều không phải là số nguyên tố, nhưng chúng là hai số nguyên tố cùng nhau.", QuestionId = qCoprime.Id, UserId = adminUser.Id, CreatedAt = DateTime.Now.AddDays(-1) });
                }

                if (qDocker != null)
                {
                    answers.Add(new Answer { Content = "Bạn tạo file docker-compose.yml dùng image mcr.microsoft.com/mssql/server. Trong ASP.NET Core thì dùng connection string trỏ tới localhost kèm port đã map (thường là 1433).", QuestionId = qDocker.Id, UserId = tinUser.Id, CreatedAt = DateTime.Now.AddHours(-2) });
                }

                if (answers.Any())
                {
                    context.Answers.AddRange(answers);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}