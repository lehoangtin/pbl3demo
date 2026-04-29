using StudyShare.Models;
using StudyShare.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace StudyShare.Repositories.Implementations
{
    public class AnswerRepository : IAnswerRepository   
    {
        private readonly AppDbContext _context;

        public AnswerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Answer?> GetByIdAsync(int id)
        {
            return await _context.Answers.FindAsync(id);
        }

        public async Task<Answer?> GetByIdAndUserAsync(int id, string userId)
        {
            return await _context.Answers.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<bool> CreateAsync(Answer answer)
        {
            _context.Answers.Add(answer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Answer answer)
        {
            var relatedReports = _context.Reports.Where(r => r.AnswerId == answer.Id);
            if (relatedReports.Any())
            {
                _context.Reports.RemoveRange(relatedReports);
            }

            _context.Answers.Remove(answer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Answer>> GetAllAsync()
        {
            return await _context.Answers.ToListAsync();
        }
        public async Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId)
        {
            return await _context.Answers
                .Include(a => a.User)
                .Where(a => a.QuestionId == questionId)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

    }
}