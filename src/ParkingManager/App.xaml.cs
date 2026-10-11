using System.IO;
using System.Windows;
using Parking.Core;
using ParkingManager.ViewModels;
using ParkingManager.Views;

namespace ParkingManager;

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

        var parkingWindow = new ParkingWindow(new MainViewModel(_core.Auth, _core.Parking));
        MainWindow = parkingWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        parkingWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _core?.Dispose();
        base.OnExit(e);
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
