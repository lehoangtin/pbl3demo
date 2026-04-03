using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var docs = _context.Documents.ToList();
            return View(docs);
        }

        public IActionResult Delete(int id)
        {
            var doc = _context.Documents.Find(id);

            if (doc != null)
            {
                var path = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                _context.Documents.Remove(doc);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}