using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPLab2
{
    public interface IXmlAnalysisStrategy
    {
        string StrategyName { get; }

        // Метод для виконання аналізу XML
        Task<string> Analyze(string xmlContent, string searchAttributeValue);
    }
}
