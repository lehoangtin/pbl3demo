using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IAnswerRepository _answerRepository; // Add AnswerRepository for admin delete answer function
        private readonly IUserService _userService;
        private readonly IAIService _aiService;
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public QuestionService(IQuestionRepository questionRepository, IAnswerRepository answerRepository, IMapper mapper,IUserService userService,       // Inject UserService
            IAIService aiService,           // Inject AIService
            IReportService reportService)
        {
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _mapper = mapper;
            _userService = userService;
            _aiService = aiService;
            _reportService = reportService;
        }

        public async Task<IEnumerable<QuestionResponse>> GetAllAsync()
        {
            // Include(q => q.User) để AutoMapper có thể lấy được Tên tác giả
            var questions = await _questionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<QuestionResponse>>(questions);
        }

        public async Task<QuestionResponse?> GetByIdAsync(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            return question == null ? null : _mapper.Map<QuestionResponse>(question);
        }

        public async Task<QuestionUpdateRequest?> GetForEditAsync(int id)
        {
            var question = await _questionRepository.GetForEditAsync(id);
            return question == null ? null : _mapper.Map<QuestionUpdateRequest>(question);
        }

public async Task<bool> CreateAsync(QuestionCreateRequest request, string userId)
{
    // 1. AI KIỂM TRA NỘI DUNG
    var contentToCheck = request.Content + " " + request.Content;
    var aiResult = await _aiService.CheckContentAsync(contentToCheck);

    if (aiResult.isFlagged) 
    {
        // 2. PHẠT NGƯỜI DÙNG (Trừ 20 điểm, tăng 1 gậy)
        await _userService.PenalizeUserAsync(userId, 10, 1); 

        // 3. LƯU VÀO LỊCH SỬ VI PHẠM (Để Admin và User cùng thấy)
        await _reportService.CreateAutoReportAsync(
            userId, 
            $"Nội dung vi phạm: {request.Content}. (Lý do AI: {aiResult.reason})", 
            "Hệ thống (AI) tự động phạt trừ 10 điểm và tăng 1 gậy cảnh cáo.",
            null, 
            null
        );
        
        return false; // Trả về false để Controller báo lỗi ra màn hình
    }

    // Nếu không vi phạm thì mới thực hiện lưu câu hỏi như bình thường...
    var question = _mapper.Map<Question>(request);
    question.UserId = userId; 
    question.CreatedAt = DateTime.Now;
    return await _questionRepository.CreateAsync(question);
}

        public async Task<bool> UpdateAsync(QuestionUpdateRequest request, string currentUserId, bool isAdmin)
        {
            var question = await _questionRepository.GetForEditAsync(request.Id);
            if (question == null) return false;

            // Kiểm tra quyền: Chỉ Admin hoặc người tạo mới được sửa
            if (!isAdmin && question.UserId != currentUserId) return false;

            _mapper.Map(request, question); // Map dữ liệu mới đè lên record cũ
            return await _questionRepository.UpdateAsync(question);
        }

        public async Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var question = await _questionRepository.GetForEditAsync(id);
            if (question == null) return false;

            if (!isAdmin && question.UserId != currentUserId) return false;

            return await _questionRepository.DeleteAsync(question);
        }// Thêm các hàm này vào trong class QuestionService

        public async Task<IEnumerable<Question>> GetAllForAdminAsync()
        {
            return await _questionRepository.GetAllForAdminAsync();
        }

        public async Task<Question?> GetDetailsForAdminAsync(int id)
        {
            return await _questionRepository.GetDetailsForAdminAsync(id);
        }

        public async Task<IEnumerable<Report>> GetReportsForQuestionAsync(int questionId)
        {
            return await _questionRepository.GetReportsForQuestionAsync(questionId);
        }

        public async Task<bool> DeleteQuestionByAdminAsync(int id)
        {
            var question = await _questionRepository.GetForEditAsync(id);
            if (question == null) return false;

            return await _questionRepository.DeleteQuestionByAdminAsync(question);
        }

        public async Task<bool> DeleteAnswerByAdminAsync(int id)
        {
            var answer = await _answerRepository.GetByIdAsync(id); // Note: This still uses _context, but since it's in QuestionService, perhaps move to AnswerRepository later
            if (answer == null) return false;

            return await _questionRepository.DeleteAnswerByAdminAsync(answer);
        }
        public async Task<IEnumerable<Question>> GetUserQuestionsAsync(string userId)
        {
            return await _questionRepository.GetUserQuestionsAsync(userId);
        }

        public async Task<bool> DeleteByUserAsync(int id, string userId)
        {
            var question = await _questionRepository.GetByIdAsync(id); // Still uses _context, but can be adjusted
            if (question == null || question.UserId != userId) return false;

            return await _questionRepository.DeleteByUserAsync(question);
        }
        public async Task<bool> DeleteByAdminAsync(int id) 
        {
            var item = await _questionRepository.GetByIdAsync(id); // Đổi thành _questionRepository nếu ở QuestionService
            if (item == null) return false;
            await _questionRepository.DeleteAsync(item);
            return true;
        }

    }
}