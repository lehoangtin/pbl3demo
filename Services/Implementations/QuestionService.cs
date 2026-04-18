using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuestionService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<QuestionResponse>> GetAllAsync()
        {
            // Include(q => q.User) để AutoMapper có thể lấy được Tên tác giả
            var questions = await _context.Questions
                                          .Include(q => q.User)
                                          .Include(q => q.Answers)
                                          .OrderByDescending(q => q.CreatedAt)
                                          .ToListAsync();
            return _mapper.Map<IEnumerable<QuestionResponse>>(questions);
        }

        public async Task<QuestionResponse?> GetByIdAsync(int id)
        {
            var question = await _context.Questions
                                         .Include(q => q.User)
                                         .Include(q => q.Answers)
                                            .ThenInclude(a => a.User) // Include người trả lời
                                         .FirstOrDefaultAsync(q => q.Id == id);
            return question == null ? null : _mapper.Map<QuestionResponse>(question);
        }

        public async Task<QuestionUpdateRequest?> GetForEditAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            return question == null ? null : _mapper.Map<QuestionUpdateRequest>(question);
        }

        public async Task<bool> CreateAsync(QuestionCreateRequest request, string userId)
        {
            var question = _mapper.Map<Question>(request);
            question.UserId = userId; // Gán ID người đăng
            question.CreatedAt = DateTime.Now;

            _context.Questions.Add(question);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(QuestionUpdateRequest request, string currentUserId, bool isAdmin)
        {
            var question = await _context.Questions.FindAsync(request.Id);
            if (question == null) return false;

            // Kiểm tra quyền: Chỉ Admin hoặc người tạo mới được sửa
            if (!isAdmin && question.UserId != currentUserId) return false;

            _mapper.Map(request, question); // Map dữ liệu mới đè lên record cũ
            _context.Questions.Update(question);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null) return false;

            if (!isAdmin && question.UserId != currentUserId) return false;

            _context.Questions.Remove(question);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}