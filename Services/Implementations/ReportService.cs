using AutoMapper;
using StudyShare.DTOs.Responses;
using StudyShare.Repositories.Interfaces;
using StudyShare.Services.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;

        public ReportService(IReportRepository reportRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReportResponse>> GetReportsForUserAsync(string userId)
        {
            var reports = await _reportRepository.GetReportsByTargetUserAsync(userId);
            return _mapper.Map<IEnumerable<ReportResponse>>(reports);
        }

        public async Task<bool> ResolveReportAsync(int reportId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null) return false;
            report.IsResolved = true; // Giả sử model Report có field này
            await _reportRepository.UpdateAsync(report);
            return true;
        }
    }
}
