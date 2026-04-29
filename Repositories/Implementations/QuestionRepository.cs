using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Repositories.Interfaces;

namespace StudyShare.Repositories.Implementations
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            return await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            return await _context.Questions
            .Include(q => q.User)        // Kéo thông tin người tạo câu hỏi
            .Include(q => q.Answers)     // Kéo danh sách bình luận
            .ThenInclude(a => a.User) // Kéo thông tin tác giả của từng bình luận đó
            .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Question?> GetForEditAsync(int id)
        {
            return await _context.Questions.FindAsync(id);
        }

        public async Task<bool> CreateAsync(Question question)
        {
            _context.Questions.Add(question);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            return await _context.SaveChangesAsync() > 0;
        }

       public async Task<bool> DeleteAsync(Question question)
{
    // 1. Xóa báo cáo của câu hỏi
    var questionReports = _context.Reports.Where(r => r.QuestionId == question.Id);
    if (questionReports.Any()) _context.Reports.RemoveRange(questionReports);

    // 2. Xóa báo cáo của các câu trả lời
    var answerIds = _context.Answers.Where(a => a.QuestionId == question.Id).Select(a => a.Id).ToList();
    if (answerIds.Any())
    {
        var answerReports = _context.Reports.Where(r => r.AnswerId.HasValue && answerIds.Contains(r.AnswerId.Value));
        _context.Reports.RemoveRange(answerReports);
    }

    // 3. Xóa câu hỏi
    _context.Questions.Remove(question);
    return await _context.SaveChangesAsync() > 0;
}

        public async Task<IEnumerable<Question>> GetAllForAdminAsync()
        {
            return await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Question?> GetDetailsForAdminAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.User)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<Report>> GetReportsForQuestionAsync(int questionId)
        {
            return await _context.Reports
                .Where(r => r.QuestionId == questionId)
                .Include(r => r.Reporter)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetUserQuestionsAsync(string userId)
        {
            return await _context.Questions.Where(q => q.UserId == userId).Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt).ToListAsync();
        }
    }
}