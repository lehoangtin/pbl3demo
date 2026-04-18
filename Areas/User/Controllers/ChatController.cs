using Microsoft.AspNetCore.Mvc;
using ai.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; 

namespace StudyShare.Areas.User.Controllers
{
    [Area("User")]
    public class UserMessage
    {
        public string text { get; set; }
    }

    [Area("User")]
    // Xóa (hoặc comment) dòng [Authorize] ở cấp độ Controller
    // [Authorize] 
    public class ChatController : Controller
    {
        private readonly AIService _aiService;

        public ChatController(AIService aiService)
        {
            _aiService = aiService;
        }

        // Nếu bạn muốn truy cập HẲN vào trang /User/Chat thì mới bắt đăng nhập, 
        // hãy chuyển [Authorize] xuống đây.
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
                return Json(new { reply = "Bạn cần đăng nhập để thảo luận với trợ lý AI nhé!" });
            }

            // 2. Kiểm tra tin nhắn rỗng
            if (msg == null || string.IsNullOrEmpty(msg.text))
            {
                return Json(new { reply = "Bạn chưa nhập gì cả!" });
            }

            // 3. Gọi AI
            var botReply = await _aiService.ChatWithAIAsync(msg.text);
            return Json(new { reply = botReply });
        }
    }
}