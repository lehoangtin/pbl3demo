using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudyShare.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Answer> Answers { get; set; }
    }
}