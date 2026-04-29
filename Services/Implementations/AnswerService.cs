using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces;
using StudyShare.DTOs.Responses; // Thêm dòng này
namespace StudyShare.Services.Implementations
{
    public class AnswerService : IAnswerService
    {
        private readonly IAnswerRepository _answerRepository;
        private readonly IMapper _mapper;

        public AnswerService(IAnswerRepository answerRepository, IMapper mapper)
        {
            _answerRepository = answerRepository;
            _mapper = mapper;
        }

        public async Task<bool> CreateAsync(AnswerCreateRequest request, string userId)
        {
            var answer = _mapper.Map<Answer>(request);
            answer.UserId = userId;
            answer.CreatedAt = DateTime.Now;

            return await _answerRepository.CreateAsync(answer);
        }

        public async Task<bool> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var answer = await _answerRepository.GetByIdAsync(id);
            if (answer == null) return false;

            if (!isAdmin && answer.UserId != currentUserId) return false;

            return await _answerRepository.DeleteAsync(answer);
        }
        public async Task<IEnumerable<AnswerResponse>> GetByQuestionIdAsync(int questionId)
        {
            var answers = await _answerRepository.GetByQuestionIdAsync(questionId);
            return _mapper.Map<IEnumerable<AnswerResponse>>(answers);
        }
        public async Task<bool> DeleteByAdminAsync(int id)
        {
            var answer = await _answerRepository.GetByIdAsync(id);
            if (answer == null) return false;
            return await _answerRepository.DeleteAsync(answer);
        }
    }
}