using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudyShare.ViewModels
{
    public class ReportedUserViewModel 
    {
        // Sử dụng = string.Empty; để khởi tạo giá trị mặc định, tránh lỗi CS8618
        public string UserId { get; set; } = string.Empty;
        
        public string FullName { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public int UniqueReporters { get; set; }
        
        public int TotalReports { get; set; }
        
        public bool IsBanned { get; set; }
    }
}