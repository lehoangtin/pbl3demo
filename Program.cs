using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Services;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using StudyShare.Services.Implementations;
using StudyShare.Mappings;
using StudyShare.Repositories.Interfaces;
using StudyShare.Repositories.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 🔥 AI Service (giữ từ nhánh minh)
// builder.Services.AddHttpClient<ai.Services.AIService>();
builder.Services.AddHttpClient<IAIService, AIService>();
// Database
builder.Services.AddDbContext<AppDbContext>(options =>  
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PBL3ConnectionString")
    ));
builder.Services.AddTransient<IEmailSender, EmailSender>();
// builder.Services.AddHttpClient<ai.Services.AIService>();
// 2. Đăng ký AIService
// Vì AIService của bạn có tiêm HttpClient ở hàm tạo (constructor), 
// nên dùng AddHttpClient là chuẩn nhất trong .NET:
// builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<StudyShare.Services.Interfaces.ICategoryService, StudyShare.Services.Implementations.CategoryService>();
builder.Services.AddScoped<StudyShare.Services.Interfaces.IQuestionService, StudyShare.Services.Implementations.QuestionService>();
builder.Services.AddScoped<StudyShare.Services.Interfaces.IUserService, StudyShare.Services.Implementations.UserService>();
builder.Services.AddScoped<StudyShare.Services.Interfaces.IAnswerService, StudyShare.Services.Implementations.AnswerService>();
builder.Services.AddScoped<StudyShare.Services.Interfaces.IReportService, StudyShare.Services.Implementations.ReportService>();
builder.Services.AddScoped<StudyShare.Services.Interfaces.IDocumentService, StudyShare.Services.Implementations.DocumentService>();
builder.Services.AddScoped<StudyShare.Services.Interfaces.IAuthService, StudyShare.Services.Implementations.AuthService>(); 

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie (giữ 1 cái thôi)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// Mail
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<EmailSender>();

var app = builder.Build();

// 🔥 Seeder (dùng bản mới của bạn)
// 🔥 Seeder (dùng bản mới của bạn)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Lấy AppDbContext từ ServiceProvider
        var context = services.GetRequiredService<AppDbContext>();
        
        // 2. Tự động áp dụng các Migration chưa chạy (Tạo DB và Bảng nếu chưa có)
        await context.Database.MigrateAsync();

        // 3. Chạy Seeder
        await DataSeeder.SeedAllAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi Seeding hoặc Migration: " + ex.Message);
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Areas route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();