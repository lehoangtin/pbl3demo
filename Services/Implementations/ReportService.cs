using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.DTOs.Responses;
using StudyShare.Services.Interfaces;

namespace StudyShare.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReportService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReportResponse>> GetAllReportsAsync()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Target)
                .Include(r => r.Document)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ReportResponse>>(reports);
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            _context.Reports.Remove(report);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}