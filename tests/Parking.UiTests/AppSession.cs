using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace Parking.UiTests;

// Запущенный экземпляр приложения. Каждый тест получает собственную
// пустую бд во временном каталоге, поэтому тесты независимы
// друг от друга и не затрагивают рабочую базу оператора.
// В пустой базе нет оператора, поэтому приложение открывается
// окном регистрации — как при первом запуске на рабочей станции.
public sealed class AppSession : IDisposable
{
    public const string RegistrationWindowTitle = "Регистрация оператора — ParkingManager";
    public const string LoginWindowTitle = "Вход — ParkingManager";
    public const string ParkingWindowTitle = "ParkingManager — учёт парковки";
    public const string OperatorLogin = "operator";
    public const string OperatorPassword = "secret1";
    public const string OperatorName = "Иванова Анна Сергеевна";
    public const int RegularCapacity = 450;
    public const int DisabledCapacity = 50;

    private readonly string _dataFolder;
    private readonly UIA3Automation _automation;
    private Application _application;

    public AppSession()
    {
        var appPath = UiTestSettings.AppPath;
        if (!File.Exists(appPath))
        {
            throw new FileNotFoundException("Приложение не найдено. Сначала соберите решение.", appPath);
        }

        _dataFolder = Path.Combine(Path.GetTempPath(), "ParkingManager.UiTests", $"{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dataFolder);

        _automation = new UIA3Automation();
        _application = Launch();
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

    // Заполняет окно регистрации и нажимает «Зарегистрироваться».
    // Возвращает окно регистрации.
    public Window SubmitRegistration(string confirmPassword, string? screenshotName = null)
    {
        var window = WaitForWindow(RegistrationWindowTitle);
        UiHelpers.SetText(window, "FullNameBox", OperatorName);
        UiHelpers.SetText(window, "NewLoginBox", OperatorLogin);
        UiHelpers.SetText(window, "NewPasswordInput", OperatorPassword);
        UiHelpers.SetText(window, "ConfirmPasswordInput", confirmPassword);

        if (screenshotName is not null)
        {
            UiHelpers.Screenshot(window, screenshotName);
        }

        UiHelpers.ClickWhenEnabled(window, "RegisterButton");
        return window;
    }

    // Регистрация оператора при первом запуске. Возвращает главное окно со схемой парковки.
    public Window RegisterOperator()
    {
        SubmitRegistration(OperatorPassword);
        return WaitForParkingWindow();
    }

    // Закрывает приложение и запускает его снова с той же базой данных.
    public void Restart()
    {
        _application.Close();
        Retry.WhileFalse(() => _application.HasExited, UiTestSettings.Timeout, UiTestSettings.PollInterval, ignoreException: true);
        _application.Dispose();
        _application = Launch();
    }

    // Ждёт главное окно и разворачивает его.
    public Window WaitForParkingWindow()
    {
        var parkingWindow = WaitForWindow(ParkingWindowTitle);
        UiHelpers.WaitForText(parkingWindow, "RegularFreeText", $"{RegularCapacity}");

        // Экран раннера CI (1024 × 768) меньше окна: разворачиваем его,
        // чтобы снимки экрана для документации были целыми.
        parkingWindow.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Maximized);
        Wait.UntilInputIsProcessed();
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

    private Application Launch()
    {
        var appPath = UiTestSettings.AppPath;
        var startInfo = new ProcessStartInfo(appPath)
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(appPath) ?? string.Empty,
        };
        startInfo.Environment["PARKING_DB_PATH"] = Path.Combine(_dataFolder, "parking.db");
        return Application.Launch(startInfo);
    }

    private Window? FindWindow(string title)
    {
        return _application.GetAllTopLevelWindows(_automation).FirstOrDefault(w => w.Title == title);
    }
}
