namespace Parking.UiTests;

public static class UiTestSettings
{
    // Путь к приложению можно задать явно (например, к опубликованной сборке)
    public const string AppPathVariable = "PARKING_APP_PATH";

    // Каталог для снимков экрана; в CI задаётся, чтобы сохранить их в артефактах
    public const string ScreenshotsVariable = "UI_SCREENSHOTS_DIR";

    public static TimeSpan Timeout => TimeSpan.FromSeconds(20);

    public static TimeSpan PollInterval => TimeSpan.FromMilliseconds(200);

    public static string ScreenshotsDirectory
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable(ScreenshotsVariable);
            return string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(AppContext.BaseDirectory, "screenshots")
                : configured;
        }
    }

    // По умолчанию используется сборка приложения той же конфигурации
    // (Debug/Release), что и сборка тестов: src/ParkingManager/bin/<конфигурация>/<платформа>
    public static string AppPath
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable(AppPathVariable);
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            var testDirectory = new DirectoryInfo(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory));
            var framework = testDirectory.Name;
            var configuration = testDirectory.Parent?.Name ?? "Debug";

            var root = testDirectory;
            while (root is not null && !File.Exists(Path.Combine(root.FullName, "ParkingManager.sln")))
            {
                root = root.Parent;
            }

            if (root is null)
            {
                throw new InvalidOperationException("Не найден корень репозитория (файл ParkingManager.sln).");
            }

            return Path.Combine(root.FullName, "src", "ParkingManager", "bin", configuration, framework, "ParkingManager.exe");
        }
    }
}
