using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace StudyShare.ViewModels
{
     public class ReportedUserViewModel {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int UniqueReporters { get; set; }
        public int TotalReports { get; set; }
        public bool IsBanned { get; set; }
    }
}