using Microsoft.AspNetCore.Mvc;
using ai.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Thêm dòng này

namespace ai.Controllers
{
    public class UserMessage
    {
        public string text { get; set; }
    }

    [Area("User")]
    [Authorize] // Thêm dòng này: Bắt buộc đăng nhập mới được dùng Chat
    public class ChatController : Controller
    {
        private readonly AIService _aiService;

        public ChatController(AIService aiService)
        {
            _aiService = aiService;
        }

        public IActionResult Index()
        {
            return View();
        }

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