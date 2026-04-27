using Microsoft.EntityFrameworkCore;
using StudyShare.Models;
using StudyShare.Repositories.Interfaces;

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
                .Include(r => r.Target)   // Lấy thông tin người bị báo cáo
                // THÊM 3 DÒNG DƯỚI ĐÂY ĐỂ LẤY NỘI DUNG VI PHẠM THỰC TẾ
                .Include(r => r.Document) 
                .Include(r => r.Question)
                .Include(r => r.Answer)
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
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Target)
                .Include(r => r.Document)
                .Include(r => r.Question)
                .Include(r => r.Answer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Report>> GetAllPendingReportsAsync()
        {
            return await _context.Reports
                .Where(r => !r.IsResolved)
                .Include(r => r.Reporter)
                .Include(r => r.Target)
                // THÊM 3 DÒNG NÀY ĐỂ TRANG DANH SÁCH CŨNG LẤY ĐƯỢC LINK
                .Include(r => r.Document)
                .Include(r => r.Question)
                .Include(r => r.Answer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Report>> GetAllResolvedReportsAsync()
        {
            return await _context.Reports
                .Where(r => r.IsResolved) // Lọc các báo cáo ĐÃ xử lý
                .Include(r => r.Reporter)
                .Include(r => r.Target)
                .Include(r => r.Document)
                .Include(r => r.Question)
                .Include(r => r.Answer)
                .OrderByDescending(r => r.CreatedAt) 
                .ToListAsync();
        }

        public async Task<Report> CreateAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();
            return report; // Trả về để lấy được ID của report sau khi lưu
        }
    }
}