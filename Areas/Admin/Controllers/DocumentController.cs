using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using StudyShare.Services.Interfaces;
using System.Security.Claims; // Thêm thư viện này để lấy thông tin User đang đăng nhập

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;

        // Tiêm Service thay vì DbContext
        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public async Task<IActionResult> Index(string search)
        {
            var documents = await _documentService.GetAllForAdminAsync(search);
            return View(documents);
        }

        public async Task<IActionResult> Review(int id)
        {
            var document = await _documentService.GetDetailsForReviewAsync(id);
            if (document == null) return NotFound();

            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var success = await _documentService.ApproveDocumentAsync(id);
            if (success)
            {
                TempData["Success"] = "Tài liệu đã được phê duyệt thành công và người dùng đã được cộng 10đ!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy tài liệu hoặc tài liệu đã được duyệt.";
            }

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Lấy ID của admin hiện tại
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();
            
            // Gọi hàm xóa qua Service (đã tích hợp sẵn logic xóa file vật lý trong Service của bạn)
            var success = await _documentService.DeleteAsync(id, currentUserId, isAdmin: true);
            
            if (!success) return NotFound();

            TempData["Success"] = "Đã xóa / từ chối tài liệu thành công!";
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }
}