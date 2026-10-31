using ParkingManager.Services;

namespace ParkingManager.Tests.Fakes;

// Вместо окна «Сохранить как» возвращает заранее заданный путь (null — «Отмена»).
public sealed class FakeSaveFileDialog : ISaveFileDialog
{
    private readonly string? _path;

    public FakeSaveFileDialog(string? path)
    {
        _path = path;
    }

    public string? LastFileName { get; private set; }

    public string? LastFilter { get; private set; }

    public string? AskPath(string defaultFileName, string filter)
    {
        LastFileName = defaultFileName;
        LastFilter = filter;
        return _path;
    }
}
