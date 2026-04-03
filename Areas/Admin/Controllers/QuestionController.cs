using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.Models;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var questions = _context.Questions.ToList();
            return View(questions);
        }

        public IActionResult Delete(int id)
        {
            var q = _context.Questions.Find(id);

            if (q != null)
            {
                _context.Questions.Remove(q);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}