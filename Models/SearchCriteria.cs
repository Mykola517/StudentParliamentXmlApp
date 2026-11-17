using System;
using System.Collections.Generic;

namespace StudentParliamentXmlApp.Models
{
    public class SearchCriteria
    {
        public string? Keyword { get; set; }
        public string? Faculty { get; set; }
        public string? Department { get; set; }
        public string? Specialty { get; set; }
        public string? EventType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string Describe()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Keyword)) parts.Add($"ключове слово «{Keyword}» (ПІБ або опис)");
            if (!string.IsNullOrEmpty(Faculty)) parts.Add($"факультет = «{Faculty}»");
            if (!string.IsNullOrEmpty(Department)) parts.Add($"кафедра = «{Department}»");
            if (!string.IsNullOrEmpty(Specialty)) parts.Add($"спеціальність = «{Specialty}»");
            if (!string.IsNullOrEmpty(EventType)) parts.Add($"вид заходу = «{EventType}»");
            if (From.HasValue) parts.Add($"захід завершується не раніше {From:dd.MM.yyyy}");
            if (To.HasValue) parts.Add($"захід починається не пізніше {To:dd.MM.yyyy}");
            return parts.Count == 0 ? "без обмежень (усі записи)" : string.Join("; ", parts);
        }
    }
}