using System;
using StudyShare.Models;

namespace StudyShare.DTOs.Responses
{
    public class ReportResponse
    {
        public int Id { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public string? ActionTaken { get; set; }

        public string? ReporterName { get; set; }
        public string TargetUserName { get; set; }
        public string? ReportedUserName { get; set; }
        public string? DocumentTitle { get; set; }

        public AppUser? Reporter { get; set; }
        public AppUser? Target { get; set; }
        public Document? Document { get; set; }
        public Question? Question { get; set; }
        public Answer? Answer { get; set; }
        public string? TargetUserId { get; set; }
        public string TargetType { get; set; } = string.Empty; 
        public int? DocumentId { get; set; }
        public int? QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public string? TargetContent { get; set; }
    }
}