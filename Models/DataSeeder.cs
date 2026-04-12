using Microsoft.AspNetCore.Identity;
using StudyShare.Models;
using StudyShare.Services;
namespace StudyShare.Models
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. Tạo các Role nếu chưa có
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Tạo tài khoản ADMIN mẫu
            var adminEmail = "admin@gmail.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản trị viên",
                    EmailConfirmed = true // Quan trọng để đăng nhập được ngay
                };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. Tạo tài khoản USER mẫu (Để bạn test)
            var userEmail = "user@gmail.com";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var normalUser = new AppUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "Sinh viên Test",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(normalUser, "User@123");
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }
    }
}