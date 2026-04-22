using Microsoft.AspNetCore.Mvc;
using ai.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; 
using System.Collections.Generic;
using System.Linq;
using StudyShare.Models; // <-- ĐÃ THÊM DÒNG NÀY ĐỂ FIX LỖI

namespace ai.Controllers
{
    public class UserMessage
    {
        public string text { get; set; }
    }

    [Area("User")]
    public class ChatController : Controller
    {
        private readonly AIService _aiService;
        private readonly UserManager<AppUser> _userManager; // Sử dụng UserManager để trừ điểm

        public ChatController(AIService aiService, UserManager<AppUser> userManager)
        {
            _aiService = aiService;
            _userManager = userManager;
        }

        [Authorize] 
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] UserMessage msg)
        {
            // 1. Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { isViolation = false, reply = "Bạn cần đăng nhập để thảo luận với trợ lý AI nhé!" });
            }

            // 2. Kiểm tra tin nhắn rỗng
            if (msg == null || string.IsNullOrWhiteSpace(msg.text))
            {
                return Json(new { isViolation = false, reply = "Bạn chưa nhập gì cả!" });
            }

            // Chuyển tin nhắn về chữ thường để quét từ khóa
            string userText = msg.text.ToLower();

            // 3. TÍNH NĂNG: KIỂM DUYỆT TỪ NGỮ VÀ TRỪ ĐIỂM
            var badWords = new List<string> { "ngu", "dốt", "đần", "mẹ mày", "cút" }; 
            
            bool isViolation = badWords.Any(word => userText.Contains(word));

            if (isViolation)
            {
                // Lấy thông tin user hiện tại từ Database
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Trừ 10 điểm
                    user.Points -= 10; 
                    await _userManager.UpdateAsync(user);

                    // Trả về JSON báo lỗi vi phạm để Frontend hiển thị cảnh báo đỏ
                    return Json(new { 
                        isViolation = true, 
                        message = "Tin nhắn của bạn chứa từ ngữ không phù hợp với chuẩn mực học tập. Bạn đã bị trừ 10 điểm vào tài khoản!" 
                    });
                }
            }

            // 4. NẾU KHÔNG VI PHẠM: Gọi AI bình thường
            var botReply = await _aiService.ChatWithAIAsync(msg.text);
            
            // Trả về JSON thành công kèm câu trả lời của AI
            return Json(new { 
                isViolation = false, 
                reply = botReply 
            });
        }
    }
}