using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using StudentParliamentXmlApp.Models;

namespace StudentParliamentXmlApp.Strategies
{
    public class SaxAnalysisStrategy : IXmlAnalysisStrategy
    {
        public string Name => "SAX API (XmlReader)";

        public string DescribeQuery(SearchCriteria criteria) =>
            $"Послідовне читання XmlReader, кожен Event перевіряється за умовами: {criteria.Describe()}";

        public List<ParliamentEvent> Search(string filePath, SearchCriteria criteria)
        {
            var results = new List<ParliamentEvent>();
            var settings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };

            using var reader = XmlReader.Create(filePath, settings);
            reader.MoveToContent();

            while (!reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Event")
                {
                    var ev = ReadEvent(reader);
                    if (IsMatch(ev, criteria)) results.Add(ev);
                }
                else
                {
                    reader.Read();
                }
            }
            return results;
        }

        private static ParliamentEvent ReadEvent(XmlReader reader)
        {
            var ev = new ParliamentEvent
            {
                FullName = reader.GetAttribute("FullName") ?? "",
                Faculty = reader.GetAttribute("Faculty") ?? "",
                Department = reader.GetAttribute("Department") ?? "",
                Specialty = reader.GetAttribute("Specialty") ?? "",
                EventType = reader.GetAttribute("EventType") ?? "",
                TimeFrame = reader.GetAttribute("TimeFrame") ?? "",
                Role = reader.GetAttribute("Role") ?? "",
                Course = reader.GetAttribute("Course") ?? "",
                Email = reader.GetAttribute("Email") ?? "",
                AttributeCount = reader.AttributeCount
            };

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return ev;
            }

            int depth = reader.Depth;
            reader.Read();

            while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth))
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Description")
                    ev.Description = reader.ReadElementContentAsString();
                else
                    reader.Read();
            }

            reader.Read();
            return ev;
        }

        private static bool IsMatch(ParliamentEvent ev, SearchCriteria c)
        {
            if (!string.IsNullOrEmpty(c.Faculty) && ev.Faculty != c.Faculty) return false;
            if (!string.IsNullOrEmpty(c.Department) && ev.Department != c.Department) return false;
            if (!string.IsNullOrEmpty(c.Specialty) && ev.Specialty != c.Specialty) return false;
            if (!string.IsNullOrEmpty(c.EventType) && ev.EventType != c.EventType) return false;

            if (!string.IsNullOrEmpty(c.Keyword))
            {
                bool found = ev.FullName.Contains(c.Keyword, StringComparison.OrdinalIgnoreCase) ||
                             ev.Description.Contains(c.Keyword, StringComparison.OrdinalIgnoreCase);
                if (!found) return false;
            }

            if (c.From.HasValue && !(ev.End.HasValue && ev.End.Value >= c.From.Value.Date)) return false;
            if (c.To.HasValue && !(ev.Start.HasValue && ev.Start.Value <= c.To.Value.Date)) return false;
            return true;
        }

        public List<string> GetDistinctValues(string filePath, string attributeName)
        {
            var values = new HashSet<string>();
            using var reader = XmlReader.Create(filePath);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Event")
                {
                    var v = reader.GetAttribute(attributeName);
                    if (!string.IsNullOrEmpty(v)) values.Add(v);
                }
            }
            return values.OrderBy(x => x).ToList();
        }
    }
}