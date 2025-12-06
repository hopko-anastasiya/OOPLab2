using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace OOPLab2.Strategies
{
    public class SAXStrategy : IXmlAnalysisStrategy
    {
        public string StrategyName => "SAX";

        public Task<string> Analyze(string xmlContent, string searchParam)
        {
            var results = new StringBuilder();
            
            var parts = searchParam.Split('|');
            string criteria = parts.Length > 0 ? parts[0] : "";
            string value = parts.Length > 1 ? parts[1] : "";
            
            results.AppendLine($"Результати пошуку за критерієм '{criteria}'");
            results.AppendLine($"Значення: '{value}' (Метод: SAX)");
            results.AppendLine();
            
            bool foundMatch = false;

            // Для зручності зберігаємо дані поточного material
            string? currentAuthor = null, currentFaculty = null, currentDepartment = null, currentTitle = null, currentPages = null, currentType = null, currentDate = null;

            using (var reader = XmlReader.Create(new StringReader(xmlContent)))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "material":
                                currentType = reader.GetAttribute("type");
                                currentAuthor = currentFaculty = currentDepartment = currentTitle = currentPages = currentDate = null;
                                break;
                            case "author":
                                currentFaculty = reader.GetAttribute("faculty");
                                currentDepartment = reader.GetAttribute("department");
                                currentAuthor = reader.ReadElementContentAsString();
                                break;
                            case "title":
                                currentTitle = reader.ReadElementContentAsString();
                                break;
                            case "pages":
                                currentPages = reader.ReadElementContentAsString();
                                break;
                            case "date":
                                currentDate = reader.ReadElementContentAsString();
                                break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "material")
                    {
                        bool match = false;
                        switch (criteria)
                        {
                            case "Спеціальність":
                                match = currentFaculty == value;
                                break;
                            case "Ім'я автора":
                                match = currentAuthor != null && currentAuthor.StartsWith(value, StringComparison.OrdinalIgnoreCase);
                                break;
                            case "Назва роботи":
                                match = currentTitle != null && currentTitle.StartsWith(value, StringComparison.OrdinalIgnoreCase);
                                break;
                        }
                        if (match)
                        {
                            results.AppendLine($"Автор: {currentAuthor} ({currentFaculty}|{currentDepartment})");
                            results.AppendLine($"Назва: {currentTitle} ({currentType})");
                            results.AppendLine($"Сторінок: {currentPages} | Дата: {currentDate}");
                            results.AppendLine("─────────────────────────────────────────────────");
                            foundMatch = true;
                        }
                    }
                }
            }

            if (!foundMatch)
            {
                results.AppendLine("❌ Не знайдено матеріалів за заданим критерієм.");
            }
            results.AppendLine("═════════════════════════════════════════════════════════");
            
            return Task.FromResult(results.ToString());
        }
    }
}
