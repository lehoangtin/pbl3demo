using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
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
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();    
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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapRazorPages();
app.Run();

