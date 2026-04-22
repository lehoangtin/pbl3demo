using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Repositories.Interfaces;
using StudyShare.Models; // Đảm bảo đúng namespace chứa AppDbContext của bạn

namespace StudyShare.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. Thực thi hàm lấy danh sách báo cáo theo User bị tố cáo
        public async Task<IEnumerable<Report>> GetReportsByTargetUserAsync(string userId)
        {
            return await _context.Reports
                .Where(r => r.TargetUserId == userId)
                .Include(r => r.Reporter) // Lấy thông tin người gửi báo cáo
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // 2. Thực thi hàm cập nhật báo cáo (ví dụ: đánh dấu đã xử lý)
        public async Task UpdateAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        // Thêm hàm GetByIdAsync nếu Interface của bạn có yêu cầu
        public async Task<Report?> GetByIdAsync(int id)
        {
            return await _context.Reports.FindAsync(id);
        }
        public async Task<IEnumerable<Report>> GetAllPendingReportsAsync()
        {
            return await _context.Reports
                .Where(r => !r.IsResolved)
                .Include(r => r.Reporter)
                .Include(r => r.Target)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
