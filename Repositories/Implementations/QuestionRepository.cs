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
                .Include(q => q.User)
                .Include(q => q.Answers)
                .ThenInclude(a => a.User)
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
            // 1. Tìm và xóa tất cả báo cáo liên quan đến câu hỏi này trước
            var relatedReports = _context.Reports.Where(r => r.QuestionId == question.Id);
            _context.Reports.RemoveRange(relatedReports);

            // 2. (Tùy chọn) Nếu câu hỏi có câu trả lời, bạn cũng nên xóa chúng để tránh lỗi tương tự
            var relatedAnswers = _context.Answers.Where(a => a.QuestionId == question.Id);
            _context.Answers.RemoveRange(relatedAnswers);

            // 3. Sau đó mới xóa câu hỏi
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

        public async Task<bool> DeleteQuestionByAdminAsync(Question question)
        {
            var reports = _context.Reports.Where(r => r.QuestionId == question.Id);
            _context.Reports.RemoveRange(reports);
            _context.Questions.Remove(question);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAnswerByAdminAsync(Answer answer)
        {
            _context.Answers.Remove(answer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Question>> GetUserQuestionsAsync(string userId)
        {
            return await _context.Questions.Where(q => q.UserId == userId).Include(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt).ToListAsync();
        }

        public async Task<bool> DeleteByUserAsync(Question question)
        {
            _context.Reports.RemoveRange(_context.Reports.Where(r => r.QuestionId == question.Id));
            var answerIds = _context.Answers.Where(a => a.QuestionId == question.Id).Select(a => a.Id);
            _context.Reports.RemoveRange(_context.Reports.Where(r => r.AnswerId != null && answerIds.Contains(r.AnswerId.Value)));
            _context.Questions.Remove(question);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}