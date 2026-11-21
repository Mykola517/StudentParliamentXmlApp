using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using StudentParliamentXmlApp.Models;
using StudentParliamentXmlApp.Services;
using StudentParliamentXmlApp.Strategies;

namespace StudentParliamentXmlApp
{
    public partial class MainPage : ContentPage
    {
        private static readonly FilePickerFileType XmlType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".xml" } },
            { DevicePlatform.Android, new[] { "text/xml", "application/xml" } },
            { DevicePlatform.iOS, new[] { "public.xml" } },
            { DevicePlatform.MacCatalyst, new[] { "public.xml" } },
        });

        private static readonly FilePickerFileType XslType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".xsl", ".xslt" } },
            { DevicePlatform.Android, new[] { "text/xml", "application/xml" } },
            { DevicePlatform.iOS, new[] { "public.xml" } },
            { DevicePlatform.MacCatalyst, new[] { "public.xml" } },
        });

        private readonly XmlAnalyzerService _analyzer = new();
        private readonly HtmlTransformService _transformer = new();
        private readonly SaxAnalysisStrategy _sax = new();
        private readonly DomAnalysisStrategy _dom = new();
        private readonly LinqAnalysisStrategy _linq = new();

        private string? _xmlPath;
        private string? _xslPath;
        private List<ParliamentEvent>? _results;

        public MainPage()
        {
            InitializeComponent();
            ResetDates();
        }

        private async void OnOpenXmlClicked(object? sender, EventArgs e)
        {
            try
            {
                var file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Оберіть XML-файл",
                    FileTypes = XmlType
                });
                if (file == null) return;

                ApplySelectedStrategy();
                var all = _analyzer.Analyze(file.FullPath, new SearchCriteria());
                var attributes = _analyzer.GetAllAttributes(file.FullPath);

                _xmlPath = file.FullPath;
                _results = null;
                FacultyPicker.ItemsSource = attributes["Faculty"];
                DepartmentPicker.ItemsSource = attributes["Department"];
                SpecialtyPicker.ItemsSource = attributes["Specialty"];
                EventTypePicker.ItemsSource = attributes["EventType"];
                ResultsCollectionView.ItemsSource = null;
                QueryLabel.Text = "";

                var counts = all.Select(x => x.AttributeCount).Distinct().OrderBy(x => x);
                FileStatusLabel.Text = $"Файл: {file.FileName}\nЗаписів Event: {all.Count}\nАтрибутів у вузлах: {string.Join(", ", counts)}";
                StatusLabel.Text = "XML завантажено, списки параметрів заповнено.";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Не вдалося відкрити файл: {ex.Message}", "ОК");
            }
        }

        private async void OnOpenXslClicked(object? sender, EventArgs e)
        {
            try
            {
                await PickXslAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", ex.Message, "ОК");
            }
        }

        private async Task PickXslAsync()
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XSL-шаблон",
                FileTypes = XslType
            });
            if (file == null) return;

            _xslPath = file.FullPath;
            XslStatusLabel.Text = $"XSL: {file.FileName}";
        }

        private async void OnSearchClicked(object? sender, EventArgs e)
        {
            if (_xmlPath == null)
            {
                await DisplayAlert("Увага", "Спочатку відкрийте XML-файл.", "ОК");
                return;
            }
            if (PeriodCheck.IsChecked && FromPicker.Date > ToPicker.Date)
            {
                await DisplayAlert("Помилка", "Початок періоду пізніше за його кінець.", "ОК");
                return;
            }

            try
            {
                ApplySelectedStrategy();
                var criteria = BuildCriteria();

                _results = _analyzer.Analyze(_xmlPath, criteria);
                ResultsCollectionView.ItemsSource = _results;
                QueryLabel.Text = _analyzer.DescribeQuery(criteria);
                StatusLabel.Text = $"Знайдено: {_results.Count}";

                if (_results.Count == 0)
                    await DisplayAlert("Результат", "За заданими критеріями нічого не знайдено.", "ОК");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Помилка аналізу XML: {ex.Message}", "ОК");
            }
        }

        private async void OnTransformClicked(object? sender, EventArgs e)
        {
            if (_xmlPath == null)
            {
                await DisplayAlert("Увага", "Спочатку відкрийте XML-файл.", "ОК");
                return;
            }

            try
            {
                if (_xslPath == null) await PickXslAsync();
                if (_xslPath == null) return;

                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StudentParliament");
                Directory.CreateDirectory(folder);
                var outputPath = Path.Combine(folder, "result.html");

                if (_results != null)
                    _transformer.TransformResults(_results, _xslPath, outputPath);
                else
                    _transformer.Transform(_xmlPath, _xslPath, outputPath);

                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
                StatusLabel.Text = $"HTML збережено і відкрито в браузері: {outputPath}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Помилка трансформації: {ex.Message}", "ОК");
            }
        }

        private void OnClearClicked(object? sender, EventArgs e)
        {
            KeywordEntry.Text = string.Empty;
            FacultyPicker.SelectedItem = null;
            DepartmentPicker.SelectedItem = null;
            SpecialtyPicker.SelectedItem = null;
            EventTypePicker.SelectedItem = null;
            PeriodCheck.IsChecked = false;
            ResetDates();

            _results = null;
            ResultsCollectionView.ItemsSource = null;
            QueryLabel.Text = string.Empty;
            StatusLabel.Text = "Поле виводу та параметри пошуку очищено.";
        }

        private async void OnExitClicked(object? sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
            if (answer)
            {
                Application.Current?.Quit();
            }
        }

        private async void OnHelpClicked(object? sender, EventArgs e)
        {
            await DisplayAlert("Довідка",
                "1.Оберіть XML - файл та метод аналізу(SAX, DOM або LINQ).\n" +
    "2. Задайте потрібні фільтри і натисніть \"Знайти\".\n" +
    "3. Щоб зберегти таблицю, оберіть XSL-файл і натисніть \"Трансформувати в HTML\".\n" +
    "4. Кнопка \"Очистити\" скидає всі поля та результати.",
                "ОК");
        }

        private async void OnAboutClicked(object? sender, EventArgs e)
        {
            await DisplayAlert("Про програму",
                "Додаток «Студентський парламент»\nЛабораторна робота № 2 (варіант 28)\n\n" +
                "Розробник: Друкачук Микола Володимирович",
                "ОК");
        }

        private void ApplySelectedStrategy()
        {
            if (DomRadio.IsChecked) _analyzer.SetStrategy(_dom);
            else if (LinqRadio.IsChecked) _analyzer.SetStrategy(_linq);
            else _analyzer.SetStrategy(_sax);
        }

        private SearchCriteria BuildCriteria()
        {
            return new SearchCriteria
            {
                Keyword = string.IsNullOrWhiteSpace(KeywordEntry.Text) ? null : KeywordEntry.Text.Trim(),
                Faculty = FacultyPicker.SelectedItem?.ToString(),
                Department = DepartmentPicker.SelectedItem?.ToString(),
                Specialty = SpecialtyPicker.SelectedItem?.ToString(),
                EventType = EventTypePicker.SelectedItem?.ToString(),
                From = PeriodCheck.IsChecked ? FromPicker.Date : null,
                To = PeriodCheck.IsChecked ? ToPicker.Date : null
            };
        }

        private void ResetDates()
        {
            FromPicker.Date = new DateTime(2025, 9, 1);
            ToPicker.Date = new DateTime(2026, 6, 30);
        }
    }
}


