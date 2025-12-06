using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using OOPLab2.Strategies;

namespace OOPLab2
{
    public partial class MainPage : ContentPage
    {
        // Шлях до XML файлу
        private string? _xmlFilePath;

        // Вміст XML файлу
        private string? _xmlContent;

        // Вбудований XSL для трансформації
        private const string EmbeddedXsl = @"<?xml version=""1.0"" encoding=""utf-8""?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
    <xsl:output method=""html"" encoding=""UTF-8"" indent=""yes""/>
    <xsl:template match=""/archive"">
        <html>
            <head>
                <title>Звіт: <xsl:value-of select=""@name""/></title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    table { border-collapse: collapse; width: 80%; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                    th { background-color: #f2f2f2; }
                </style>
            </head>
            <body>
                <h1>Електронний Архів: <xsl:value-of select=""@name""/></h1>
                <p>Дата Створення: <b><xsl:value-of select=""@creationDate""/></b></p>
                <table>
                    <tr>
                        <th>Автор (Ф-т/Каф)</th>
                        <th>Назва Матеріалу</th>
                        <th>Вид</th>
                        <th>Обсяг (стор.)</th>
                        <th>Дата</th>
                    </tr>
                    <xsl:apply-templates select=""material""/>
                </table>
            </body>
        </html>
    </xsl:template>
    <xsl:template match=""material"">
        <tr>
            <td><xsl:value-of select=""author""/> (<xsl:value-of select=""author/@faculty""/> / <xsl:value-of select=""author/@department""/>)</td>
            <td><xsl:value-of select=""title""/></td>
            <td><xsl:value-of select=""@type""/></td>
            <td><xsl:value-of select=""pages""/></td>
            <td><xsl:value-of select=""date""/></td>
        </tr>
    </xsl:template>
</xsl:stylesheet>";

        // Контекст для Стратегії
        private readonly XmlProcessorContext _context = new();

        public MainPage()
        {
            InitializeComponent();

            // Встановлення початкової стратегії (SAX)
            _context.SetStrategy(new SAXStrategy());
        }

        // Обробник для діалогу підтвердження виходу (використовуємо життєвий цикл MAUI)
        protected override bool OnBackButtonPressed()
        {
            // Перехоплюємо натискання "назад" (або закриття сторінки) і показуємо підтвердження
            Dispatcher.Dispatch(async () =>
            {
                bool answer = await DisplayAlert("Підтвердження Виходу", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
                if (answer)
                {
                    try
                    {
                        Application.Current?.Quit();
                    }
                    catch
                    {
                        // Якщо платформа не підтримує Quit, просто повернемося назад
                        await Shell.Current.GoToAsync("..");
                    }
                }
            });

            // Повертаємо true, щоб зупинити стандартну навігацію назад — ми обробляємо це самостійно
            return true;
        }


        // --- 1. Завантаження Файлів ---

        private string _selectedCriteria = "Спеціальність";
        private string _searchValue = "";

        private async void LoadXmlButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var xmlFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/xml", "text/xml" } },
                    { DevicePlatform.iOS, new[] { "public.xml" } },
                    { DevicePlatform.WinUI, new[] { ".xml" } },
                    { DevicePlatform.macOS, new[] { "public.xml" } },
                });

                var options = new PickOptions
                {
                    PickerTitle = "Виберіть XML-файл",
                    FileTypes = xmlFileType
                };

                var result = await FilePicker.Default.PickAsync(options);

                if (result != null)
                {
                    _xmlFilePath = result.FullPath;
                    _xmlContent = File.ReadAllText(_xmlFilePath);
                    XmlFileLabel.Text = $"XML-файл: {result.FileName}";

                    // Активація кнопок
                    TransformButton.IsEnabled = true;
                    AnalyzeButton.IsEnabled = !string.IsNullOrWhiteSpace(_searchValue) && !string.IsNullOrWhiteSpace(_selectedCriteria);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка завантаження XML", ex.Message, "OK");
            }
        }

        // --- Обробники для пошуку ---
        private void SearchCriteriaPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SearchCriteriaPicker.SelectedIndex != -1 && SearchCriteriaPicker.SelectedItem != null)
            {
                _selectedCriteria = SearchCriteriaPicker.SelectedItem.ToString();
                UpdateSearchPlaceholder(_selectedCriteria);
                SearchValueEntry.Text = string.Empty;
            }
            else
            {
                _selectedCriteria = "";
                SearchValueEntry.Placeholder = "Виберіть критерій...";
                SearchValueEntry.Text = string.Empty;
            }
            AnalyzeButton.IsEnabled = !string.IsNullOrWhiteSpace(_searchValue) && !string.IsNullOrWhiteSpace(_selectedCriteria);
        }

        private void UpdateSearchPlaceholder(string criteria)
        {
            SearchValueEntry.Placeholder = criteria switch
            {
                "Спеціальність" => "Наприклад: КН",
                "Ім'я автора" => "Наприклад: Петренко О.В.",
                "Назва роботи" => "Наприклад: Розробка веб-застосунку",
                _ => "Введіть значення..."
            };
        }

        private void SearchValueEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchValue = e.NewTextValue;
            AnalyzeButton.IsEnabled = !string.IsNullOrWhiteSpace(_searchValue) && !string.IsNullOrWhiteSpace(_selectedCriteria);
        }

        // --- 3. Вибір Методу Аналізу (Стратегії) ---
        private void StrategyRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (OutputEditor == null)
            {
                return;
            }
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            string content = radio.Content?.ToString() ?? "";
            IXmlAnalysisStrategy? newStrategy = content switch
            {
                "SAX" => new SAXStrategy(),
                "DOM" => new DOMStrategy(),
                "LINQ" => new LINQStrategy(),
                _ => null
            };

            if (newStrategy != null)
            {
                _context.SetStrategy(newStrategy);
                OutputEditor.Text = $"Стратегія змінена на: **{newStrategy.StrategyName}**";
            }
        }

        // --- 4. Кнопки Дій ---

        private async void AnalyzeButton_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_xmlContent))
            {
                OutputEditor.Text = "Помилка: XML-файл не завантажено.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedCriteria) || string.IsNullOrWhiteSpace(_searchValue))
            {
                OutputEditor.Text = "Помилка: Не вибрано критерій або не введено значення для пошуку.";
                return;
            }

            try
            {
                // Передаємо критерій і значення у стратегію
                string result = await _context.ExecuteAnalysis(_xmlContent, $"{_selectedCriteria}|{_searchValue}");
                OutputEditor.Text = result;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка Аналізу", ex.Message, "OK");
                OutputEditor.Text = $"Помилка під час аналізу: {ex.Message}";
            }
        }

        private async void TransformButton_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_xmlContent))
            {
                OutputEditor.Text = "Помилка: XML-файл не завантажено.";
                return;
            }

            try
            {
                // Використовуємо вбудований XSL
                var xslt = new XslCompiledTransform();
                using (var xslReader = XmlReader.Create(new StringReader(EmbeddedXsl)))
                {
                    xslt.Load(xslReader);
                }

                // Завантаження XML
                using (var xmlReader = XmlReader.Create(new StringReader(_xmlContent)))
                {
                    // Зберігаємо у папку Documents користувача
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string outputFilePath = Path.Combine(documentsPath, "output_report.html");

                    // Трансформація
                    using (var outputWriter = new XmlTextWriter(outputFilePath, System.Text.Encoding.UTF8))
                    {
                        xslt.Transform(xmlReader, outputWriter);
                    }

                    OutputEditor.Text = $"✅ Трансформація успішно виконана!\nРезультат збережено у файл: {outputFilePath}";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка Трансформації", ex.Message, "OK");
                OutputEditor.Text = $"Помилка під час трансформації: {ex.Message}";
            }
        }

        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            // Очищення полів та скидання параметрів
            OutputEditor.Text = string.Empty;
            SearchCriteriaPicker.SelectedIndex = -1;
            SearchValueEntry.Text = string.Empty;
            _selectedCriteria = "";
            _searchValue = "";
            AnalyzeButton.IsEnabled = false;
        }
    }
}
