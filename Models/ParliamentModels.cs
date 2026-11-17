using System;
using System.Collections.Generic;
using System.Globalization;

namespace StudentParliamentXmlApp.Models
{
    public class ParliamentEvent
    {
        public string FullName { get; set; } = "";
        public string Faculty { get; set; } = "";
        public string Department { get; set; } = "";
        public string Specialty { get; set; } = "";
        public string EventType { get; set; } = "";
        public string TimeFrame { get; set; } = "";
        public string Role { get; set; } = "";
        public string Course { get; set; } = "";
        public string Email { get; set; } = "";
        public string Description { get; set; } = "";
        public int AttributeCount { get; set; }

        public DateTime? Start => ParsePart(0);
        public DateTime? End => ParsePart(1);

        public string TimeFrameText
        {
            get
            {
                if (Start.HasValue && End.HasValue)
                    return Start.Value == End.Value
                        ? Start.Value.ToString("dd.MM.yyyy")
                        : $"{Start.Value:dd.MM.yyyy} — {End.Value:dd.MM.yyyy}";
                return TimeFrame.Length > 0 ? TimeFrame : "не вказано";
            }
        }

        public string Details
        {
            get
            {
                var parts = new List<string>();
                if (Faculty.Length > 0) parts.Add($"Факультет: {Faculty}");
                if (Department.Length > 0) parts.Add($"Кафедра: {Department}");
                if (Specialty.Length > 0) parts.Add($"Спеціальність: {Specialty}");
                if (Role.Length > 0) parts.Add($"Роль: {Role}");
                if (Course.Length > 0) parts.Add($"Курс: {Course}");
                if (Email.Length > 0) parts.Add($"E-mail: {Email}");
                return string.Join(" · ", parts);
            }
        }

        private DateTime? ParsePart(int index)
        {
            var parts = TimeFrame.Split('/');
            if (parts.Length != 2) return null;
            return DateTime.TryParseExact(parts[index].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d) ? d : null;
        }
    }
}