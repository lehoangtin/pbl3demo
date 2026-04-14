using Microsoft.AspNetCore.Mvc;
using ai.Services;
using System.Threading.Tasks;

namespace ai.Controllers
{
    // Class để hứng dữ liệu từ giao diện gửi lên
    public class UserMessage
    {
        public string text { get; set; }
    }

    public class ChatController : Controller
    {
        private readonly AIService _aiService;

        public ChatController(AIService aiService)
        {
            _aiService = aiService;
        }

        // 1. Hiển thị trang Chat
        [Area("User")]
        public IActionResult Index()
        {
            return View();
        }

        // 2. Nhận tin nhắn và trả về kết quả ngầm (API dùng cho JavaScript)
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] UserMessage msg)
        {
            if (msg == null || string.IsNullOrEmpty(msg.text))
            {
                return Json(new { reply = "Bạn chưa nhập gì cả!" });
            }

            var botReply = await _aiService.ChatWithAIAsync(msg.text);
            return Json(new { reply = botReply });
        }
    }
}