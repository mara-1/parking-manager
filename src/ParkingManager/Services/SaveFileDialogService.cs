using Microsoft.Win32;

namespace ParkingManager.Services;

// Стандартное окно Windows «Сохранить как».
public sealed class SaveFileDialogService : ISaveFileDialog
{
    public string? AskPath(string defaultFileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            Filter = filter,
            AddExtension = true,
            OverwritePrompt = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
