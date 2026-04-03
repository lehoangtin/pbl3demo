using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;
using System.Security.Claims;
namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private readonly string[] allowedExtensions = 
            { ".pdf", ".docx", ".pptx", ".xlsx" };

        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

        public DocumentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // LIST
        public IActionResult Index()
        {
            var docs = _context.Documents.ToList();
            return View(docs);
        }

        // CREATE VIEW
        public IActionResult Create()
        {
            return View();
        }

        // UPLOAD
        [HttpPost]
        public async Task<IActionResult> Create(Document doc, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                ModelState.AddModelError("", "Chọn file!");
                return View(doc);
            }

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();

                // ❌ validate type
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", $"File {file.FileName} không hợp lệ!");
                    continue;
                }

                // ❌ validate size
                if (file.Length > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("", $"File {file.FileName} quá lớn!");
                    continue;
                }

                // ✅ rename file
                var newFileName = Guid.NewGuid() + ext;
                var path = Path.Combine(uploadPath, newFileName);

                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                // lưu DB (mỗi file 1 record)
                var newDoc = new Document
                {
                    Title = doc.Title,
                    Description = doc.Description,
                    FilePath = "/uploads/" + newFileName,
                    FileName = file.FileName,
                    FileType = ext,
                    FileSize = file.Length
                };

                _context.Documents.Add(newDoc);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // DOWNLOAD
        public IActionResult Download(int id)
        {
            var doc = _context.Documents.Find(id);

            if (doc == null) return NotFound();

            var path = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));

            doc.DownloadCount++;
            _context.SaveChanges();

            return PhysicalFile(path, "application/octet-stream", doc.FileName);
        }

        // DELETE
        public IActionResult Delete(int id)
        {
            var doc = _context.Documents.Find(id);

            if (doc == null) return NotFound();

            var path = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            _context.Documents.Remove(doc);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // DETAILS + PREVIEW
        public IActionResult Details(int id)
        {
            var doc = _context.Documents.Find(id);
            if (doc == null) return NotFound();

            return View(doc);
        }
    }
}