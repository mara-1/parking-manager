using System.Globalization;
using ClosedXML.Excel;
using Parking.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Parking.Core.Services;

// Выгрузка отчёта: Excel (ClosedXML) — для дальнейшей обработки,
// PDF (QuestPDF) — для печати и передачи руководству.
public sealed class ReportExporter : IReportExporter
{
    public const string SummarySheet = "Сводка";
    public const string CategoriesSheet = "По категориям";
    public const string DaysSheet = "По дням";
    public const string MoneyNumberFormat = "#,##0.00";

    public void ExportToExcel(PeriodReport report, string path)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrEmpty(path);

        using var workbook = new XLWorkbook();
        FillSummary(workbook.Worksheets.Add(SummarySheet), report);
        FillCategories(workbook.Worksheets.Add(CategoriesSheet), report);
        FillDays(workbook.Worksheets.Add(DaysSheet), report);
        workbook.SaveAs(path);
    }

    public void ExportToPdf(PeriodReport report, string path)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrEmpty(path);

        // Бесплатная лицензия QuestPDF для некоммерческих и учебных проектов.
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container => container.Page(page => ComposePage(page, report))).GeneratePdf(path);
    }

    private static void FillSummary(IXLWorksheet sheet, PeriodReport report)
    {
        sheet.Cell(1, 1).Value = $"Отчёт о работе парковки за {ReportText.Period(report)}";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Cell(2, 1).Value = $"Сформирован {Created(report)}";

        sheet.Cell(4, 1).Value = "Показатель";
        sheet.Cell(4, 2).Value = "Значение";
        sheet.Range(4, 1, 4, 2).Style.Font.Bold = true;

        var row = 5;
        foreach (var (label, value) in SummaryNumbers(report))
        {
            sheet.Cell(row, 1).Value = label;
            sheet.Cell(row, 2).Value = value;
            row++;
        }

        sheet.Cell(row, 1).Value = "Выручка, руб.";
        sheet.Cell(row, 2).Value = (double)report.Revenue;
        sheet.Cell(row, 2).Style.NumberFormat.Format = MoneyNumberFormat;
        row++;
        sheet.Cell(row, 1).Value = "Средняя длительность стоянки";
        sheet.Cell(row, 2).Value = ReportText.Duration(report.AverageStay);

        sheet.Column(1).Width = 36;
        sheet.Column(2).Width = 18;
    }

    private static void FillCategories(IXLWorksheet sheet, PeriodReport report)
    {
        var titles = new[] { "Категория мест", "Стоянок", "Завершено", "Выручка, руб.", "Средняя длительность" };
        for (var column = 0; column < titles.Length; column++)
        {
            sheet.Cell(1, column + 1).Value = titles[column];
            sheet.Column(column + 1).Width = column == 0 ? 26 : 18;
        }

        sheet.Range(1, 1, 1, titles.Length).Style.Font.Bold = true;

        var row = 2;
        foreach (var category in report.Categories)
        {
            sheet.Cell(row, 1).Value = category.Category.ToDisplayName();
            sheet.Cell(row, 2).Value = category.Sessions;
            sheet.Cell(row, 3).Value = category.Finished;
            sheet.Cell(row, 4).Value = (double)category.Revenue;
            sheet.Cell(row, 4).Style.NumberFormat.Format = MoneyNumberFormat;
            sheet.Cell(row, 5).Value = ReportText.Duration(category.AverageStay);
            row++;
        }

        sheet.Cell(row, 1).Value = "Итого";
        sheet.Cell(row, 2).Value = report.TotalSessions;
        sheet.Cell(row, 3).Value = report.FinishedSessions;
        sheet.Cell(row, 4).Value = (double)report.Revenue;
        sheet.Cell(row, 4).Style.NumberFormat.Format = MoneyNumberFormat;
        sheet.Cell(row, 5).Value = ReportText.Duration(report.AverageStay);
        sheet.Range(row, 1, row, titles.Length).Style.Font.Bold = true;
    }

    private static void FillDays(IXLWorksheet sheet, PeriodReport report)
    {
        sheet.Cell(1, 1).Value = "Дата";
        sheet.Cell(1, 2).Value = "Въездов";
        sheet.Cell(1, 3).Value = "Выручка, руб.";
        sheet.Range(1, 1, 1, 3).Style.Font.Bold = true;

        var row = 2;
        foreach (var day in report.Days)
        {
            sheet.Cell(row, 1).Value = day.Date;
            sheet.Cell(row, 1).Style.DateFormat.Format = "dd.MM.yyyy";
            sheet.Cell(row, 2).Value = day.Entries;
            sheet.Cell(row, 3).Value = (double)day.Revenue;
            sheet.Cell(row, 3).Style.NumberFormat.Format = MoneyNumberFormat;
            row++;
        }

        sheet.Column(1).Width = 14;
        sheet.Column(2).Width = 12;
        sheet.Column(3).Width = 16;
    }

    private static IEnumerable<(string Label, int Value)> SummaryNumbers(PeriodReport report)
    {
        yield return ("Стоянок за период", report.TotalSessions);
        yield return ("Завершено", report.FinishedSessions);
        yield return ("На парковке на момент отчёта", report.ActiveSessions);
        yield return ("Броней", report.BookingsTotal);
        yield return ("Заехали по брони", report.BookingsArrived);
        yield return ("Неявки", report.BookingsNoShow);
        yield return ("Отменены", report.BookingsCancelled);
        yield return ("Ожидаются", report.BookingsWaiting);
    }

    // Шрифт по умолчанию — Lato: он входит в пакет QuestPDF и содержит кириллицу,
    // поэтому PDF выглядит одинаково на любом компьютере.
    private static void ComposePage(PageDescriptor page, PeriodReport report)
    {
        page.Size(PageSizes.A4);
        page.Margin(1.5f, Unit.Centimetre);
        page.DefaultTextStyle(style => style.FontSize(10));
        page.Header().Element(c => ComposeHeader(c, report));
        page.Content().PaddingVertical(12).Element(c => ComposeContent(c, report));
        page.Footer().AlignCenter().Element(ComposeFooter);
    }

    private static void ComposeHeader(IContainer container, PeriodReport report)
    {
        container.Column(header =>
        {
            header.Item().Text($"Отчёт о работе парковки за {ReportText.Period(report)}").FontSize(16).SemiBold();
            header.Item().Text($"ParkingManager. Сформирован {Created(report)}").FontColor(Colors.Grey.Darken1);
        });
    }

    private static void ComposeContent(IContainer container, PeriodReport report)
    {
        container.Column(column =>
        {
            column.Spacing(10);
            AddSection(column, "Итоги", new PdfTable(
                new List<float> { 3, 2 },
                new List<string>(),
                SummaryRows(report)));
            AddSection(column, "По категориям мест", new PdfTable(
                new List<float> { 3, 2, 2, 2, 2 },
                new List<string> { "Категория мест", "Стоянок", "Завершено", "Выручка", "Ср. длительность" },
                CategoryRows(report)));
            AddSection(column, "По дням", new PdfTable(
                new List<float> { 2, 2, 3 },
                new List<string> { "Дата", "Въездов", "Выручка" },
                DayRows(report)));
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Text(text =>
        {
            text.Span("Страница ");
            text.CurrentPageNumber();
            text.Span(" из ");
            text.TotalPages();
        });
    }

    private static void AddSection(ColumnDescriptor column, string title, PdfTable table)
    {
        column.Item().Text(title).FontSize(12).SemiBold();
        column.Item().Element(c => ComposeTable(c, table));
    }

    // Таблица PDF: первый столбец — подписи слева, остальные — числа справа.
    private static void ComposeTable(IContainer container, PdfTable data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var width in data.Widths)
                {
                    columns.RelativeColumn(width);
                }
            });

            if (data.Headers.Count > 0)
            {
                table.Header(header =>
                {
                    for (var i = 0; i < data.Headers.Count; i++)
                    {
                        Align(HeaderCell(header.Cell()), i).Text(data.Headers[i]).SemiBold();
                    }
                });
            }

            foreach (var row in data.Rows)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    Align(BodyCell(table.Cell()), i).Text(row[i]);
                }
            }
        });
    }

    private static List<string[]> SummaryRows(PeriodReport report)
    {
        var rows = SummaryNumbers(report)
            .Select(n => new[] { n.Label, Count(n.Value) })
            .ToList();
        rows.Add(new[] { "Выручка", ReportText.Money(report.Revenue) });
        rows.Add(new[] { "Средняя длительность стоянки", ReportText.Duration(report.AverageStay) });
        return rows;
    }

    private static List<string[]> CategoryRows(PeriodReport report)
    {
        return report.Categories
            .Select(c => new[]
            {
                c.Category.ToDisplayName(),
                Count(c.Sessions),
                Count(c.Finished),
                ReportText.Money(c.Revenue),
                ReportText.Duration(c.AverageStay),
            })
            .ToList();
    }

    private static List<string[]> DayRows(PeriodReport report)
    {
        return report.Days
            .Select(d => new[] { ReportText.Day(d.Date), Count(d.Entries), ReportText.Money(d.Revenue) })
            .ToList();
    }

    private static IContainer Align(IContainer cell, int column)
    {
        return column == 0 ? cell : cell.AlignRight();
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingVertical(4);
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
    }

    private static string Created(PeriodReport report)
    {
        return report.CreatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    private static string Count(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    // Данные таблицы PDF: относительные ширины столбцов, заголовки (могут отсутствовать) и строки.
    private sealed class PdfTable
    {
        public PdfTable(IReadOnlyList<float> widths, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
        {
            Widths = widths;
            Headers = headers;
            Rows = rows;
        }

        public IReadOnlyList<float> Widths { get; }

        public IReadOnlyList<string> Headers { get; }

        public IReadOnlyList<string[]> Rows { get; }
    }
}
