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
        private readonly IMapper _mapper;

        public QuestionService(IQuestionRepository questionRepository, IMapper mapper)
        {
            _questionRepository = questionRepository;
            _mapper = mapper;
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
            var question = _mapper.Map<Question>(request);
            question.UserId = userId; // Gán ID người đăng
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
            var answer = await _context.Answers.FindAsync(id); // Note: This still uses _context, but since it's in QuestionService, perhaps move to AnswerRepository later
            if (answer == null) return false;

            return await _questionRepository.DeleteAnswerByAdminAsync(answer);
        }
        public async Task<IEnumerable<Question>> GetUserQuestionsAsync(string userId)
        {
            return await _questionRepository.GetUserQuestionsAsync(userId);
        }

        public async Task<bool> DeleteByUserAsync(int id, string userId)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId); // Still uses _context, but can be adjusted
            if (question == null) return false;

            return await _questionRepository.DeleteByUserAsync(question);
        }
    }
}