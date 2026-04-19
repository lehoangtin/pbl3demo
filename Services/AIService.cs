using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ai.Services
{
    // Lớp này dùng để hứng kết quả trả về từ Python
    public class AIModerationResponse
    {
        public bool isFlagged { get; set; }
        public string reason { get; set; }
    }

    public class ChatResponse
    {
        public string reply { get; set; }
    }

    public class AIService
    {
        // Hàm gửi tin nhắn Chat sang Python và nhận phản hồi
        public async Task<string> ChatWithAIAsync(string message)
        {
            var payload = new { message = message };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/api/chat", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ChatResponse>(responseString);
                    return result?.reply ?? "Xin lỗi, không thể xử lý phản hồi từ AI.";
                }
            }
            catch { }

            return "Xin lỗi, Server AI đang ngủ trưa. Vui lòng thử lại sau!";
        }

        private readonly HttpClient _httpClient;

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Hàm gửi nội dung sang Python kiểm tra và nhận kết quả có bị chặn hay không
        public async Task<AIModerationResponse> CheckContentAsync(string text)
        {
            var payload = new { text = text };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                // Gửi sang Python
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/api/moderate", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<AIModerationResponse>(responseString, options);

                    return result ?? new AIModerationResponse { isFlagged = false, reason = "An toàn" };
                }
                else
                {
                    // Nếu Python từ chối/ báo lỗi, ép C# hiển thị lỗi đó ra
                    return new AIModerationResponse { isFlagged = true, reason = $"Python báo lỗi HTTP: {response.StatusCode}" };
                }
            }
            catch (Exception ex)
            {
                // NẾU C# KHÔNG TÌM THẤY PYTHON, IN THẲNG LỖI ĐÓ RA MÀN HÌNH ĐỂ DEBUG!
                return new AIModerationResponse { isFlagged = true, reason = $"Lỗi kết nối C#: {ex.Message}" };
            }
        }
    }
}