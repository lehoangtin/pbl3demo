using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class AnswerService : IAnswerService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AnswerService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> CreateAsync(AnswerCreateRequest request, string userId)
        {
            var answer = _mapper.Map<Answer>(request);
            answer.UserId = userId;
            answer.CreatedAt = DateTime.Now;

            _context.Answers.Add(answer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer == null) return false;

            if (!isAdmin && answer.UserId != currentUserId) return false;

            _context.Answers.Remove(answer);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteByUserAsync(int id, string userId)
        {
            var answer = await _context.Answers.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (answer == null) return false;

            _context.Reports.RemoveRange(_context.Reports.Where(r => r.AnswerId == id));
            _context.Answers.Remove(answer);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}