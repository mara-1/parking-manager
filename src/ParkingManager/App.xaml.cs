using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Parking.Core;
using ParkingManager.Services;
using ParkingManager.ViewModels;
using ParkingManager.Views;

namespace ParkingManager;

[ExcludeFromCodeCoverage(Justification = "Запуск приложения и окон: проверяется UI-тестами (FlaUI), покрытие которых не собирается.")]
public partial class App : Application
{
    // Переменная окружения с путём к файлу базы данных. Используется
    // автоматизированными UI-тестами, чтобы каждый тест работал
    // с отдельной чистой базой и не затрагивал рабочие данные.
    public const string DatabasePathVariable = "PARKING_DB_PATH";

    private CoreServices? _core;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        UseRussianFormats();

        // Окно входа показывается до главного окна, поэтому приложение
        // не должно завершаться при его закрытии.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _core = CoreServices.Create(GetDatabasePath());

        var loginWindow = new LoginWindow(new LoginViewModel(_core.Auth));
        if (loginWindow.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        var reports = new ReportsViewModel(_core.Reports, _core.Exporter, new SaveFileDialogService(), TimeProvider.System);
        var mainViewModel = new MainViewModel(_core.Auth, _core.Parking, _core.Settings, _core.Bookings, reports, TimeProvider.System);
        var parkingWindow = new ParkingWindow(mainViewModel);
        MainWindow = parkingWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        parkingWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _core?.Dispose();
        base.OnExit(e);
    }

    // Даты и числа в окнах (в том числе в календаре DatePicker) показываются
    // в русском формате независимо от языка Windows.
    private static void UseRussianFormats()
    {
        var russian = CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.DefaultThreadCurrentCulture = russian;
        CultureInfo.DefaultThreadCurrentUICulture = russian;
        Thread.CurrentThread.CurrentCulture = russian;
        Thread.CurrentThread.CurrentUICulture = russian;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(russian.IetfLanguageTag)));
    }

    private static string GetDatabasePath()
    {
        var databasePath = Environment.GetEnvironmentVariable(DatabasePathVariable);
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ParkingManager");
            databasePath = Path.Combine(dataFolder, "parking.db");
        }

        var folder = Path.GetDirectoryName(Path.GetFullPath(databasePath));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return databasePath;
    }
}
