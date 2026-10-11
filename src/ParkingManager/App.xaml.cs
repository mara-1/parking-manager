using System.IO;
using System.Windows;
using Parking.Core;
using ParkingManager.ViewModels;
using ParkingManager.Views;

namespace ParkingManager;

public partial class App : Application
{
    private CoreServices? _core;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Окно входа показывается до главного окна, поэтому приложение
        // не должно завершаться при его закрытии.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ParkingManager");
        Directory.CreateDirectory(dataFolder);
        _core = CoreServices.Create(Path.Combine(dataFolder, "parking.db"));

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
}
