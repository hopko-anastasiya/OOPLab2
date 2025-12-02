using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPLab2
{
    public class XmlProcessorContext
    {
        private IXmlAnalysisStrategy _strategy;

        // Встановлення/зміна стратегії
        public void SetStrategy(IXmlAnalysisStrategy strategy)
        {
            _strategy = strategy;
        }

        // Виконання аналізу за допомогою поточної стратегії
        public Task<string> ExecuteAnalysis(string xmlContent, string searchAttributeValue)
        {
            if (_strategy == null)
            {
                return Task.FromResult("Помилка: Стратегія аналізу не вибрана.");
            }

            return _strategy.Analyze(xmlContent, searchAttributeValue);
        }
    }
}
