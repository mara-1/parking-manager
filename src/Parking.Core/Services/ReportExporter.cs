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

        Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(style => style.FontSize(10));

            page.Header().Column(header =>
            {
                header.Item().Text($"Отчёт о работе парковки за {ReportText.Period(report)}").FontSize(16).SemiBold();
                header.Item().Text($"ParkingManager. Сформирован {Created(report)}").FontColor(Colors.Grey.Darken1);
            });

            page.Content().PaddingVertical(12).Column(column =>
            {
                column.Spacing(10);
                column.Item().Text("Итоги").FontSize(12).SemiBold();
                column.Item().Element(c => ComposeSummary(c, report));
                column.Item().Text("По категориям мест").FontSize(12).SemiBold();
                column.Item().Element(c => ComposeCategories(c, report));
                column.Item().Text("По дням").FontSize(12).SemiBold();
                column.Item().Element(c => ComposeDays(c, report));
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Страница ");
                text.CurrentPageNumber();
                text.Span(" из ");
                text.TotalPages();
            });
        })).GeneratePdf(path);
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
        string[] titles = ["Категория мест", "Стоянок", "Завершено", "Выручка, руб.", "Средняя длительность"];
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

    private static void ComposeSummary(IContainer container, PeriodReport report)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            foreach (var (label, value) in SummaryNumbers(report))
            {
                table.Cell().Element(BodyCell).Text(label);
                table.Cell().Element(BodyCell).AlignRight().Text(value.ToString(CultureInfo.InvariantCulture));
            }

            table.Cell().Element(BodyCell).Text("Выручка");
            table.Cell().Element(BodyCell).AlignRight().Text(ReportText.Money(report.Revenue)).SemiBold();
            table.Cell().Element(BodyCell).Text("Средняя длительность стоянки");
            table.Cell().Element(BodyCell).AlignRight().Text(ReportText.Duration(report.AverageStay));
        });
    }

    private static void ComposeCategories(IContainer container, PeriodReport report)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Категория мест").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Стоянок").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Завершено").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Выручка").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Ср. длительность").SemiBold();
            });

            foreach (var category in report.Categories)
            {
                table.Cell().Element(BodyCell).Text(category.Category.ToDisplayName());
                table.Cell().Element(BodyCell).AlignRight().Text(Count(category.Sessions));
                table.Cell().Element(BodyCell).AlignRight().Text(Count(category.Finished));
                table.Cell().Element(BodyCell).AlignRight().Text(ReportText.Money(category.Revenue));
                table.Cell().Element(BodyCell).AlignRight().Text(ReportText.Duration(category.AverageStay));
            }
        });
    }

    private static void ComposeDays(IContainer container, PeriodReport report)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Дата").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Въездов").SemiBold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Выручка").SemiBold();
            });

            foreach (var day in report.Days)
            {
                table.Cell().Element(BodyCell).Text(ReportText.Day(day.Date));
                table.Cell().Element(BodyCell).AlignRight().Text(Count(day.Entries));
                table.Cell().Element(BodyCell).AlignRight().Text(ReportText.Money(day.Revenue));
            }
        });
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
}
