using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using StudentParliamentXmlApp.Models;

namespace StudentParliamentXmlApp.Strategies
{
    public class LinqAnalysisStrategy : IXmlAnalysisStrategy
    {
        public string Name => "LINQ to XML (XDocument)";

        public string DescribeQuery(SearchCriteria criteria) =>
            $"LINQ: doc.Descendants(\"Event\").Where(...), умови: {criteria.Describe()}";

        public List<ParliamentEvent> Search(string filePath, SearchCriteria c)
        {
            var doc = XDocument.Load(filePath);

            IEnumerable<ParliamentEvent> query = doc.Descendants("Event").Select(node => new ParliamentEvent
            {
                FullName = (string?)node.Attribute("FullName") ?? "",
                Faculty = (string?)node.Attribute("Faculty") ?? "",
                Department = (string?)node.Attribute("Department") ?? "",
                Specialty = (string?)node.Attribute("Specialty") ?? "",
                EventType = (string?)node.Attribute("EventType") ?? "",
                TimeFrame = (string?)node.Attribute("TimeFrame") ?? "",
                Role = (string?)node.Attribute("Role") ?? "",
                Course = (string?)node.Attribute("Course") ?? "",
                Email = (string?)node.Attribute("Email") ?? "",
                Description = (string?)node.Element("Description") ?? "",
                AttributeCount = node.Attributes().Count()
            });

            if (!string.IsNullOrEmpty(c.Faculty)) query = query.Where(e => e.Faculty == c.Faculty);
            if (!string.IsNullOrEmpty(c.Department)) query = query.Where(e => e.Department == c.Department);
            if (!string.IsNullOrEmpty(c.Specialty)) query = query.Where(e => e.Specialty == c.Specialty);
            if (!string.IsNullOrEmpty(c.EventType)) query = query.Where(e => e.EventType == c.EventType);

            if (!string.IsNullOrEmpty(c.Keyword))
            {
                var kw = c.Keyword;
                query = query.Where(e =>
                    e.FullName.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    e.Description.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
            }
            if (c.From.HasValue)
            {
                var from = c.From.Value.Date;
                query = query.Where(e => e.End.HasValue && e.End.Value >= from);
            }
            if (c.To.HasValue)
            {
                var to = c.To.Value.Date;
                query = query.Where(e => e.Start.HasValue && e.Start.Value <= to);
            }

            return query.ToList();
        }

        public List<string> GetDistinctValues(string filePath, string attributeName) =>
            XDocument.Load(filePath)
                .Descendants("Event")
                .Select(e => (string?)e.Attribute(attributeName))
                .Where(v => !string.IsNullOrEmpty(v))
                .Select(v => v!)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
    }
}