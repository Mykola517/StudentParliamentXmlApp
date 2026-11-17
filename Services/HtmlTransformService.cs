using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using StudentParliamentXmlApp.Models;

namespace StudentParliamentXmlApp.Services
{
    public class HtmlTransformService
    {
        public string Transform(string xmlFilePath, string xsltFilePath, string outputHtmlPath)
        {
            using var input = XmlReader.Create(xmlFilePath);
            return Run(input, xsltFilePath, outputHtmlPath);
        }

        public string TransformResults(IEnumerable<ParliamentEvent> events, string xsltFilePath, string outputHtmlPath)
        {
            var doc = new XDocument(
                new XElement("StudentParliament",
                    events.Select(ToElement)));

            using var input = doc.CreateReader();
            return Run(input, xsltFilePath, outputHtmlPath);
        }

        private static XElement ToElement(ParliamentEvent e)
        {
            var element = new XElement("Event",
                new XAttribute("FullName", e.FullName),
                new XAttribute("Faculty", e.Faculty),
                new XAttribute("Department", e.Department),
                new XAttribute("Specialty", e.Specialty),
                new XAttribute("EventType", e.EventType));

            if (e.TimeFrame.Length > 0) element.Add(new XAttribute("TimeFrame", e.TimeFrame));
            if (e.Role.Length > 0) element.Add(new XAttribute("Role", e.Role));
            if (e.Course.Length > 0) element.Add(new XAttribute("Course", e.Course));
            if (e.Email.Length > 0) element.Add(new XAttribute("Email", e.Email));
            if (e.Description.Length > 0) element.Add(new XElement("Description", e.Description));
            return element;
        }

        private static string Run(XmlReader input, string xsltFilePath, string outputHtmlPath)
        {
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltFilePath);

            using var output = new MemoryStream();
            xslt.Transform(input, null, output);

            var bytes = output.ToArray();
            File.WriteAllBytes(outputHtmlPath, bytes);
            return xslt.OutputSettings.Encoding.GetString(bytes).TrimStart('\uFEFF');
        }
    }
}