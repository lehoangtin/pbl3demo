using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
// Đăng ký dịch vụ gọi API AI
builder.Services.AddHttpClient<ai.Services.AIService>();
// DB
builder.Services.AddDbContext<AppDbContext>(options =>  
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PBL3ConnectionString")
    ));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Hoặc đường dẫn cụ thể đến trang đăng nhập của bạn
    options.AccessDeniedPath = "/Account/AccessDenied";
});
// Identity
// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedEmail = true; 
    
    // 🔥 Cấu hình mật khẩu mạnh (Dành cho Production)
    options.Password.RequiredLength = 8;              // Tối thiểu 8 ký tự
    options.Password.RequireDigit = true;             // Yêu cầu có số (0-9)
    options.Password.RequireNonAlphanumeric = true;   // Yêu cầu ký tự đặc biệt (@, #, $, %...)
    options.Password.RequireUppercase = true;         // Yêu cầu ít nhất 1 chữ hoa (A-Z)
    options.Password.RequireLowercase = true;         // Yêu cầu ít nhất 1 chữ thường (a-z)
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Ghi nhớ đăng nhập 30 ngày
});
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<EmailSender>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DataSeeder.SeedRolesAndUsersAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi Seeding: " + ex.Message);
    }
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Thêm đoạn này TRƯỚC default route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Đây là đoạn mặc định đã có của bạn
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapRazorPages();
app.Run();

