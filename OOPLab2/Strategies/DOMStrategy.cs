using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Xml;

namespace OOPLab2.Strategies
{
   public class DOMStrategy : IXmlAnalysisStrategy
    {
        public string StrategyName => "DOM";

        public Task<string> Analyze(string xmlContent, string searchParam)
        {
            var results = new StringBuilder();
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            var parts = searchParam.Split('|');
            string criteria = parts.Length > 0 ? parts[0] : "";
            string value = parts.Length > 1 ? parts[1] : "";


            results.AppendLine($"Результати пошуку за критерієм '{criteria}'");
            results.AppendLine($"Значення: '{value}' (Метод: DOM)");
            results.AppendLine();

            XmlNodeList materialNodes = null;
            switch (criteria)
            {
                case "Спеціальність":
                    materialNodes = doc.SelectNodes($"//material[author[@faculty='{value}']]");
                    break;
                case "Ім'я автора":
                    materialNodes = doc.SelectNodes($"//material[starts-with(author, '{value}')]");
                    break;
                case "Назва роботи":
                    materialNodes = doc.SelectNodes($"//material[starts-with(title, '{value}')]");
                    break;
                default:
                    results.AppendLine("Невідомий критерій пошуку.");
                    break;
            }

            if (materialNodes != null && materialNodes.Count > 0)
            {
                foreach (XmlNode materialNode in materialNodes)
                {
                    XmlNode authorNode = materialNode.SelectSingleNode("author");
                    string authorName = authorNode?.InnerText;
                    string faculty = authorNode?.Attributes?["faculty"]?.Value;
                    string department = authorNode?.Attributes?["department"]?.Value;
                    string title = materialNode.SelectSingleNode("title")?.InnerText;
                    string pages = materialNode.SelectSingleNode("pages")?.InnerText;
                    string type = materialNode.Attributes["type"]?.Value;
                    string date = materialNode.SelectSingleNode("date")?.InnerText;
                    results.AppendLine($"Автор: {authorName} ({faculty}|{department})");
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
