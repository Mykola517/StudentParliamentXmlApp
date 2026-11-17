using System;
using System.Collections.Generic;
using StudentParliamentXmlApp.Models;
using StudentParliamentXmlApp.Strategies;

namespace StudentParliamentXmlApp.Services
{
    public class XmlAnalyzerService
    {
        private IXmlAnalysisStrategy? _strategy;

        public void SetStrategy(IXmlAnalysisStrategy strategy)
        {
            _strategy = strategy;
        }

        public string DescribeQuery(SearchCriteria criteria)
        {
            return $"[{Current().Name}] {Current().DescribeQuery(criteria)}";
        }

        public List<ParliamentEvent> Analyze(string filePath, SearchCriteria criteria)
        {
            return Current().Search(filePath, criteria);
        }

        public Dictionary<string, List<string>> GetAllAttributes(string filePath)
        {
            var strategy = Current();
            return new Dictionary<string, List<string>>
            {
                { "Faculty", strategy.GetDistinctValues(filePath, "Faculty") },
                { "Department", strategy.GetDistinctValues(filePath, "Department") },
                { "Specialty", strategy.GetDistinctValues(filePath, "Specialty") },
                { "EventType", strategy.GetDistinctValues(filePath, "EventType") }
            };
        }

        private IXmlAnalysisStrategy Current()
        {
            return _strategy ?? throw new InvalidOperationException("Не обрано метод аналізу XML.");
        }
    }
}