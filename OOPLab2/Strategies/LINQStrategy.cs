using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OOPLab2.Strategies
{
    public class LINQStrategy : IXmlAnalysisStrategy
    {
        public string StrategyName => "LINQ";

        public Task<string> Analyze(string xmlContent, string searchParam)
        {
            var results = new StringBuilder();
            var doc = XDocument.Parse(xmlContent);

            // searchParam: "Критерій|Значення"
            var parts = searchParam.Split('|');
            string criteria = parts.Length > 0 ? parts[0] : "";
            string value = parts.Length > 1 ? parts[1] : "";

            results.AppendLine($"Результати пошуку за критерієм '{criteria}'");
            results.AppendLine($"Значення: '{value}' (Метод: LINQ)");
            results.AppendLine();

            IEnumerable<XElement> matchingMaterials = Enumerable.Empty<XElement>();
            switch (criteria)
            {
                case "Спеціальність":
                    matchingMaterials = doc.Descendants("material")
                        .Where(m => m.Element("author")?.Attribute("faculty")?.Value == value);
                    break;
                case "Ім'я автора":
                    matchingMaterials = doc.Descendants("material")
                        .Where(m => m.Element("author")?.Value != null && m.Element("author").Value.StartsWith(value, StringComparison.OrdinalIgnoreCase));
                    break;
                case "Назва роботи":
                    matchingMaterials = doc.Descendants("material")
                        .Where(m => m.Element("title") != null && m.Element("title")!.Value.StartsWith(value, StringComparison.OrdinalIgnoreCase));
                    break;
                default:
                    results.AppendLine("Невідомий критерій пошуку.");
                    break;
            }

            if (matchingMaterials.Any())
            {
                foreach (var m in matchingMaterials)
                {
                    string author = m.Element("author")?.Value;
                    string faculty = m.Element("author")?.Attribute("faculty")?.Value;
                    string department = m.Element("author")?.Attribute("department")?.Value;
                    string title = m.Element("title")?.Value;
                    string pages = m.Element("pages")?.Value;
                    string type = m.Attribute("type")?.Value;
                    string date = m.Element("date")?.Value;
                    results.AppendLine($"Автор: {author} ({faculty}|{department})");
                    results.AppendLine($"Назва: {title} ({type})");
                    results.AppendLine($"Сторінок: {pages} | Дата: {date}");
                    results.AppendLine("─────────────────────────────────────────────────");
                }
            }
            else
            {
                results.AppendLine("❌ Не знайдено матеріалів за заданим критерієм.");
            }
            results.AppendLine("═════════════════════════════════════════════════════════");
            return Task.FromResult(results.ToString());
        }
    }
}
