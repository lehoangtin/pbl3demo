using System.Threading.Tasks;
// Khai báo các model dùng chung (nếu bạn để chung file thì mang qua, không thì nên tách ra file Models riêng)
using StudyShare.Services; 

namespace StudyShare.Services.Interfaces
{
    public interface IAIService
    {
        Task<string> ChatWithAIAsync(string message);
        Task<AIModerationResponse> CheckContentAsync(string text);
    }
}