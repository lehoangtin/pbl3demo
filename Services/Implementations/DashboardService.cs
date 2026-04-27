using AutoMapper;
using StudyShare.Repositories.Interfaces;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;

namespace StudyShare.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepo;
        private readonly IDocumentRepository _docRepo;
        private readonly IQuestionRepository _quesRepo;
        private readonly ICategoryRepository _cateRepo;
        private readonly IAnswerRepository _ansRepo;
        private readonly IMapper _mapper;

        public DashboardService(
            IUserRepository userRepo,
            IDocumentRepository docRepo,
            IQuestionRepository quesRepo,
            ICategoryRepository cateRepo,
            IAnswerRepository ansRepo,
            IMapper mapper)
        {
            _userRepo = userRepo;
            _docRepo = docRepo;
            _quesRepo = quesRepo;
            _cateRepo = cateRepo;
            _ansRepo = ansRepo;
            _mapper = mapper;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardStatsAsync()
        {
            var allUsers = await _userRepo.GetAllAsync();
            var allDocs = await _docRepo.GetAllAsync();

            return new AdminDashboardViewModel
            {
                TotalUsers = allUsers.Count(),
                PendingDocuments = allDocs.Count(d => !d.IsApproved),
                TotalDocuments = allDocs.Count(),
                TotalQuestions = (await _quesRepo.GetAllAsync()).Count()
            };
        }

    }
}
