using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(string searchTerm)
    {
        var query = _context.Documents
            .Include(d => d.User)
            .Where(d => d.IsApproved) // Chỉ hiện file đã duyệt
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(d => d.Title.Contains(searchTerm) || d.Description.Contains(searchTerm));
        }

        var docs = await query.OrderByDescending(d => d.UploadDate).ToListAsync();
        ViewBag.SearchTerm = searchTerm;
        return View(docs);
    }
}