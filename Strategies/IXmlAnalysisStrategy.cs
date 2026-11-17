using System.Collections.Generic;
using StudentParliamentXmlApp.Models;

namespace StudentParliamentXmlApp.Strategies
{
    public interface IXmlAnalysisStrategy
    {
        string Name { get; }
        string DescribeQuery(SearchCriteria criteria);
        List<ParliamentEvent> Search(string filePath, SearchCriteria criteria);
        List<string> GetDistinctValues(string filePath, string attributeName);
    }
}