using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;
using StudyShare.Repositories.Interfaces;

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

        public async Task<IEnumerable<ReportResponse>> GetAllReportsAsync()
        {
            var reports = await _reportRepository.GetAllReportsAsync();
            return _mapper.Map<IEnumerable<ReportResponse>>(reports);
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null) return false;

            return await _reportRepository.DeleteAsync(report);
        }
    }
}