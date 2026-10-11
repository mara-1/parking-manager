using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace Parking.UiTests;

// Запущенный экземпляр приложения. Каждый тест получает собственную
// пустую бд во временном каталоге, поэтому тесты независимы
// друг от друга и не затрагивают рабочую базу оператора.
public sealed class AppSession : IDisposable
{
    public const string LoginWindowTitle = "Вход — ParkingManager";
    public const string ParkingWindowTitle = "ParkingManager — схема парковки";
    public const string DemoLogin = "admin";
    public const string DemoPassword = "admin123";
    public const string DemoOperatorName = "Демо-оператор";
    public const int DemoSpotCount = 20;

    private readonly string _dataFolder;
    private readonly UIA3Automation _automation;
    private readonly Application _application;

    public AppSession()
    {
        var appPath = UiTestSettings.AppPath;
        if (!File.Exists(appPath))
        {
            throw new FileNotFoundException("Приложение не найдено. Сначала соберите решение.", appPath);
        }

        _dataFolder = Path.Combine(Path.GetTempPath(), "ParkingManager.UiTests", $"{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dataFolder);

        var startInfo = new ProcessStartInfo(appPath)
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(appPath) ?? string.Empty,
        };
        startInfo.Environment["PARKING_DB_PATH"] = Path.Combine(_dataFolder, "parking.db");

        _automation = new UIA3Automation();
        _application = Application.Launch(startInfo);
    }

    public Window WaitForWindow(string title)
    {
        var result = Retry.WhileNull(
            () => FindWindow(title),
            UiTestSettings.Timeout,
            UiTestSettings.PollInterval,
            ignoreException: true);

        return result.Result ?? throw new InvalidOperationException($"Окно «{title}» не появилось.");
    }

    public bool WaitForWindowClosed(string title)
    {
        var result = Retry.WhileFalse(
            () => FindWindow(title) is null,
            UiTestSettings.Timeout,
            UiTestSettings.PollInterval,
            ignoreException: true);

        return result.Result;
    }

    public bool IsWindowOpen(string title)
    {
        return FindWindow(title) is not null;
    }

    // Заполняет окно входа и нажимает «Войти». Возвращает окно входа.
    public Window SubmitLogin(string login, string password, string? screenshotName = null)
    {
        var window = WaitForWindow(LoginWindowTitle);
        UiHelpers.SetText(window, "LoginBox", login);
        UiHelpers.SetText(window, "PasswordInput", password);

        if (screenshotName is not null)
        {
            UiHelpers.Screenshot(window, screenshotName);
        }

        UiHelpers.ClickWhenEnabled(window, "LoginButton");
        return window;
    }

    // Вход демо-оператором. Возвращает главное окно со схемой парковки.
    public Window LoginAsDemoOperator()
    {
        SubmitLogin(DemoLogin, DemoPassword);
        var parkingWindow = WaitForWindow(ParkingWindowTitle);
        UiHelpers.WaitForText(parkingWindow, "FreeCountText", $"{DemoSpotCount}");
        return parkingWindow;
    }

    public void Dispose()
    {
        _application.Close();
        _application.Dispose();
        _automation.Dispose();

        try
        {
            Directory.Delete(_dataFolder, recursive: true);
        }
        catch (IOException)
        {
            // Файл базы может быть ещё занят завершающимся процессом;
            // временный каталог будет очищен системой.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private Window? FindWindow(string title)
    {
        return _application.GetAllTopLevelWindows(_automation).FirstOrDefault(w => w.Title == title);
    }
}
