using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using StudentParliamentXmlApp.Models;

namespace StudentParliamentXmlApp.Strategies
{
    public class DomAnalysisStrategy : IXmlAnalysisStrategy
    {
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";
        private const string Lower = "abcdefghijklmnopqrstuvwxyzабвгґдеєжзиіїйклмнопрстуфхцчшщьюя";

        public string Name => "DOM API (XmlDocument + XPath)";

        public string DescribeQuery(SearchCriteria criteria) => "XPath: " + BuildXPath(criteria);

        public List<ParliamentEvent> Search(string filePath, SearchCriteria criteria)
        {
            var results = new List<ParliamentEvent>();
            var doc = new XmlDocument();
            doc.Load(filePath);

            var nodes = doc.SelectNodes(BuildXPath(criteria));
            if (nodes == null) return results;

            foreach (XmlNode node in nodes)
            {
                results.Add(new ParliamentEvent
                {
                    FullName = Attr(node, "FullName"),
                    Faculty = Attr(node, "Faculty"),
                    Department = Attr(node, "Department"),
                    Specialty = Attr(node, "Specialty"),
                    EventType = Attr(node, "EventType"),
                    TimeFrame = Attr(node, "TimeFrame"),
                    Role = Attr(node, "Role"),
                    Course = Attr(node, "Course"),
                    Email = Attr(node, "Email"),
                    Description = node.SelectSingleNode("Description")?.InnerText ?? "",
                    AttributeCount = node.Attributes?.Count ?? 0
                });
            }
            return results;
        }

        private static string Attr(XmlNode node, string name) => node.Attributes?[name]?.Value ?? "";

        private static string BuildXPath(SearchCriteria c)
        {
            var predicates = new List<string>();

            if (!string.IsNullOrEmpty(c.Keyword))
            {
                var kw = Literal(c.Keyword.ToLowerInvariant());
                predicates.Add(
                    $"(contains(translate(@FullName,{Literal(Upper)},{Literal(Lower)}),{kw}) or " +
                    $"contains(translate(Description,{Literal(Upper)},{Literal(Lower)}),{kw}))");
            }
            if (!string.IsNullOrEmpty(c.Faculty)) predicates.Add($"@Faculty={Literal(c.Faculty)}");
            if (!string.IsNullOrEmpty(c.Department)) predicates.Add($"@Department={Literal(c.Department)}");
            if (!string.IsNullOrEmpty(c.Specialty)) predicates.Add($"@Specialty={Literal(c.Specialty)}");
            if (!string.IsNullOrEmpty(c.EventType)) predicates.Add($"@EventType={Literal(c.EventType)}");

            if (c.From.HasValue)
                predicates.Add($"number(translate(substring-after(@TimeFrame,'/'),'-',''))>={c.From.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}");
            if (c.To.HasValue)
                predicates.Add($"number(translate(substring-before(@TimeFrame,'/'),'-',''))<={c.To.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}");

            return "//Event" + string.Concat(predicates.Select(p => $"[{p}]"));
        }

        private static string Literal(string s)
        {
            if (!s.Contains('\'')) return $"'{s}'";
            if (!s.Contains('"')) return $"\"{s}\"";
            return "concat(" + string.Join(", \"'\", ", s.Split('\'').Select(p => $"'{p}'")) + ")";
        }

        public List<string> GetDistinctValues(string filePath, string attributeName)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);
            var nodes = doc.SelectNodes($"//Event/@{attributeName}");
            if (nodes == null) return new List<string>();

            return nodes.Cast<XmlNode>()
                .Select(n => n.Value ?? "")
                .Where(v => v.Length > 0)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}